// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro Teste", "Cidade Teste", "SP", "Brasil").Value!;

    private static Arquivo GetValidArquivo() =>
        Arquivo.Criar("foto.jpg", new byte[] { 1, 2, 3 }).Value!;

    private static Result<Colaborador> CriarColaborador(
        string nome = "Fulano de Tal",
        DateOnly? dataNascimento = null,
        DateOnly? dataAdmissao = null,
        ColaboradorTipo tipo = ColaboradorTipo.Atendente,
        ColaboradorVinculo vinculo = ColaboradorVinculo.CLT,
        decimal salario = 2500m) =>
        Colaborador.Criar(
            1, nome, "529.982.247-25",
            dataNascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "colaborador@example.com",
            GetValidLogradouro(), "123", null, "Abcdef12", GetValidArquivo(),
            dataAdmissao ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            tipo, vinculo, salario);

    [Theory(DisplayName = "Colaborador: criação bem-sucedida com combinações válidas de tipo e vínculo")]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.PJ)]
    [InlineData(ColaboradorTipo.AuxiliarDeLimpeza, ColaboradorVinculo.Estagio)]
    [InlineData(ColaboradorTipo.Administrador, ColaboradorVinculo.CLT)]
    public void Deve_Criar_Com_Sucesso_Quando_TipoEVinculoValidos(ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var result = CriarColaborador(tipo: tipo, vinculo: vinculo);

        Assert.True(result.IsSuccess);
        Assert.Equal(tipo, result.Value!.Tipo);
        Assert.Equal(vinculo, result.Value.Vinculo);
    }

    [Theory(DisplayName = "Colaborador: Administrador com vínculo diferente de CLT -> ADMINISTRADOR_CLT_INVALIDO")]
    [InlineData(ColaboradorVinculo.PJ)]
    [InlineData(ColaboradorVinculo.Estagio)]
    public void Deve_Falhar_Criacao_Quando_AdministradorComVinculoInvalido(ColaboradorVinculo vinculo)
    {
        var result = CriarColaborador(tipo: ColaboradorTipo.Administrador, vinculo: vinculo);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: tipo fora do enum -> TIPO_COLABORADOR_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_TipoInvalido()
    {
        var result = CriarColaborador(tipo: (ColaboradorTipo)999);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: vínculo fora do enum -> VINCULO_COLABORADOR_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_VinculoInvalido()
    {
        var result = CriarColaborador(vinculo: (ColaboradorVinculo)999);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "VINCULO_COLABORADOR_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: data de admissão obrigatória -> DATA_ADMISSAO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_DataAdmissaoPadrao()
    {
        var result = CriarColaborador(dataAdmissao: default(DateOnly));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "DATA_ADMISSAO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Colaborador: data de admissão futura -> DATA_ADMISSAO_MAIOR_ATUAL")]
    [InlineData(1)]
    [InlineData(30)]
    public void Deve_Falhar_Criacao_Quando_DataAdmissaoFutura(int diasNoFuturo)
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(diasNoFuturo));

        var result = CriarColaborador(dataAdmissao: data);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "DATA_ADMISSAO_MAIOR_ATUAL");
    }

    [Theory(DisplayName = "Colaborador: data de nascimento -> obrigatoriedade e idade mínima de 12 anos")]
    [InlineData("default", "DATA_NASCIMENTO_OBRIGATORIO")]
    [InlineData("menorDeIdade", "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoInvalida(string cenario, string mensagemEsperada)
    {
        var data = cenario == "default" ? default : DateOnly.FromDateTime(DateTime.Today.AddYears(-5));

        var result = CriarColaborador(dataNascimento: data);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == mensagemEsperada);
    }

    [Theory(DisplayName = "Colaborador: salário inválido -> SALARIO_INVALIDO")]
    [InlineData(0)]
    [InlineData(-100)]
    public void Deve_Falhar_Criacao_Quando_SalarioInvalido(decimal salario)
    {
        var result = CriarColaborador(salario: salario);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "SALARIO_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: foto obrigatória -> FOTO_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_FotoNula()
    {
        var result = Colaborador.Criar(
            1, "Fulano de Tal", "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "colaborador@example.com",
            GetValidLogradouro(), "123", null, "Abcdef12", null!,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente, ColaboradorVinculo.CLT, 2500m);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "FOTO_OBRIGATORIA");
    }

    [Fact(DisplayName = "Colaborador: acumula notificações de vários campos inválidos simultaneamente")]
    public void Deve_Acumular_Multiplas_Notificacoes_Quando_VariosCamposInvalidos()
    {
        var result = CriarColaborador(nome: "", salario: 0, tipo: (ColaboradorTipo)999);

        Assert.True(result.IsFailure);
        Assert.True(result.Notificacoes.Count >= 3);
    }

    [Fact(DisplayName = "Colaborador: Desligar com data válida marca DataDemissao e encerra o vínculo")]
    public void Deve_Desligar_Com_Sucesso_Quando_DataValida()
    {
        var colaborador = CriarColaborador(dataAdmissao: DateOnly.FromDateTime(DateTime.Today.AddYears(-2))).Value!;
        var dataDemissao = DateOnly.FromDateTime(DateTime.Today);

        colaborador.Desligar(dataDemissao);

        Assert.Equal(dataDemissao, colaborador.DataDemissao);
    }

    [Fact(DisplayName = "Colaborador: Desligar um colaborador já demitido lança DomainException")]
    public void Deve_Lancar_Excecao_Ao_Desligar_Quando_JaDemitido()
    {
        var colaborador = CriarColaborador(dataAdmissao: DateOnly.FromDateTime(DateTime.Today.AddYears(-2))).Value!;
        colaborador.Desligar(DateOnly.FromDateTime(DateTime.Today));

        Assert.Throws<DomainException>(() => colaborador.Desligar(DateOnly.FromDateTime(DateTime.Today)));
    }

    [Fact(DisplayName = "Colaborador: Desligar com data anterior à admissão lança DomainException")]
    public void Deve_Lancar_Excecao_Ao_Desligar_Quando_DataAnteriorAdmissao()
    {
        var dataAdmissao = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        var colaborador = CriarColaborador(dataAdmissao: dataAdmissao).Value!;

        Assert.Throws<DomainException>(() => colaborador.Desligar(dataAdmissao.AddDays(-1)));
    }

    [Fact(DisplayName = "Colaborador: Desligar com data futura lança DomainException")]
    public void Deve_Lancar_Excecao_Ao_Desligar_Quando_DataFutura()
    {
        var colaborador = CriarColaborador(dataAdmissao: DateOnly.FromDateTime(DateTime.Today.AddYears(-2))).Value!;

        Assert.Throws<DomainException>(() => colaborador.Desligar(DateOnly.FromDateTime(DateTime.Today.AddDays(1))));
    }
}
