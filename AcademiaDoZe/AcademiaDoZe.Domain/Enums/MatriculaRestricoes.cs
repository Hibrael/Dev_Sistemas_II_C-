//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Domain.Enums
{
    [Flags]
    public enum MatriculaRestricoes
    {
        Nenhuma = 0,
        Diabetes = 1,
        Labirintite = 2,
        ProblemasRespiratorios = 4,
        RemedioContinuo = 8,
        ProblemasCardiacos = 16,
        ProblemasOsseos = 32,
        CirurgiaDebilitante = 64,
    }
}
