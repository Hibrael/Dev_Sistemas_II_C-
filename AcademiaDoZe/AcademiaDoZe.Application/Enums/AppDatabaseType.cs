//Hibrael Andre Cidade Xavier
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

/// <summary>
/// Espelho, na camada de aplicação, de Infrastructure.Data.DatabaseType. Existe para que a
/// apresentação escolha o gerenciador de banco sem referenciar a infraestrutura.
/// Os valores numéricos são idênticos aos da infraestrutura — é o que permite a conversão direta
/// em DatabaseTypeEnumMappingExtensions.
/// </summary>
public enum AppDatabaseType
{
    [Display(Name = "SQL Server")]
    SqlServer = 0,

    [Display(Name = "MySQL")]
    MySql = 1,

    [Display(Name = "SQLite")]
    Sqlite = 2
}
