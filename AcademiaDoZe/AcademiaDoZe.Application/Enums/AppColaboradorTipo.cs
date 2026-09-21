//Hibrael Andre Cidade Xavier
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

/// <summary>
/// Espelho, na camada de aplicação, de Domain.Enums.ColaboradorTipo. Existe para que a
/// apresentação não precise referenciar o domínio, e acrescenta o nome amigável de exibição.
/// Os valores numéricos são idênticos aos do domínio — é o que permite a conversão direta
/// em ColaboradorEnumMappingExtensions.
/// </summary>
public enum AppColaboradorTipo
{
    [Display(Name = "Administrador")]
    Administrador = 0,

    [Display(Name = "Atendente")]
    Atendente = 1,

    [Display(Name = "Instrutor")]
    Instrutor = 2,

    [Display(Name = "Auxiliar de Limpeza")]
    AuxiliarDeLimpeza = 3
}
