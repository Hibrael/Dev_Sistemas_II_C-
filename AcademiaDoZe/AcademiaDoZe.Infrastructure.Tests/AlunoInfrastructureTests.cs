//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests
{
    public class AlunoInfrastructureTests : TestBase
    {
        [Fact]
        public void GerarSenhaUsaSiglaDoSgbdAtual()
        {
            Assert.Equal($"Senha{SiglaSgbdAtual}123", GerarSenha());
            Assert.DoesNotContain("|", GerarSenha());
            Assert.DoesNotContain("==", GerarSenha());
        }

        private readonly LogradouroRepository logradouroRepository;
        private readonly AlunoRepository repository;

        public AlunoInfrastructureTests()
        {
            logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
            repository = new AlunoRepository(ConnectionString, DatabaseType);
        }

        // Nome: nome do autor. Complemento: sobrenome do autor (requisito da atividade).
        private async Task<Logradouro> InserirLogradouro() =>
            await logradouroRepository.Adicionar(Logradouro.Criar(0, GerarCep(), "Hibrael Andre", "Cidade Xavier", NomeSgbdAtual, "SC", "Brasil").Value!);

        private async Task<Aluno> Inserir()
        {
            var logradouro = await InserirLogradouro();
            var foto = Arquivo.Criar("foto.jpg", [5, 6, 7, 8]).Value!;
            var result = Aluno.Criar(0, "Hibrael Andre", GerarCpf(), new DateOnly(2000, 3, 20), GerarTelefone(), GerarEmail(),
                logradouro, "100", "Cidade Xavier", GerarSenha(), foto);
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
            Assert.Equal(item.Email.EnderecoEmail, encontrado.Email.EnderecoEmail);
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
            var atualizado = Aluno.Criar(item.Id, "Hibrael Andre", item.Cpf.Numero, item.DataNascimento,
                item.Telefone.Numero, item.Email.EnderecoEmail, novoLogradouro, "200", "Cidade Xavier",
                GerarSenha(), item.Foto).Value!;

            await repository.Atualizar(atualizado);
            var encontrado = await repository.ObterPorId(item.Id);

            Assert.Equal("200", encontrado!.Endereco.NumeroCasa);
            Assert.Equal(novoLogradouro.Id, encontrado.Endereco.Logradouro.Id);
        }

        [Fact]
        public async Task AtualizarRegistroInexistenteLancaExcecao()
        {
            var logradouro = await InserirLogradouro();
            var foto = Arquivo.Criar("foto.jpg", [1, 2]).Value!;
            var item = Aluno.Criar(999999, "Hibrael Andre", GerarCpf(), new DateOnly(2000, 1, 1), GerarTelefone(),
                GerarEmail(), logradouro, "1", "Cidade Xavier", GerarSenha(), foto).Value!;

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
