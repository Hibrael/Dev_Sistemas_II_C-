// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro Teste", "Cidade Teste", "SP", "Brasil").Value!;

    private static Arquivo GetValidArquivo() =>
        Arquivo.Criar("foto.jpg", new byte[] { 1, 2, 3 }).Value!;

    private static Aluno GetValidAluno() =>
        Aluno.Criar(
            1, "João da Silva", "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "(11) 91234-5678", "aluno@example.com",
            GetValidLogradouro(), "123", null, "Abcdef12", GetValidArquivo()).Value!;

    [Fact(DisplayName = "AcessoAluno: criação bem-sucedida com aluno válido registra DataHora")]
    public void Deve_Criar_Com_Sucesso_Quando_AlunoValido()
    {
        var aluno = GetValidAluno();
        var antes = DateTime.UtcNow;

        var acesso = AcessoAluno.Criar(1, aluno);

        var depois = DateTime.UtcNow;
        Assert.Equal(aluno, acesso.Aluno);
        Assert.InRange(acesso.DataHora, antes.AddSeconds(-1), depois.AddSeconds(1));
    }

    [Fact(DisplayName = "AcessoAluno: aluno nulo -> ArgumentNullException")]
    public void Deve_Lancar_Excecao_Quando_AlunoNulo()
    {
        Assert.Throws<ArgumentNullException>(() => AcessoAluno.Criar(1, null!));
    }
}
