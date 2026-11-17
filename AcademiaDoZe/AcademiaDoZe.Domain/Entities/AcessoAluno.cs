//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Registro de uma ida do Aluno à academia: o instante da chegada (DataHora) e, depois que
    /// ele passa pela catraca na volta, o instante da saída (DataHoraSaida).
    ///
    /// Os instantes são guardados em UTC (DateTime.UtcNow), como já era feito na entrada; a
    /// conversão para o horário local fica a cargo de quem exibe.
    /// </summary>
    public sealed class AcessoAluno : Entity, IAggregateRoot
    {
        public Aluno Aluno { get; private set; }

        /// <summary>Instante da chegada (UTC).</summary>
        public DateTime DataHora { get; private set; }

        /// <summary>Instante da saída (UTC); nulo enquanto o aluno ainda está na academia.</summary>
        public DateTime? DataHoraSaida { get; private set; }

        /// <summary>Verdadeiro enquanto a saída não foi registrada (o aluno está na academia).</summary>
        public bool EmAberto => DataHoraSaida is null;

        /// <summary>Tempo que o aluno permaneceu na academia; nulo enquanto não sai.</summary>
        public TimeSpan? Permanencia => DataHoraSaida - DataHora;

        private AcessoAluno(int id, Aluno aluno, DateTime dataHora, DateTime? dataHoraSaida) : base(id)
        {
            Aluno = aluno;
            DataHora = dataHora;
            DataHoraSaida = dataHoraSaida;
        }

        public static AcessoAluno Criar(int id, Aluno aluno)
        {
            ArgumentNullException.ThrowIfNull(aluno);

            return new AcessoAluno(id, aluno, DateTime.UtcNow, null);
        }

        /// <summary>
        /// Reidrata um acesso a partir de dados já persistidos (ex.: leitura da Infrastructure)
        /// — ver comentário equivalente em Aluno.Restaurar.
        ///
        /// Diferente de Criar, aceita a DataHora gravada em vez de carimbar DateTime.UtcNow:
        /// sem isso um repositório não teria como devolver o instante real do check-in, já que
        /// o construtor é privado e não há mutator para a propriedade.
        /// </summary>
        public static AcessoAluno Restaurar(int id, Aluno aluno, DateTime dataHora, DateTime? dataHoraSaida = null)
        {
            ArgumentNullException.ThrowIfNull(aluno);

            return new AcessoAluno(id, aluno, dataHora, dataHoraSaida);
        }

        /// <summary>
        /// Fecha a ida à academia carimbando a saída. Sem instante informado usa DateTime.UtcNow.
        /// Um acesso só pode ser fechado uma vez, e a saída não pode ser anterior à chegada.
        /// </summary>
        public void RegistrarSaida(DateTime? instante = null)
        {
            if (!EmAberto)
                throw new DomainException("A saída deste acesso já foi registrada.");

            var saida = instante ?? DateTime.UtcNow;
            if (saida < DataHora)
                throw new DomainException("A saída não pode ser anterior à entrada.");

            DataHoraSaida = saida;
        }
    }
}
