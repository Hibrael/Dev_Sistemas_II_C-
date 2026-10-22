//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

/// <summary>
/// Converte o enum [Flags] AppMatriculaRestricoes em texto amigável: o [Display(Name = "...")] de cada
/// restrição marcada, separados por vírgula (ex.: "Diabetes, Labirintite").
/// Sem nenhuma marcada, exibe o Display de Nenhuma ("Nenhuma Restrição").
/// </summary>
public sealed class RestricoesFlagsDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppMatriculaRestricoes flags)
            return string.Empty;

        // Nenhuma
        if (flags == AppMatriculaRestricoes.Nenhuma)
            return AppMatriculaRestricoes.Nenhuma.GetDisplayName();

        // Junta todas as que estão marcadas
        var partes = new List<string>();
        foreach (var v in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (v == AppMatriculaRestricoes.Nenhuma)
                continue;

            if (flags.HasFlag(v))
                partes.Add(v.GetDisplayName());
        }

        return partes.Count > 0 ? string.Join(", ", partes) : AppMatriculaRestricoes.Nenhuma.GetDisplayName();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
