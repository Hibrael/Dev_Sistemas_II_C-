//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests
{
    public class ColaboradorInfrastructureTests : TestBase
    {
        private readonly LogradouroRepository logradouroRepository;
        private readonly ColaboradorRepository repository;

        public ColaboradorInfrastructureTests()
        {
            logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
            repository = new ColaboradorRepository(ConnectionString, DatabaseType);
        }

        // Nome: nome do autor. Complemento: sobrenome do autor (requisito da atividade).
        private async Task<Logradouro> InserirLogradouro() =>
            await logradouroRepository.Adicionar(Logradouro.Criar(0, GerarCep(), "Hibrael Andre", "Cidade Xavier", NomeSgbdAtual, "SC", "Brasil").Value!);

        private async Task<Colaborador> Inserir(ColaboradorTipo tipo = ColaboradorTipo.Atendente, ColaboradorVinculo vinculo = ColaboradorVinculo.CLT, decimal salario = 2500m)
        {
            var logradouro = await InserirLogradouro();
            var foto = Arquivo.Criar("foto.jpg", [1, 2, 3, 4]).Value!;
            var result = Colaborador.Criar(0, "Hibrael Andre", GerarCpf(), new DateOnly(1995, 5, 15), GerarTelefone(), GerarEmail(),
                logradouro, "100", "Cidade Xavier", GerarSenha(), foto, DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                tipo, vinculo, salario);
            Assert.True(result.IsSuccess);
            return await repository.Adicionar(result.Value!);
        }

        [Fact]
        public async Task AdicionarEObterPorIdRetornamORegistroCorreto()
        {
            var item = await Inserir();
            var encontrado = await repository.ObterPorId(item.Id);
            Assert.NotNull(encontrado);
            Assert.Equal(item.Cpf.Numero, encontrado.Cpf.Numero);
            Assert.Equal(item.Nome, encontrado.Nome);
            Assert.Equal(item.Salario, encontrado.Salario);
            Assert.Equal(item.Tipo, encontrado.Tipo);
            Assert.Equal(item.Vinculo, encontrado.Vinculo);
        }

        [Fact]
        public async Task ObterPorIdInexistenteRetornaNulo() =>
            Assert.Null(await repository.ObterPorId(999999));

        [Fact]
        public async Task ObterTodosRetornaRegistrosInseridos()
        {
            await Inserir();
            Assert.NotEmpty(await repository.ObterTodos());
        }

        [Fact]
        public async Task AtualizarAlteraOsDadosDoRegistro()
        {
            var item = await Inserir();
            var novoLogradouro = await InserirLogradouro();
            var atualizado = Colaborador.Criar(item.Id, "Hibrael Andre", item.Cpf.Numero, item.DataNascimento,
                item.Telefone.Numero, item.Email.EnderecoEmail, novoLogradouro, "200", "Cidade Xavier",
                GerarSenha(), item.Foto, item.DataAdmissao, ColaboradorTipo.Administrador, ColaboradorVinculo.CLT, 3000m).Value!;

            await repository.Atualizar(atualizado);
            var encontrado = await repository.ObterPorId(item.Id);

            Assert.Equal("200", encontrado!.Endereco.NumeroCasa);
            Assert.Equal(ColaboradorTipo.Administrador, encontrado.Tipo);
            Assert.Equal(3000m, encontrado.Salario);
        }

        [Fact]
        public async Task AtualizarRegistroInexistenteLancaExcecao()
        {
            var logradouro = await InserirLogradouro();
            var foto = Arquivo.Criar("foto.jpg", [1, 2]).Value!;
            var item = Colaborador.Criar(999999, "Hibrael Andre", GerarCpf(), new DateOnly(1990, 1, 1), GerarTelefone(),
                GerarEmail(), logradouro, "1", "Cidade Xavier", GerarSenha(), foto,
                DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT, 2000m).Value!;

            var excecao = await Assert.ThrowsAsync<InfrastructureException>(() => repository.Atualizar(item));
            Assert.Equal("REGISTRO_NAO_ENCONTRADO", excecao.ErrorCode);
        }

        [Fact]
        public async Task RemoverExcluiORegistro()
        {
            var item = await Inserir();
            Assert.True(await repository.Remover(item.Id));
            Assert.Null(await repository.ObterPorId(item.Id));
        }

        [Fact]
        public async Task RemoverRegistroInexistenteRetornaFalse() =>
            Assert.False(await repository.Remover(999999));

        [Fact]
        public async Task ObterPorCpfEncontraOuNaoORegistro()
        {
            var item = await Inserir();
            Assert.Equal(item.Id, (await repository.ObterPorCpf(item.Cpf))!.Id);
            Assert.Null(await repository.ObterPorCpf(Cpf.Criar(GerarCpf()).Value!));
        }

        [Fact]
        public async Task ObterPorEmailEncontraOuNaoORegistro()
        {
            var item = await Inserir();
            Assert.Equal(item.Id, (await repository.ObterPorEmail(item.Email))!.Id);
            Assert.Null(await repository.ObterPorEmail(Email.Criar(GerarEmail()).Value!));
        }

        [Fact]
        public async Task CpfJaExisteRespeitaOIdInformado()
        {
            var item = await Inserir();
            Assert.True(await repository.CpfJaExiste(item.Cpf));
            Assert.False(await repository.CpfJaExiste(item.Cpf, item.Id));
            Assert.False(await repository.CpfJaExiste(Cpf.Criar(GerarCpf()).Value!));
        }

        [Fact]
        public async Task EmailJaExisteRespeitaOIdInformado()
        {
            var item = await Inserir();
            Assert.True(await repository.EmailJaExiste(item.Email));
            Assert.False(await repository.EmailJaExiste(item.Email, item.Id));
            Assert.False(await repository.EmailJaExiste(Email.Criar(GerarEmail()).Value!));
        }

        [Fact]
        public async Task ObterPorTipoFiltraCorretamente()
        {
            var item = await Inserir(tipo: ColaboradorTipo.Instrutor);
            var resultados = await repository.ObterPorTipo(ColaboradorTipo.Instrutor);
            Assert.Contains(resultados, c => c.Id == item.Id);
        }

        [Fact]
        public async Task ObterPorVinculoFiltraCorretamente()
        {
            var item = await Inserir(vinculo: ColaboradorVinculo.PJ);
            var resultados = await repository.ObterPorVinculo(ColaboradorVinculo.PJ);
            Assert.Contains(resultados, c => c.Id == item.Id);
        }

        [Fact]
        public async Task TrocarSenhaAtualizaOHashEPermiteVerificacao()
        {
            var item = await Inserir();
            var novaSenhaTexto = $"Nova{SiglaSgbdAtual}Senha123";
            var novaSenha = Senha.Criar(novaSenhaTexto).Value!;

            Assert.True(await repository.TrocarSenha(item.Id, novaSenha));
            var atualizado = await repository.ObterPorId(item.Id);
            Assert.NotNull(atualizado);
            Assert.True(atualizado!.Senha.Verificar(novaSenhaTexto));

            Assert.False(await repository.TrocarSenha(999999, novaSenha));
        }
    }
}
