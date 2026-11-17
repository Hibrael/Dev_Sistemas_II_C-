//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests
{
    /// <summary>
    /// Catraca: entrada (acesso em aberto), saída (data_hora_saida) e a consulta por período usada
    /// para somar a permanência do dia do colaborador.
    /// </summary>
    public class AcessoInfrastructureTests : TestBase
    {
        private readonly LogradouroRepository logradouroRepository;
        private readonly AlunoRepository alunoRepository;
        private readonly ColaboradorRepository colaboradorRepository;
        private readonly AcessoAlunoRepository acessoAlunoRepository;
        private readonly AcessoColaboradorRepository acessoColaboradorRepository;

        public AcessoInfrastructureTests()
        {
            logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
            alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
            colaboradorRepository = new ColaboradorRepository(ConnectionString, DatabaseType);
            acessoAlunoRepository = new AcessoAlunoRepository(ConnectionString, DatabaseType);
            acessoColaboradorRepository = new AcessoColaboradorRepository(ConnectionString, DatabaseType);
        }

        private async Task<Logradouro> InserirLogradouro() =>
            await logradouroRepository.Adicionar(Logradouro.Criar(0, GerarCep(), "Hibrael Andre", "Cidade Xavier", NomeSgbdAtual, "SC", "Brasil").Value!);

        private async Task<Aluno> InserirAluno()
        {
            var result = Aluno.Criar(0, "Hibrael Andre Cidade Xavier", GerarCpf(), new DateOnly(2000, 3, 20), GerarTelefone(), GerarEmail(),
                await InserirLogradouro(), "100", "Cidade Xavier", GerarSenha(), Arquivo.Criar("foto.jpg", [1, 2, 3]).Value!);
            Assert.True(result.IsSuccess);
            return await alunoRepository.Adicionar(result.Value!);
        }

        private async Task<Colaborador> InserirColaborador()
        {
            var result = Colaborador.Criar(0, "Hibrael Andre Cidade Xavier", GerarCpf(), new DateOnly(1995, 5, 15), GerarTelefone(), GerarEmail(),
                await InserirLogradouro(), "100", "Cidade Xavier", GerarSenha(), Arquivo.Criar("foto.jpg", [1, 2, 3]).Value!,
                DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Instrutor, ColaboradorVinculo.Estagio, 1500m);
            Assert.True(result.IsSuccess);
            return await colaboradorRepository.Adicionar(result.Value!);
        }

        [Fact]
        public async Task EntradaFicaEmAbertoESaidaFechaOAcessoDoAluno()
        {
            var aluno = await InserirAluno();

            var acesso = await acessoAlunoRepository.Adicionar(AcessoAluno.Criar(0, aluno));
            var aberto = await acessoAlunoRepository.ObterEmAbertoPorAluno(aluno.Id);
            Assert.NotNull(aberto);
            Assert.Equal(acesso.Id, aberto.Id);
            Assert.Contains(await acessoAlunoRepository.ObterEmAberto(), a => a.Id == acesso.Id);

            aberto.RegistrarSaida(aberto.DataHora.AddMinutes(75));
            await acessoAlunoRepository.Atualizar(aberto);

            Assert.Null(await acessoAlunoRepository.ObterEmAbertoPorAluno(aluno.Id));
            var fechado = await acessoAlunoRepository.ObterPorId(acesso.Id);
            Assert.NotNull(fechado);
            Assert.False(fechado.EmAberto);
            Assert.Equal(75, (int)Math.Round(fechado.Permanencia!.Value.TotalMinutes));
        }

        [Fact]
        public async Task ObterPorColaboradorNoPeriodoTrazTodosOsRegistrosDoDia()
        {
            var colaborador = await InserirColaborador();
            var dia = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            // dois registros no dia, um no dia anterior e um no seguinte (fora do período)
            await acessoColaboradorRepository.Adicionar(AcessoColaborador.Restaurar(0, colaborador, dia.AddHours(8), dia.AddHours(11)));
            await acessoColaboradorRepository.Adicionar(AcessoColaborador.Restaurar(0, colaborador, dia.AddHours(13), dia.AddHours(15)));
            await acessoColaboradorRepository.Adicionar(AcessoColaborador.Restaurar(0, colaborador, dia.AddHours(-3), dia.AddHours(-1)));
            await acessoColaboradorRepository.Adicionar(AcessoColaborador.Restaurar(0, colaborador, dia.AddDays(1), dia.AddDays(1).AddHours(1)));

            var doDia = (await acessoColaboradorRepository.ObterPorColaboradorNoPeriodo(colaborador.Id, dia, dia.AddDays(1))).ToList();

            Assert.Equal(2, doDia.Count);
            Assert.Equal(TimeSpan.FromHours(5), AcessoColaborador.SomarPermanencia(doDia, dia.AddDays(1)));
            Assert.Null(await acessoColaboradorRepository.ObterEmAbertoPorColaborador(colaborador.Id));
        }
    }
}
