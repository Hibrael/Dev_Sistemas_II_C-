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

    private static Colaborador GetColaboradorComVinculo(ColaboradorVinculo vinculo) =>
        Colaborador.Criar(
            2, "Ciclano de Tal", "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
            "(11) 91234-5678", "ciclano@example.com",
            GetValidLogradouro(), "123", null, "Abcdef12", GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)),
            ColaboradorTipo.Atendente, vinculo, 1500m).Value!;

    [Theory(DisplayName = "Colaborador: jornada diária máxima conforme o vínculo")]
    [InlineData(ColaboradorVinculo.CLT, 8)]
    [InlineData(ColaboradorVinculo.PJ, 8)]
    [InlineData(ColaboradorVinculo.Estagio, 6)]
    public void Deve_Definir_JornadaDiariaMaxima_Por_Vinculo(ColaboradorVinculo vinculo, int horas)
    {
        Assert.Equal(TimeSpan.FromHours(horas), GetColaboradorComVinculo(vinculo).JornadaDiariaMaxima);
    }

    [Fact(DisplayName = "AcessoColaborador: RegistrarSaida fecha o acesso e calcula a permanência")]
    public void Deve_Calcular_Permanencia_Quando_RegistrarSaida()
    {
        var entrada = new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc);
        var acesso = AcessoColaborador.Restaurar(1, GetValidColaborador(), entrada);

        acesso.RegistrarSaida(entrada.AddHours(4));

        Assert.False(acesso.EmAberto);
        Assert.Equal(TimeSpan.FromHours(4), acesso.Permanencia);
    }

    [Fact(DisplayName = "AcessoColaborador: saída registrada duas vezes -> DomainException")]
    public void Deve_Lancar_Excecao_Quando_SaidaJaRegistrada()
    {
        var entrada = new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc);
        var acesso = AcessoColaborador.Restaurar(1, GetValidColaborador(), entrada, entrada.AddHours(1));

        Assert.Throws<AcademiaDoZe.Domain.Exceptions.DomainException>(() => acesso.RegistrarSaida(entrada.AddHours(2)));
    }

    [Fact(DisplayName = "AcessoColaborador: SomarPermanencia soma todos os registros do dia, o aberto até a referência")]
    public void Deve_Somar_Permanencia_De_Todos_Os_Registros()
    {
        var colaborador = GetValidColaborador();
        var dia = new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc);
        var acessos = new[]
        {
            AcessoColaborador.Restaurar(1, colaborador, dia.AddHours(8), dia.AddHours(12)),               // 4 h
            AcessoColaborador.Restaurar(2, colaborador, dia.AddHours(13), dia.AddHours(15).AddMinutes(30)), // 2 h 30
            AcessoColaborador.Restaurar(3, colaborador, dia.AddHours(16))                                   // aberto: 1 h até a referência
        };

        var total = AcessoColaborador.SomarPermanencia(acessos, dia.AddHours(17));

        Assert.Equal(TimeSpan.FromHours(7.5), total);
    }
}
