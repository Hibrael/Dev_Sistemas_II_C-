//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Matricula referencia o Aluno por AlunoId (e não por uma referência de objeto Aluno)
    /// porque Aluno e Matricula são Aggregate Roots distintos: entidades de agregados
    /// diferentes devem se referenciar por identidade, nunca por composição direta.
    ///
    /// ATENÇÃO (adaptação): a versão inicial desta classe possuía Valor/Ativa (matrícula
    /// cancelável manualmente). O laboratório de infraestrutura de Matricula trabalha com
    /// outro recorte de negócio -- objetivo do aluno, restrições médicas (múltipla escolha),
    /// laudo médico e observações -- e a tabela tb_matricula já foi criada com essas colunas
    /// (objetivo, restricao_medica, obs_restricao, laudo_medico), sem valor/ativa. Por isso a
    /// entidade foi ajustada para esse formato: "matrícula ativa" deixa de ser um estado
    /// gravado (Ativa/Cancelar) e passa a ser calculado a partir de DataFim pelo repositório
    /// (ObterMatriculaAtivaPorAluno, PossuiMatriculaAtiva, ObterAtivas), como pedido no material.
    /// </summary>
    public sealed class Matricula : Entity, IAggregateRoot
    {
        public int AlunoId { get; private set; }
        public MatriculaPlano Plano { get; private set; }
        public DateOnly DataInicio { get; private set; }
        public DateOnly DataFim { get; private set; }
        public string Objetivo { get; private set; }
        public MatriculaRestricoes RestricoesMedicas { get; private set; }
        public string? ObservacoesRestricoes { get; private set; }
        public Arquivo? LaudoMedico { get; private set; }

        private Matricula(int id, int alunoId, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim,
            string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico, string? observacoesRestricoes) : base(id)
        {
            AlunoId = alunoId;
            Plano = plano;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Objetivo = objetivo;
            RestricoesMedicas = restricoesMedicas;
            LaudoMedico = laudoMedico;
            ObservacoesRestricoes = observacoesRestricoes;
        }

        public static Result<Matricula> Criar(int id, int alunoId, MatriculaPlano plano, DateOnly dataInicio, string objetivo,
            MatriculaRestricoes restricoesMedicas = MatriculaRestricoes.Nenhuma, Arquivo? laudoMedico = null, string? observacoesRestricoes = null)
        {
            var notificacoes = new List<Notificacoes>();

            if (alunoId <= 0)
                notificacoes.Add(new Notificacoes("AlunoId", "ALUNO_INVALIDO"));

            if (!Enum.IsDefined(plano))
                notificacoes.Add(new Notificacoes("Plano", "PLANO_MATRICULA_INVALIDO"));

            if (NormalizadoService.TextoVazioOuNulo(objetivo))
                notificacoes.Add(new Notificacoes("Objetivo", "OBJETIVO_MATRICULA_INVALIDO"));

            if ((restricoesMedicas & ~TodasRestricoes) != 0)
                notificacoes.Add(new Notificacoes("RestricoesMedicas", "RESTRICAO_MEDICA_INVALIDA"));

            if (notificacoes.Count != 0)
                return Result<Matricula>.Failure(notificacoes);

            var dataFim = CalcularDataFim(dataInicio, plano);
            return Result<Matricula>.Success(new Matricula(id, alunoId, plano, dataInicio, dataFim,
                NormalizadoService.LimparEspacos(objetivo), restricoesMedicas, laudoMedico, observacoesRestricoes));
        }

        // MatriculaRestricoes é [Flags]: Enum.IsDefined só reconhece combinações declaradas
        // explicitamente, então a validação acima usa uma máscara com todas as flags válidas
        // (OR de todos os valores) para aceitar qualquer combinação por múltipla escolha e
        // ainda assim rejeitar bits fora do enum.
        private const MatriculaRestricoes TodasRestricoes =
            MatriculaRestricoes.Diabetes | MatriculaRestricoes.Labirintite | MatriculaRestricoes.ProblemasRespiratorios |
            MatriculaRestricoes.RemedioContinuo | MatriculaRestricoes.ProblemasCardiacos | MatriculaRestricoes.ProblemasOsseos |
            MatriculaRestricoes.CirurgiaDebilitante;

        private static DateOnly CalcularDataFim(DateOnly dataInicio, MatriculaPlano plano) => plano switch
        {
            MatriculaPlano.Mensal => dataInicio.AddMonths(1),
            MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
            MatriculaPlano.Semestral => dataInicio.AddMonths(6),
            MatriculaPlano.Anual => dataInicio.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(plano), plano, "Plano de matrícula inválido.")
        };
    }
}
