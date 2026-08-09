//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>Registro de um check-in/acesso de um Aluno à academia.</summary>
    public sealed class AcessoAluno : Entity, IAggregateRoot
    {
        public Aluno Aluno { get; private set; }
        public DateTime DataHora { get; private set; }

        private AcessoAluno(int id, Aluno aluno, DateTime dataHora) : base(id)
        {
            Aluno = aluno;
            DataHora = dataHora;
        }

        public static AcessoAluno Criar(int id, Aluno aluno)
        {
            ArgumentNullException.ThrowIfNull(aluno);

            return new AcessoAluno(id, aluno, DateTime.UtcNow);
        }
    }
}
