// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory(DisplayName = "Logradouro: CEP inválido -> notificação do ValueObject Cep é propagada")]
    [InlineData(null, "CEP_OBRIGATORIO")]
    [InlineData("123", "CEP_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_CepInvalido(string? cep, string mensagemEsperada)
    {
        var result = Logradouro.Criar(1, cep!, "Rua Teste", "Bairro", "Cidade", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == mensagemEsperada);
    }

    [Theory(DisplayName = "Logradouro: campos obrigatórios vazios")]
    [InlineData("", "Bairro", "Cidade", "SP", "Brasil", "NOME_OBRIGATORIO")]
    [InlineData("Rua Teste", "", "Cidade", "SP", "Brasil", "BAIRRO_OBRIGATORIO")]
    [InlineData("Rua Teste", "Bairro", "", "SP", "Brasil", "CIDADE_OBRIGATORIO")]
    [InlineData("Rua Teste", "Bairro", "Cidade", "", "Brasil", "ESTADO_OBRIGATORIO")]
    [InlineData("Rua Teste", "Bairro", "Cidade", "SP", "", "PAIS_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_CampoObrigatorioVazio(
        string nome, string bairro, string cidade, string estado, string pais, string mensagemEsperada)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, bairro, cidade, estado, pais);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == mensagemEsperada);
    }

    [Theory(DisplayName = "Logradouro: estado é normalizado para maiúsculo, sem espaços")]
    [InlineData(" sp ", "SP")]
    [InlineData("rj", "RJ")]
    public void Deve_Normalizar_Estado_Para_Maiusculo_Sem_Espacos(string estado, string estadoEsperado)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", estado, "Brasil");

        Assert.True(result.IsSuccess);
        Assert.Equal(estadoEsperado, result.Value!.Estado);
    }

    [Fact(DisplayName = "Logradouro: criação bem-sucedida com todos os dados válidos")]
    public void Deve_Criar_Logradouro_Quando_Valido()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil");

        Assert.True(result.IsSuccess);
        Assert.Equal("12345678", result.Value!.Cep.Numero);
        Assert.Equal("Rua Teste", result.Value.Nome);
    }

    [Fact(DisplayName = "Logradouro: acumula notificações de vários campos inválidos simultaneamente")]
    public void Deve_Acumular_Multiplas_Notificacoes_Quando_VariosCamposInvalidos()
    {
        var result = Logradouro.Criar(1, "123", "", "", "Cidade", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.True(result.Notificacoes.Count >= 3);
    }
}
