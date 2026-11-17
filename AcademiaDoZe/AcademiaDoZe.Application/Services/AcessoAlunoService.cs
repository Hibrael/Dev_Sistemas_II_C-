//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services
{
    /// <summary>
    /// Casos de uso dos acessos de aluno, incluindo a catraca (entrada e saída).
    ///
    /// A catraca recebe apenas o Id do aluno e carrega a entidade pelo repositório de alunos:
    /// o caminho por DTO (AdicionarAsync) exige a senha em texto plano para reconstruir o Aluno,
    /// e a apresentação nunca tem essa senha — os DTOs de saída não a carregam.
    /// </summary>
    public sealed class AcessoAlunoService : IAcessoAlunoService
    {
        private readonly Func<IAcessoAlunoRepository> _repoFactory;
        private readonly Func<IAlunoRepository> _alunoRepoFactory;
        private readonly Func<IMatriculaRepository> _matriculaRepoFactory;

        public AcessoAlunoService(Func<IAcessoAlunoRepository> repoFactory, Func<IAlunoRepository> alunoRepoFactory, Func<IMatriculaRepository> matriculaRepoFactory)
        {
            _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
            _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
            _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
        }

        public async Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador do acesso do aluno deve ser maior que zero.");

            var repository = _repoFactory();
            var entity = await repository.ObterPorId(id, cancellationToken);
            return entity?.ToDto();
        }

        public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var repository = _repoFactory();
            var entities = await repository.ObterTodos(cancellationToken);
            return entities.Select(entity => entity.ToDto()).ToList();
        }

        public async Task<IEnumerable<AcessoAlunoDto>> ObterEmAbertoAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _repoFactory().ObterEmAberto(cancellationToken);
            return entities.Select(entity => entity.ToDto()).ToList();
        }

        public async Task<AcessoAlunoDto> AdicionarAsync(AcessoAlunoDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = dto.ToEntity();
            var repository = _repoFactory();
            var result = await repository.Adicionar(entity, cancellationToken);
            return result.ToDto();
        }

        public async Task<AcessoAlunoDto> AtualizarAsync(AcessoAlunoDto dto, CancellationToken cancellationToken = default)
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
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador do acesso do aluno deve ser maior que zero.");

            var repository = _repoFactory();
            return await repository.Remover(id, cancellationToken);
        }

        public async Task<RegistroAcessoResultadoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default)
        {
            var aluno = await ObterAlunoAsync(alunoId, cancellationToken);

            // Regra: só entra quem tem matrícula ativa. O repositório considera ativa a matrícula
            // com data_fim >= hoje; aqui também se exige que o plano já tenha começado.
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var matricula = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(aluno.Id, cancellationToken);

            if (matricula is null)
                return Negado(aluno, "O aluno não possui matrícula ativa.");

            if (matricula.DataInicio > hoje)
                return Negado(aluno, $"A matrícula do aluno só começa em {matricula.DataInicio:dd/MM/yyyy}.");

            var acessoRepo = _repoFactory();
            var emAberto = await acessoRepo.ObterEmAbertoPorAluno(aluno.Id, cancellationToken);
            if (emAberto is not null)
                return Negado(aluno, $"O aluno já está na academia: entrou em {ParaLocal(emAberto.DataHora):dd/MM/yyyy HH:mm} e ainda não registrou a saída.");

            var acesso = await acessoRepo.Adicionar(AcessoAluno.Criar(0, aluno), cancellationToken);

            var diasRestantes = matricula.DataFim.DayNumber - hoje.DayNumber;
            var textoPlano = diasRestantes == 0
                ? $"Hoje é o último dia do plano ({matricula.DataFim:dd/MM/yyyy})."
                : $"Restam {diasRestantes} dia(s) de plano, até {matricula.DataFim:dd/MM/yyyy}.";

            return new RegistroAcessoResultadoDto
            {
                Permitido = true,
                Titulo = "Entrada liberada",
                Mensagem = $"Bem-vindo(a), {aluno.Nome}! {textoPlano}",
                PessoaId = aluno.Id,
                NomePessoa = aluno.Nome,
                AcessoId = acesso.Id,
                Entrada = ParaLocal(acesso.DataHora),
                FimPlano = matricula.DataFim,
                DiasRestantesPlano = diasRestantes
            };
        }

        public async Task<RegistroAcessoResultadoDto> RegistrarSaidaAsync(int alunoId, CancellationToken cancellationToken = default)
        {
            var aluno = await ObterAlunoAsync(alunoId, cancellationToken);

            // A saída fecha o acesso em aberto do próprio aluno: assim é sempre o mesmo aluno que entrou.
            var acessoRepo = _repoFactory();
            var acesso = await acessoRepo.ObterEmAbertoPorAluno(aluno.Id, cancellationToken);
            if (acesso is null)
                return Negado(aluno, "Não há entrada em aberto para este aluno: registre a entrada antes da saída.", "Saída não registrada");

            acesso.RegistrarSaida();
            await acessoRepo.Atualizar(acesso, cancellationToken);

            var permanencia = acesso.Permanencia!.Value;
            return new RegistroAcessoResultadoDto
            {
                Permitido = true,
                Titulo = "Saída registrada",
                Mensagem = $"Até logo, {aluno.Nome}! Você permaneceu {RegistroAcessoResultadoDto.FormatarDuracao(permanencia)} na academia.",
                PessoaId = aluno.Id,
                NomePessoa = aluno.Nome,
                AcessoId = acesso.Id,
                Entrada = ParaLocal(acesso.DataHora),
                Saida = ParaLocal(acesso.DataHoraSaida!.Value),
                Permanencia = permanencia
            };
        }

        private async Task<Aluno> ObterAlunoAsync(int alunoId, CancellationToken cancellationToken)
        {
            if (alunoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(alunoId), "O identificador do aluno deve ser maior que zero.");

            return await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken)
                ?? throw new KeyNotFoundException($"Aluno com ID {alunoId} não encontrado.");
        }

        private static RegistroAcessoResultadoDto Negado(Aluno aluno, string motivo, string titulo = "Acesso negado") => new()
        {
            Permitido = false,
            Titulo = titulo,
            Mensagem = motivo,
            PessoaId = aluno.Id,
            NomePessoa = aluno.Nome
        };

        // Os instantes são gravados em UTC (ver AcessoAluno); o banco devolve sem Kind.
        private static DateTime ParaLocal(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
    }
}
