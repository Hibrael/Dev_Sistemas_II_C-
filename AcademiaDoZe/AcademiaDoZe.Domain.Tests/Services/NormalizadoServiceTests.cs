// AcademiaDoZe.Domain.Tests
//
// Testa o serviço real AcademiaDoZe.Domain.Services.NormalizadoService (note: "Normalizado",
// não "Normalizacao" como no material em PDF). O método de dígitos também mudou de nome
// para ApenasDigitos (era LimparEDigitos no PDF), e existe um ParaMinusculo simétrico ao
// ParaMaiusculo que o PDF não cobria.
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.Tests.Services;

public class NormalizadoServiceTests
{
    [Theory(DisplayName = "NormalizadoService: TextoVazioOuNulo -> valida nulo/vazio/espacos")]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("texto", false)]
    public void Deve_TextoVazioOuNulo_RetornarEsperado(string? input, bool expected)
    {
        var result = NormalizadoService.TextoVazioOuNulo(input);

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizadoService: LimparEspacos -> normaliza sequências de espaços em um único espaço")]
    [InlineData("", "")]
    [InlineData(" a  b   c ", "a b c")]
    [InlineData("a\tb\nc", "a b c")]
    public void Deve_LimparEspacos_Normalizar_Espacos(string input, string expected)
    {
        var result = NormalizadoService.LimparEspacos(input);

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizadoService: LimparTodosEspacos -> remove todos os espaços")]
    [InlineData("", "")]
    [InlineData("a b c", "abc")]
    [InlineData(" a  b ", "ab")]
    public void Deve_LimparTodosEspacos_Remover_Todos_Os_Espacos(string input, string expected)
    {
        var result = NormalizadoService.LimparTodosEspacos(input);

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizadoService: ApenasDigitos -> mantém apenas dígitos")]
    [InlineData("", "")]
    [InlineData("a1b2c3", "123")]
    [InlineData("(11) 91234-5678", "11912345678")]
    [InlineData("no-digits", "")]
    public void Deve_ApenasDigitos_Manter_Somente_Digitos(string input, string expected)
    {
        var result = NormalizadoService.ApenasDigitos(input);

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizadoService: ParaMaiusculo -> converte para maiúsculo e remove espaços das bordas")]
    [InlineData("abc", "ABC")]
    [InlineData(" abc ", "ABC")]
    [InlineData("áéíõç", "ÁÉÍÕÇ")]
    public void Deve_ParaMaiusculo_Converter_Para_Maiusculo(string input, string expected)
    {
        var result = NormalizadoService.ParaMaiusculo(input);

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "NormalizadoService: ParaMinusculo -> converte para minúsculo e remove espaços das bordas")]
    [InlineData("ABC", "abc")]
    [InlineData(" ABC ", "abc")]
    [InlineData("ÁÉÍÕÇ", "áéíõç")]
    public void Deve_ParaMinusculo_Converter_Para_Minusculo(string input, string expected)
    {
        var result = NormalizadoService.ParaMinusculo(input);

        Assert.Equal(expected, result);
    }
}
