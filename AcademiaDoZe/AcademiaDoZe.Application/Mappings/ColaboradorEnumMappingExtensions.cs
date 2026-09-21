//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Mappings;

/// <summary>
/// Converte entre os enums de colaborador do domínio e seus espelhos na camada de aplicação.
/// A conversão é por cast direto porque os espelhos App* declaram exatamente os mesmos valores
/// numéricos; a checagem com Enum.IsDefined garante que um valor fora da faixa não passe
/// silenciosamente para a outra camada.
/// </summary>
public static class ColaboradorEnumMappingExtensions
{
    public static AppColaboradorTipo ToApp(this ColaboradorTipo tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new InvalidOperationException($"Tipo: TIPO_COLABORADOR_INVALIDO ({tipo})");

        return (AppColaboradorTipo)tipo;
    }

    public static ColaboradorTipo ToDomain(this AppColaboradorTipo tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new InvalidOperationException($"Tipo: TIPO_COLABORADOR_INVALIDO ({tipo})");

        return (ColaboradorTipo)tipo;
    }

    public static AppColaboradorVinculo ToApp(this ColaboradorVinculo vinculo)
    {
        if (!Enum.IsDefined(vinculo))
            throw new InvalidOperationException($"Vinculo: VINCULO_COLABORADOR_INVALIDO ({vinculo})");

        return (AppColaboradorVinculo)vinculo;
    }

    public static ColaboradorVinculo ToDomain(this AppColaboradorVinculo vinculo)
    {
        if (!Enum.IsDefined(vinculo))
            throw new InvalidOperationException($"Vinculo: VINCULO_COLABORADOR_INVALIDO ({vinculo})");

        return (ColaboradorVinculo)vinculo;
    }
}
