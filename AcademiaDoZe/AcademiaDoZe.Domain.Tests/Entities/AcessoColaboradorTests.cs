// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoColaboradorTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro Teste", "Cidade Teste", "SP", "Brasil").Value!;

    private static Arquivo GetValidArquivo() =>
        Arquivo.Criar("foto.jpg", new byte[] { 1, 2, 3 }).Value!;

    private static Colaborador GetValidColaborador() =>
        Colaborador.Criar(
            1, "Fulano de Tal", "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "colaborador@example.com",
            GetValidLogradouro(), "123", null, "Abcdef12", GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente, ColaboradorVinculo.CLT, 2500m).Value!;

    [Fact(DisplayName = "AcessoColaborador: criação bem-sucedida com colaborador válido registra DataHora")]
    public void Deve_Criar_Com_Sucesso_Quando_ColaboradorValido()
    {
        var colaborador = GetValidColaborador();
        var antes = DateTime.UtcNow;

        var acesso = AcessoColaborador.Criar(1, colaborador);

        var depois = DateTime.UtcNow;
        Assert.Equal(colaborador, acesso.Colaborador);
        Assert.InRange(acesso.DataHora, antes.AddSeconds(-1), depois.AddSeconds(1));
    }

    [Fact(DisplayName = "AcessoColaborador: colaborador nulo -> ArgumentNullException")]
    public void Deve_Lancar_Excecao_Quando_ColaboradorNulo()
    {
        Assert.Throws<ArgumentNullException>(() => AcessoColaborador.Criar(1, null!));
    }
}
