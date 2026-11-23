//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

/// <summary>
/// Transforma a foto gravada no banco (ArquivoDto, ou o array de bytes direto, como vem nos DTOs
/// de acesso) em ImageSource para exibição.
/// </summary>
public sealed class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ArquivoDto foto && foto.Conteudo is { Length: > 0 } conteudo)
            return ImageSource.FromStream(() => new MemoryStream(conteudo));

        if (value is byte[] { Length: > 0 } bytes)
            return ImageSource.FromStream(() => new MemoryStream(bytes));

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
