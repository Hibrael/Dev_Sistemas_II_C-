// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
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

    [Fact(DisplayName = "AcessoAluno: novo acesso fica em aberto, sem saída nem permanência")]
    public void Deve_Ficar_EmAberto_Quando_Criado()
    {
        var acesso = AcessoAluno.Criar(1, GetValidAluno());

        Assert.True(acesso.EmAberto);
        Assert.Null(acesso.DataHoraSaida);
        Assert.Null(acesso.Permanencia);
    }

    [Fact(DisplayName = "AcessoAluno: RegistrarSaida fecha o acesso e calcula a permanência")]
    public void Deve_Calcular_Permanencia_Quando_RegistrarSaida()
    {
        var entrada = new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc);
        var acesso = AcessoAluno.Restaurar(1, GetValidAluno(), entrada);

        acesso.RegistrarSaida(entrada.AddMinutes(95));

        Assert.False(acesso.EmAberto);
        Assert.Equal(TimeSpan.FromMinutes(95), acesso.Permanencia);
    }

    [Fact(DisplayName = "AcessoAluno: saída registrada duas vezes -> DomainException")]
    public void Deve_Lancar_Excecao_Quando_SaidaJaRegistrada()
    {
        var entrada = new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc);
        var acesso = AcessoAluno.Restaurar(1, GetValidAluno(), entrada, entrada.AddHours(1));

        Assert.Throws<DomainException>(() => acesso.RegistrarSaida(entrada.AddHours(2)));
    }

    [Fact(DisplayName = "AcessoAluno: saída anterior à entrada -> DomainException")]
    public void Deve_Lancar_Excecao_Quando_SaidaAntesDaEntrada()
    {
        var entrada = new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc);
        var acesso = AcessoAluno.Restaurar(1, GetValidAluno(), entrada);

        Assert.Throws<DomainException>(() => acesso.RegistrarSaida(entrada.AddMinutes(-1)));
    }
}
