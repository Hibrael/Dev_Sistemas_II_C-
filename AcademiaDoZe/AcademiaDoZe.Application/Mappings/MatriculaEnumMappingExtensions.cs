//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Mappings;

/// <summary>
/// Converte entre os enums de matrícula do domínio e seus espelhos na camada de aplicação.
/// </summary>
public static class MatriculaEnumMappingExtensions
{
    // MatriculaRestricoes é [Flags]: Enum.IsDefined só reconhece valores declarados
    // isoladamente, então combinações válidas (ex.: Diabetes | Alergias) seriam rejeitadas.
    // A validação usa a máscara com o OR de todas as flags, como faz Matricula.Criar.
    private const AppMatriculaRestricoes TodasRestricoes =
        AppMatriculaRestricoes.Diabetes | AppMatriculaRestricoes.Labirintite |
        AppMatriculaRestricoes.ProblemasRespiratorios | AppMatriculaRestricoes.RemedioContinuo |
        AppMatriculaRestricoes.ProblemasCardiacos | AppMatriculaRestricoes.ProblemasOsseos |
        AppMatriculaRestricoes.CirurgiaDebilitante;

    public static AppMatriculaPlano ToApp(this MatriculaPlano plano)
    {
        if (!Enum.IsDefined(plano))
            throw new InvalidOperationException($"Plano: PLANO_MATRICULA_INVALIDO ({plano})");

        return (AppMatriculaPlano)plano;
    }

    public static MatriculaPlano ToDomain(this AppMatriculaPlano plano)
    {
        if (!Enum.IsDefined(plano))
            throw new InvalidOperationException($"Plano: PLANO_MATRICULA_INVALIDO ({plano})");

        return (MatriculaPlano)plano;
    }

    public static AppMatriculaRestricoes ToApp(this MatriculaRestricoes restricoes) =>
        (AppMatriculaRestricoes)restricoes;

    public static MatriculaRestricoes ToDomain(this AppMatriculaRestricoes restricoes)
    {
        if ((restricoes & ~TodasRestricoes) != 0)
            throw new InvalidOperationException($"RestricoesMedicas: RESTRICAO_MEDICA_INVALIDA ({restricoes})");

        return (MatriculaRestricoes)restricoes;
    }
}
