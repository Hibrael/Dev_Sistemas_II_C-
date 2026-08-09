//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Matricula referencia o Aluno por AlunoId (e não por uma referência de objeto Aluno)
    /// porque Aluno e Matricula são Aggregate Roots distintos: entidades de agregados
    /// diferentes devem se referenciar por identidade, nunca por composição direta.
    /// </summary>
    public sealed class Matricula : Entity, IAggregateRoot
    {
        public int AlunoId { get; private set; }
        public MatriculaPlano Plano { get; private set; }
        public MatriculaRestricoes Restricoes { get; private set; }
        public DateOnly DataInicio { get; private set; }
        public DateOnly DataFim { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativa { get; private set; }

        private Matricula(int id, int alunoId, MatriculaPlano plano, MatriculaRestricoes restricoes,
            DateOnly dataInicio, DateOnly dataFim, decimal valor) : base(id)
        {
            AlunoId = alunoId;
            Plano = plano;
            Restricoes = restricoes;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Valor = valor;
            Ativa = true;
        }

        public static Result<Matricula> Criar(int id, int alunoId, MatriculaPlano plano,
            MatriculaRestricoes restricoes, DateOnly dataInicio, decimal valor)
        {
            var notificacoes = new List<Notification>();

            if (alunoId <= 0)
                notificacoes.Add(new Notification("AlunoId", "ALUNO_INVALIDO"));

            if (!Enum.IsDefined(plano))
                notificacoes.Add(new Notification("Plano", "PLANO_MATRICULA_INVALIDO"));

            if (valor <= 0)
                notificacoes.Add(new Notification("Valor", "VALOR_MATRICULA_INVALIDO"));

            if (notificacoes.Count != 0)
                return Result<Matricula>.Failure(notificacoes);

            var dataFim = CalcularDataFim(dataInicio, plano);
            return Result<Matricula>.Success(new Matricula(id, alunoId, plano, restricoes, dataInicio, dataFim, valor));
        }

        public void Cancelar()
        {
            if (!Ativa)
                throw new DomainException("Matrícula já está cancelada.");

            Ativa = false;
        }

        private static DateOnly CalcularDataFim(DateOnly dataInicio, MatriculaPlano plano) => plano switch
        {
            MatriculaPlano.Mensal => dataInicio.AddMonths(1),
            MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
            MatriculaPlano.Semestral => dataInicio.AddMonths(6),
            MatriculaPlano.Anual => dataInicio.AddYears(1),
            _ => throw new DomainException("Plano de matrícula inválido.")
        };
    }
}



