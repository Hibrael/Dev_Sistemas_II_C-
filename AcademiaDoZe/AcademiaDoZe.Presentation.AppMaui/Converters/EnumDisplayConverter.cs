//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

/// <summary>
/// Converte qualquer enum para o texto de [Display(Name = "...")], usado nos Pickers e na listagem.
/// É só de exibição: o Picker devolve o próprio valor do enum em SelectedItem, por isso não há ConvertBack.
/// </summary>
public sealed class EnumDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Enum e ? e.GetDisplayName() : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
