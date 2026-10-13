//Hibrael Andre Cidade Xavier
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

/// <summary>
/// Formata para exibição os números que o banco guarda só com dígitos.
/// ConverterParameter: "Cpf" (000.000.000-00), "Telefone" ((00) 00000-0000) ou "Cep" (00000-000).
/// Valores fora do tamanho esperado são exibidos como vieram.
/// </summary>
public sealed class MascaraConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var texto = value?.ToString() ?? string.Empty;
        var d = new string([.. texto.Where(char.IsDigit)]);

        return (parameter as string) switch
        {
            "Cpf" when d.Length == 11 => $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}",
            "Telefone" when d.Length == 11 => $"({d[..2]}) {d[2..7]}-{d[7..]}",
            "Telefone" when d.Length == 10 => $"({d[..2]}) {d[2..6]}-{d[6..]}",
            "Cep" when d.Length == 8 => $"{d[..5]}-{d[5..]}",
            _ => texto
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
