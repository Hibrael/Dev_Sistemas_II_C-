//Hibrael Andre Cidade Xavier
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

/// <summary>
/// Espelho, na camada de aplicação, de Domain.Enums.MatriculaRestricoes. É [Flags] porque as
/// restrições médicas são de múltipla escolha: um aluno pode acumular várias. Os valores
/// numéricos são idênticos aos do domínio, o que permite a conversão direta por cast.
/// </summary>
[Flags]
public enum AppMatriculaRestricoes
{
    [Display(Name = "Nenhuma Restrição")]
    Nenhuma = 0,

    [Display(Name = "Diabetes")]
    Diabetes = 1,

    [Display(Name = "Labirintite")]
    Labirintite = 2,

    [Display(Name = "Problemas Respiratórios")]
    ProblemasRespiratorios = 4,

    [Display(Name = "Remédio Contínuo")]
    RemedioContinuo = 8,

    [Display(Name = "Problemas Cardíacos")]
    ProblemasCardiacos = 16,

    [Display(Name = "Problemas Ósseos")]
    ProblemasOsseos = 32,

    [Display(Name = "Cirurgia Debilitante")]
    CirurgiaDebilitante = 64
}
