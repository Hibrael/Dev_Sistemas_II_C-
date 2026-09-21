//Hibrael Andre Cidade Xavier
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AcademiaDoZe.Application.Enums;

/// <summary>
/// Lê o [Display(Name = "...")] de um valor de enum para que a apresentação exiba o texto
/// amigável sem precisar conhecer o domínio nem repetir esses rótulos na tela.
/// </summary>
public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var type = value.GetType();
        var field = type.GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DisplayAttribute>();

        if (attribute != null)
            return attribute.Name ?? value.ToString();

        // Enums [Flags] podem combinar vários valores: nesse caso o campo não existe
        // isoladamente, então juntamos o rótulo de cada flag ligada.
        if (type.GetCustomAttribute<FlagsAttribute>() != null)
        {
            var names = new List<string>();

            foreach (Enum flag in Enum.GetValues(type))
            {
                if (Convert.ToInt64(flag) != 0 && value.HasFlag(flag))
                {
                    var flagField = type.GetField(flag.ToString());
                    var flagAttr = flagField?.GetCustomAttribute<DisplayAttribute>();
                    names.Add(flagAttr?.Name ?? flag.ToString());
                }
            }

            if (names.Count > 0)
                return string.Join(", ", names);
        }

        return value.ToString();
    }
}
