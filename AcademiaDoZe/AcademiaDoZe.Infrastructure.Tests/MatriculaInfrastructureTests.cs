//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests
{
    public class MatriculaInfrastructureTests : TestBase
    {
        private readonly LogradouroRepository logradouroRepository;
        private readonly AlunoRepository alunoRepository;
        private readonly MatriculaRepository repository;

        public MatriculaInfrastructureTests()
        {
            logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
            alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
            repository = new MatriculaRepository(ConnectionString, DatabaseType);
        }

        // Nome: nome do autor. Complemento/bairro: sobrenome do autor. Cidade: nome do SGBD
        // ativo (mesmo requisito já usado nos testes de Logradouro/Aluno/Colaborador).
        private async Task<Logradouro> InserirLogradouro() =>
            await logradouroRepository.Adicionar(Logradouro.Criar(0, GerarCep(), "Hibrael Andre", "Cidade Xavier", NomeSgbdAtual, "SC", "Brasil").Value!);

        private async Task<Aluno> InserirAluno()
        {
            var logradouro = await InserirLogradouro();
            var foto = Arquivo.Criar("foto.jpg", [1, 2, 3]).Value!;
            var result = Aluno.Criar(0, "Hibrael Andre", GerarCpf(), new DateOnly(2000, 3, 20), GerarTelefone(), GerarEmail(),
                logradouro, "100", "Cidade Xavier", GerarSenha(), foto);
            Assert.True(result.IsSuccess);
            return await alunoRepository.Adicionar(result.Value!);
        }

        // Objetivo: nome do autor (requisito da atividade). Observações da restrição: nome do
        // SGBD ativo (requisito da atividade), o que também evidencia nos prints/SELECT que os
        // registros foram gerados durante a execução com aquele gerenciador específico.
        private async Task<Matricula> Inserir(int alunoId, MatriculaPlano plano = MatriculaPlano.Mensal, DateOnly? dataInicio = null,
            MatriculaRestricoes restricoesMedicas = MatriculaRestricoes.Nenhuma, Arquivo? laudoMedico = null, string? observacoesRestricoes = null)
        {
            var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);
            var result = Matricula.Criar(0, alunoId, plano, inicio, "Hibrael Andre", restricoesMedicas, laudoMedico, observacoesRestricoes ?? NomeSgbdAtual);
            Assert.True(result.IsSuccess);
            return await repository.Adicionar(result.Value!);
        }

        [Fact]
        public async Task AdicionarEObterPorIdRetornamORegistroCorreto()
        {
            var aluno = await InserirAluno();
            var laudo = Arquivo.Criar("laudo.jpg", [10, 20, 30]).Value!;
            var restricoes = MatriculaRestricoes.Diabetes | MatriculaRestricoes.ProblemasCardiacos;
            var item = await Inserir(aluno.Id, MatriculaPlano.Trimestral, restricoesMedicas: restricoes, laudoMedico: laudo);

            var encontrado = await repository.ObterPorId(item.Id);

            Assert.NotNull(encontrado);
            Assert.Equal(aluno.Id, encontrado.AlunoId);
            Assert.Equal(MatriculaPlano.Trimestral, encontrado.Plano);
            Assert.Equal(item.DataInicio.AddMonths(3), encontrado.DataFim);
            Assert.Equal("Hibrael Andre", encontrado.Objetivo);
            Assert.Equal(restricoes, encontrado.RestricoesMedicas);
            Assert.Equal(NomeSgbdAtual, encontrado.ObservacoesRestricoes);
            Assert.NotNull(encontrado.LaudoMedico);
            Assert.Equal(laudo.Conteudo, encontrado.LaudoMedico!.Conteudo);
        }

        [Fact]
        public async Task ObterPorIdInexistenteRetornaNulo() =>
            Assert.Null(await repository.ObterPorId(999999));

        [Fact]
        public async Task ObterTodosRetornaRegistrosInseridos()
        {
            var aluno = await InserirAluno();
            await Inserir(aluno.Id);

            Assert.NotEmpty(await repository.ObterTodos());
        }

        [Fact]
        public async Task RestricoesMedicasAceitamCombinacaoDeMultiplaEscolhaViaFlags()
        {
            var aluno = await InserirAluno();
            var combinacao = MatriculaRestricoes.Labirintite | MatriculaRestricoes.ProblemasRespiratorios | MatriculaRestricoes.RemedioContinuo;
            var item = await Inserir(aluno.Id, MatriculaPlano.Semestral, restricoesMedicas: combinacao);

            var encontrado = await repository.ObterPorId(item.Id);

            Assert.NotNull(encontrado);
            Assert.Equal(combinacao, encontrado.RestricoesMedicas);
            Assert.True(encontrado.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
            Assert.True(encontrado.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasRespiratorios));
            Assert.True(encontrado.RestricoesMedicas.HasFlag(MatriculaRestricoes.RemedioContinuo));
            Assert.False(encontrado.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
            Assert.False(encontrado.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasOsseos));
        }

        [Fact]
        public async Task AtualizarAlteraOsDadosDoRegistro()
        {
            var aluno = await InserirAluno();
            var item = await Inserir(aluno.Id, MatriculaPlano.Mensal, restricoesMedicas: MatriculaRestricoes.ProblemasOsseos);

            var novasRestricoes = MatriculaRestricoes.ProblemasOsseos | MatriculaRestricoes.CirurgiaDebilitante | MatriculaRestricoes.Diabetes;
            var atualizado = Matricula.Criar(item.Id, aluno.Id, MatriculaPlano.Anual, item.DataInicio, "Hibrael Andre Cidade Xavier",
                novasRestricoes, item.LaudoMedico, $"Restrição atualizada - {NomeSgbdAtual}").Value!;

            var resultado = await repository.Atualizar(atualizado);

            Assert.Equal(MatriculaPlano.Anual, resultado.Plano);
            var encontrado = await repository.ObterPorId(item.Id);
            Assert.NotNull(encontrado);
            Assert.Equal(MatriculaPlano.Anual, encontrado.Plano);
            Assert.Equal(item.DataInicio.AddYears(1), encontrado.DataFim);
            Assert.Equal("Hibrael Andre Cidade Xavier", encontrado.Objetivo);
            Assert.Equal(novasRestricoes, encontrado.RestricoesMedicas);
            Assert.Equal($"Restrição atualizada - {NomeSgbdAtual}", encontrado.ObservacoesRestricoes);
        }

        [Fact]
        public async Task AtualizarRegistroInexistenteLancaExcecao()
        {
            var aluno = await InserirAluno();
            var item = Matricula.Criar(999999, aluno.Id, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Hibrael Andre").Value!;

            var excecao = await Assert.ThrowsAsync<InfrastructureException>(() => repository.Atualizar(item));
            Assert.Equal("REGISTRO_NAO_ENCONTRADO", excecao.ErrorCode);
        }

        [Fact]
        public async Task RemoverExcluiORegistro()
        {
            var aluno = await InserirAluno();
            var item = await Inserir(aluno.Id);

            Assert.True(await repository.Remover(item.Id));
            Assert.Null(await repository.ObterPorId(item.Id));
        }

        [Fact]
        public async Task RemoverRegistroInexistenteRetornaFalse() =>
            Assert.False(await repository.Remover(999999));

        [Fact]
        public async Task ObterPorAlunoFiltraCorretamente()
        {
            var aluno = await InserirAluno();
            var matriculaDoAluno = await Inserir(aluno.Id);
            var outroAluno = await InserirAluno();
            await Inserir(outroAluno.Id);

            var matriculas = await repository.ObterPorAluno(aluno.Id);

            Assert.NotEmpty(matriculas);
            Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoId));
            Assert.Contains(matriculas, m => m.Id == matriculaDoAluno.Id);
        }

        [Fact]
        public async Task ObterMatriculaAtivaPorAlunoEPossuiMatriculaAtivaRefletemAVigenciaPelaDataFim()
        {
            var aluno = await InserirAluno();

            Assert.False(await repository.PossuiMatriculaAtiva(aluno.Id));
            Assert.Null(await repository.ObterMatriculaAtivaPorAluno(aluno.Id));

            // Matrícula mensal iniciada há 2 anos: data_fim já está bem no passado (vencida).
            await Inserir(aluno.Id, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today).AddYears(-2));
            Assert.False(await repository.PossuiMatriculaAtiva(aluno.Id));
            Assert.Null(await repository.ObterMatriculaAtivaPorAluno(aluno.Id));

            var vigente = await Inserir(aluno.Id, MatriculaPlano.Anual, DateOnly.FromDateTime(DateTime.Today));

            Assert.True(await repository.PossuiMatriculaAtiva(aluno.Id));
            var encontrada = await repository.ObterMatriculaAtivaPorAluno(aluno.Id);
            Assert.NotNull(encontrada);
            Assert.Equal(vigente.Id, encontrada.Id);
        }

        [Fact]
        public async Task ObterAtivasRetornaApenasVigentesEFiltraPorAlunoQuandoInformado()
        {
            var aluno = await InserirAluno();
            await Inserir(aluno.Id, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today).AddYears(-3));
            var vigenteDoAluno = await Inserir(aluno.Id, MatriculaPlano.Semestral, DateOnly.FromDateTime(DateTime.Today));

            var outroAluno = await InserirAluno();
            var vigenteDeOutroAluno = await Inserir(outroAluno.Id, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today));

            var ativasGeral = await repository.ObterAtivas();
            Assert.NotEmpty(ativasGeral);
            Assert.Contains(ativasGeral, m => m.Id == vigenteDoAluno.Id);
            Assert.Contains(ativasGeral, m => m.Id == vigenteDeOutroAluno.Id);

            var ativasDoAluno = await repository.ObterAtivas(aluno.Id);
            Assert.NotEmpty(ativasDoAluno);
            Assert.All(ativasDoAluno, m => Assert.Equal(aluno.Id, m.AlunoId));
            Assert.Contains(ativasDoAluno, m => m.Id == vigenteDoAluno.Id);
        }

        [Fact]
        public async Task ObterVencendoEmDiasRetornaMatriculasProximasDoVencimento()
        {
            var aluno = await InserirAluno();
            // Mensal iniciada há 25 dias: vence em ~5 dias, dentro da janela de 30 dias pedida.
            var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-25));
            var item = await Inserir(aluno.Id, MatriculaPlano.Mensal, inicio);

            var vencendoEm30Dias = await repository.ObterVencendoEmDias(30);

            Assert.NotEmpty(vencendoEm30Dias);
            Assert.Contains(vencendoEm30Dias, m => m.Id == item.Id);
        }

        [Fact]
        public async Task ObterPorPlanoFiltraCorretamente()
        {
            var aluno = await InserirAluno();
            var item = await Inserir(aluno.Id, MatriculaPlano.Trimestral);

            var trimestrais = await repository.ObterPorPlano(MatriculaPlano.Trimestral);

            Assert.NotEmpty(trimestrais);
            Assert.Contains(trimestrais, m => m.Id == item.Id && m.Plano == MatriculaPlano.Trimestral);
        }

        [Fact]
        public async Task LaudoMedicoNuloQuandoNaoInformadoNaCriacao()
        {
            var aluno = await InserirAluno();
            var item = await Inserir(aluno.Id);

            var encontrado = await repository.ObterPorId(item.Id);

            Assert.NotNull(encontrado);
            Assert.Null(encontrado.LaudoMedico);
        }

        [Fact]
        public async Task ObservacoesRestricaoContemONomeDoSgbdAtivoNoMomentoDoTeste()
        {
            var aluno = await InserirAluno();
            var item = await Inserir(aluno.Id);

            var encontrado = await repository.ObterPorId(item.Id);

            Assert.NotNull(encontrado);
            Assert.Contains(NomeSgbdAtual, encontrado.ObservacoesRestricoes);
        }

        [Fact]
        public async Task ObjetivoEPreenchidoComONomeDoAutor()
        {
            var aluno = await InserirAluno();
            var item = await Inserir(aluno.Id);

            var encontrado = await repository.ObterPorId(item.Id);

            Assert.NotNull(encontrado);
            Assert.Equal("Hibrael Andre", encontrado.Objetivo);
        }
    }
}
