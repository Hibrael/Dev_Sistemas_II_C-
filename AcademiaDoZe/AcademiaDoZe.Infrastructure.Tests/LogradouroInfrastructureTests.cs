//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests
{
    public class LogradouroInfrastructureTests : TestBase
    {
        private readonly LogradouroRepository repository;
        public LogradouroInfrastructureTests() => repository = new(ConnectionString, DatabaseType);

        private async Task<Logradouro> Inserir(string nome = "Hibrael Andre Cidade Xavier", string bairro = "Cidade Xavier", string cidade = "SQLite")
        {
            var result = Logradouro.Criar(0, GerarCep(), nome, bairro, cidade, "SC", "Brasil");
            Assert.True(result.IsSuccess);
            return await repository.Adicionar(result.Value!);
        }

        [Fact] public async Task AdicionarEObterPorId() { var item = await Inserir(); var found = await repository.ObterPorId(item.Id); Assert.NotNull(found); Assert.Equal(item.Cep.Numero, found.Cep.Numero); }
        [Fact] public async Task ObterPorIdInexistenteRetornaNulo() => Assert.Null(await repository.ObterPorId(999999));
        [Fact] public async Task ObterTodosRetornaRegistros() { await Inserir(); Assert.NotEmpty(await repository.ObterTodos()); }
        [Fact] public async Task AtualizarAlteraRegistro() { var item = await Inserir(); var updated = Logradouro.Criar(item.Id, GerarCep(), "Rua Atualizada", "Bairro Novo", "SQLite", "SC", "Brasil").Value!; await repository.Atualizar(updated); var found = await repository.ObterPorId(item.Id); Assert.Equal("Rua Atualizada", found!.Nome); }
        [Fact] public async Task AtualizarInexistenteLancaExcecao() { var item = Logradouro.Criar(999999, GerarCep(), "Rua", "Bairro", "SQLite", "SC", "Brasil").Value!; var ex = await Assert.ThrowsAsync<InfrastructureException>(() => repository.Atualizar(item)); Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode); }
        [Fact] public async Task RemoverExcluiRegistro() { var item = await Inserir(); Assert.True(await repository.Remover(item.Id)); Assert.Null(await repository.ObterPorId(item.Id)); }
        [Fact] public async Task RemoverInexistenteRetornaFalse() => Assert.False(await repository.Remover(999999));
        [Fact] public async Task ObterPorCepEncontraRegistro() { var item = await Inserir(); Assert.Equal(item.Id, (await repository.ObterPorCep(item.Cep))!.Id); Assert.Null(await repository.ObterPorCep(Cep.Criar("99999999").Value!)); }
        [Fact] public async Task CepJaExisteRespeitaId() { var item = await Inserir(); Assert.True(await repository.CepJaExiste(item.Cep)); Assert.False(await repository.CepJaExiste(item.Cep, item.Id)); Assert.False(await repository.CepJaExiste(Cep.Criar(GerarCep()).Value!)); }
        [Fact] public async Task ObterPorCidadeFiltra() { var city = "SQLite_" + Guid.NewGuid().ToString("N")[..6]; await Inserir(cidade: city); Assert.Single(await repository.ObterPorCidade(city)); Assert.Empty(await repository.ObterPorCidade("Cidade inexistente")); }
        [Fact] public async Task ObterPorBairroFiltra() { var city = "SQLite_" + Guid.NewGuid().ToString("N")[..6]; var district = "Cidade Xavier_" + Guid.NewGuid().ToString("N")[..6]; await Inserir(bairro: district, cidade: city); Assert.Single(await repository.ObterPorBairro(city, district)); Assert.Empty(await repository.ObterPorBairro(city, "Bairro inexistente")); }
    }
}