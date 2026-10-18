//Hibrael Andre Cidade Xavier
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

/// <summary>
/// Permite o binding two-way entre DateOnly (DTO) e DateTime (exigido pelo DatePicker do MAUI).
/// </summary>
public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateOnly dateOnly ? dateOnly.ToDateTime(TimeOnly.MinValue) : DateTime.Today;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dateTime ? DateOnly.FromDateTime(dateTime) : DateOnly.FromDateTime(DateTime.Today);
}
