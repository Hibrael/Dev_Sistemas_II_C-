//Hibrael Andre Cidade Xavier
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

/// <summary>
/// Espelho, na camada de aplicação, de Domain.Enums.MatriculaPlano, com os mesmos valores
/// numéricos do domínio.
/// </summary>
public enum AppMatriculaPlano
{
    [Display(Name = "Mensal")]
    Mensal = 0,

    [Display(Name = "Trimestral")]
    Trimestral = 1,

    [Display(Name = "Semestral")]
    Semestral = 2,

    [Display(Name = "Anual")]
    Anual = 3
}
