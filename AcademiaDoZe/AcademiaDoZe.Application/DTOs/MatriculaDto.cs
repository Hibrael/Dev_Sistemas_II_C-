//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class MatriculaDto
{
    public int Id { get; set; }

    /// <summary>
    /// Identificador do aluno matriculado. É este campo — e não o objeto AlunoMatricula — que
    /// alimenta Matricula.Criar, porque Aluno e Matricula são Aggregate Roots distintos e a
    /// entidade referencia o aluno por identidade (AlunoId), nunca por composição.
    /// </summary>
    public required int AlunoId { get; set; }

    /// <summary>
    /// Dados do aluno, preenchidos por MatriculaService na leitura para a apresentação não
    /// precisar de uma segunda consulta. Ignorado na escrita: só AlunoId é considerado.
    /// </summary>
    public AlunoDto? AlunoMatricula { get; set; }

    public required AppMatriculaPlano Plano { get; set; }
    public required DateOnly DataInicio { get; set; }

    /// <summary>
    /// Somente leitura: é calculada pelo domínio a partir de DataInicio e do Plano.
    /// </summary>
    public DateOnly DataFim { get; set; }

    public required string Objetivo { get; set; }
    public required AppMatriculaRestricoes RestricoesMedicas { get; set; }
    public string? ObservacoesRestricoes { get; set; }
    public ArquivoDto? LaudoMedico { get; set; }
}
