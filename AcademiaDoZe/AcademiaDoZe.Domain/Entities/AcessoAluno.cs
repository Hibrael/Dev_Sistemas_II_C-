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

        /// <summary>
        /// Reidrata um acesso a partir de dados já persistidos (ex.: leitura da Infrastructure)
        /// — ver comentário equivalente em Aluno.Restaurar.
        ///
        /// Diferente de Criar, aceita a DataHora gravada em vez de carimbar DateTime.UtcNow:
        /// sem isso um repositório não teria como devolver o instante real do check-in, já que
        /// o construtor é privado e não há mutator para a propriedade.
        /// </summary>
        public static AcessoAluno Restaurar(int id, Aluno aluno, DateTime dataHora)
        {
            ArgumentNullException.ThrowIfNull(aluno);

            return new AcessoAluno(id, aluno, dataHora);
        }
    }
}
