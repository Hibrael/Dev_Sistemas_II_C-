//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services
{
    /// <summary>
    /// Casos de uso dos acessos de colaborador, incluindo a catraca (entrada e saída).
    ///
    /// A catraca recebe apenas o Id do colaborador e carrega a entidade pelo repositório de
    /// colaboradores — ver o comentário equivalente em AcessoAlunoService.
    ///
    /// "Dia" aqui é o dia local (de meia-noite a meia-noite no fuso da máquina); como os acessos
    /// são gravados em UTC, os limites do dia são convertidos para UTC antes da consulta.
    /// </summary>
    public sealed class AcessoColaboradorService : IAcessoColaboradorService
    {
        private readonly Func<IAcessoColaboradorRepository> _repoFactory;
        private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;

        public AcessoColaboradorService(Func<IAcessoColaboradorRepository> repoFactory, Func<IColaboradorRepository> colaboradorRepoFactory)
        {
            _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
            _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
        }

        public async Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador do acesso do colaborador deve ser maior que zero.");

            var repository = _repoFactory();
            var entity = await repository.ObterPorId(id, cancellationToken);
            return entity?.ToDto();
        }

        public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var repository = _repoFactory();
            var entities = await repository.ObterTodos(cancellationToken);
            return entities.Select(entity => entity.ToDto()).ToList();
        }

        public async Task<IEnumerable<AcessoColaboradorDto>> ObterEmAbertoAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _repoFactory().ObterEmAberto(cancellationToken);
            return entities.Select(entity => entity.ToDto()).ToList();
        }

        public async Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = dto.ToEntity();
            var repository = _repoFactory();
            var result = await repository.Adicionar(entity, cancellationToken);
            return result.ToDto();
        }

        public async Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = dto.ToEntity();
            var repository = _repoFactory();
            var result = await repository.Atualizar(entity, cancellationToken);
            return result.ToDto();
        }

        public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador do acesso do colaborador deve ser maior que zero.");

            var repository = _repoFactory();
            return await repository.Remover(id, cancellationToken);
        }

        public async Task<RegistroAcessoResultadoDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default)
        {
            var colaborador = await ObterColaboradorAsync(colaboradorId, cancellationToken);
            var acessoRepo = _repoFactory();

            var emAberto = await acessoRepo.ObterEmAbertoPorColaborador(colaborador.Id, cancellationToken);
            if (emAberto is not null)
                return Negado(colaborador, $"O colaborador já está na academia: entrou em {ParaLocal(emAberto.DataHora):dd/MM/yyyy HH:mm} e ainda não registrou a saída.");

            // Regra: não pode entrar quem já cumpriu (ou passou) a jornada do dia somando todas as idas.
            var agora = DateTime.UtcNow;
            var limite = colaborador.JornadaDiariaMaxima;
            var acessosDoDia = await ObterAcessosDoDiaAsync(acessoRepo, colaborador.Id, cancellationToken);
            var totalDoDia = AcessoColaborador.SomarPermanencia(acessosDoDia, agora);

            if (totalDoDia >= limite)
                return Negado(colaborador,
                    $"Jornada diária de {RegistroAcessoResultadoDto.FormatarDuracao(limite)} ({DescreverVinculo(colaborador)}) já atingida: " +
                    $"hoje o colaborador já permaneceu {RegistroAcessoResultadoDto.FormatarDuracao(totalDoDia)} na academia.",
                    totalDoDia, limite);

            var acesso = await acessoRepo.Adicionar(AcessoColaborador.Criar(0, colaborador), cancellationToken);
            var restante = limite - totalDoDia;

            return new RegistroAcessoResultadoDto
            {
                Permitido = true,
                Titulo = "Entrada liberada",
                Mensagem = $"Bom trabalho, {colaborador.Nome}! Já cumpridas hoje: {RegistroAcessoResultadoDto.FormatarDuracao(totalDoDia)}; " +
                           $"ainda pode permanecer {RegistroAcessoResultadoDto.FormatarDuracao(restante)} (limite de {RegistroAcessoResultadoDto.FormatarDuracao(limite)} — {DescreverVinculo(colaborador)}).",
                PessoaId = colaborador.Id,
                NomePessoa = colaborador.Nome,
                AcessoId = acesso.Id,
                Entrada = ParaLocal(acesso.DataHora),
                PermanenciaNoDia = totalDoDia,
                JornadaDiariaMaxima = limite,
                JornadaRestante = restante
            };
        }

        public async Task<RegistroAcessoResultadoDto> RegistrarSaidaAsync(int colaboradorId, CancellationToken cancellationToken = default)
        {
            var colaborador = await ObterColaboradorAsync(colaboradorId, cancellationToken);
            var acessoRepo = _repoFactory();

            // A saída fecha o acesso em aberto do próprio colaborador: assim é sempre o mesmo que entrou.
            var acesso = await acessoRepo.ObterEmAbertoPorColaborador(colaborador.Id, cancellationToken);
            if (acesso is null)
                return Negado(colaborador, "Não há entrada em aberto para este colaborador: registre a entrada antes da saída.", titulo: "Saída não registrada");

            acesso.RegistrarSaida();
            await acessoRepo.Atualizar(acesso, cancellationToken);

            // Regra: a permanência exibida é a soma de todos os registros do dia, já com esta saída.
            // O acesso recém-fechado entra na lista mesmo que a entrada tenha sido em outro dia
            // (ex.: saída depois da meia-noite), e substitui a versão lida antes do fechamento.
            var acessosDoDia = (await ObterAcessosDoDiaAsync(acessoRepo, colaborador.Id, cancellationToken))
                .Where(a => a.Id != acesso.Id)
                .Append(acesso)
                .ToList();

            var limite = colaborador.JornadaDiariaMaxima;
            var totalDoDia = AcessoColaborador.SomarPermanencia(acessosDoDia, acesso.DataHoraSaida!.Value);
            var ultrapassou = totalDoDia > limite;
            var permanencia = acesso.Permanencia!.Value;

            var mensagem = $"Até logo, {colaborador.Nome}! Nesta ida: {RegistroAcessoResultadoDto.FormatarDuracao(permanencia)}. " +
                           $"Total do dia ({acessosDoDia.Count} registro(s)): {RegistroAcessoResultadoDto.FormatarDuracao(totalDoDia)} de {RegistroAcessoResultadoDto.FormatarDuracao(limite)}.";
            if (ultrapassou)
                mensagem += $" Atenção: a jornada diária ({DescreverVinculo(colaborador)}) foi ultrapassada em {RegistroAcessoResultadoDto.FormatarDuracao(totalDoDia - limite)}.";

            return new RegistroAcessoResultadoDto
            {
                Permitido = true,
                Titulo = "Saída registrada",
                Mensagem = mensagem,
                PessoaId = colaborador.Id,
                NomePessoa = colaborador.Nome,
                AcessoId = acesso.Id,
                Entrada = ParaLocal(acesso.DataHora),
                Saida = ParaLocal(acesso.DataHoraSaida.Value),
                Permanencia = permanencia,
                PermanenciaNoDia = totalDoDia,
                JornadaDiariaMaxima = limite,
                JornadaRestante = ultrapassou ? TimeSpan.Zero : limite - totalDoDia,
                UltrapassouJornada = ultrapassou
            };
        }

        // Acessos com entrada no dia local de hoje, consultados pelos limites convertidos para UTC.
        private static async Task<List<AcessoColaborador>> ObterAcessosDoDiaAsync(IAcessoColaboradorRepository repository, int colaboradorId, CancellationToken cancellationToken)
        {
            var inicioDoDiaUtc = DateTime.Today.ToUniversalTime();
            var fimDoDiaUtc = DateTime.Today.AddDays(1).ToUniversalTime();
            return (await repository.ObterPorColaboradorNoPeriodo(colaboradorId, inicioDoDiaUtc, fimDoDiaUtc, cancellationToken)).ToList();
        }

        private async Task<Colaborador> ObterColaboradorAsync(int colaboradorId, CancellationToken cancellationToken)
        {
            if (colaboradorId <= 0)
                throw new ArgumentOutOfRangeException(nameof(colaboradorId), "O identificador do colaborador deve ser maior que zero.");

            return await _colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken)
                ?? throw new KeyNotFoundException($"Colaborador com ID {colaboradorId} não encontrado.");
        }

        private static string DescreverVinculo(Colaborador colaborador) => colaborador.Vinculo switch
        {
            Domain.Enums.ColaboradorVinculo.Estagio => "estágio",
            Domain.Enums.ColaboradorVinculo.CLT => "CLT",
            Domain.Enums.ColaboradorVinculo.PJ => "PJ",
            _ => colaborador.Vinculo.ToString()
        };

        private static RegistroAcessoResultadoDto Negado(Colaborador colaborador, string motivo, TimeSpan? totalDoDia = null, TimeSpan? limite = null, string titulo = "Acesso negado") => new()
        {
            Permitido = false,
            Titulo = titulo,
            Mensagem = motivo,
            PessoaId = colaborador.Id,
            NomePessoa = colaborador.Nome,
            PermanenciaNoDia = totalDoDia,
            JornadaDiariaMaxima = limite
        };

        // Os instantes são gravados em UTC (ver AcessoColaborador); o banco devolve sem Kind.
        private static DateTime ParaLocal(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
    }
}
