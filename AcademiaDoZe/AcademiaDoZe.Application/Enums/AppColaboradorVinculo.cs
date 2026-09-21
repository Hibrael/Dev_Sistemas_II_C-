//Hibrael Andre Cidade Xavier
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

/// <summary>
/// Espelho, na camada de aplicação, de Domain.Enums.ColaboradorVinculo, com os mesmos
/// valores numéricos do domínio.
/// </summary>
public enum AppColaboradorVinculo
{
    [Display(Name = "CLT")]
    CLT = 0,

    [Display(Name = "PJ")]
    PJ = 1,

    [Display(Name = "Estagiário")]
    Estagio = 2
}
