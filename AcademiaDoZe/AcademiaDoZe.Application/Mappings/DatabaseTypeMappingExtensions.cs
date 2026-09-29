//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.Mappings;

/// <summary>
/// Converte entre o tipo de banco da infraestrutura e seu espelho na camada de aplicação.
/// A conversão é por cast direto porque AppDatabaseType declara exatamente os mesmos valores
/// numéricos; a checagem com Enum.IsDefined garante que um valor fora da faixa não passe
/// silenciosamente para a outra camada.
/// </summary>
public static class DatabaseTypeMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new InvalidOperationException($"DatabaseType: TIPO_BANCO_INVALIDO ({tipo})");

        return (DatabaseType)tipo;
    }

    public static AppDatabaseType ToApp(this DatabaseType tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new InvalidOperationException($"DatabaseType: TIPO_BANCO_INVALIDO ({tipo})");

        return (AppDatabaseType)tipo;
    }
}
