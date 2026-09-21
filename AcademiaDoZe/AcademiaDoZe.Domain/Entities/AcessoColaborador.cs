//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>Registro de um check-in/acesso de um Colaborador à academia.</summary>
    public sealed class AcessoColaborador : Entity, IAggregateRoot
    {
        public Colaborador Colaborador { get; private set; }
        public DateTime DataHora { get; private set; }

        private AcessoColaborador(int id, Colaborador colaborador, DateTime dataHora) : base(id)
        {
            Colaborador = colaborador;
            DataHora = dataHora;
        }

        public static AcessoColaborador Criar(int id, Colaborador colaborador)
        {
            ArgumentNullException.ThrowIfNull(colaborador);

            return new AcessoColaborador(id, colaborador, DateTime.UtcNow);
        }

        /// <summary>
        /// Reidrata um acesso a partir de dados já persistidos (ex.: leitura da Infrastructure)
        /// — ver comentário equivalente em Colaborador.Restaurar.
        ///
        /// Diferente de Criar, aceita a DataHora gravada em vez de carimbar DateTime.UtcNow:
        /// sem isso um repositório não teria como devolver o instante real do check-in, já que
        /// o construtor é privado e não há mutator para a propriedade.
        /// </summary>
        public static AcessoColaborador Restaurar(int id, Colaborador colaborador, DateTime dataHora)
        {
            ArgumentNullException.ThrowIfNull(colaborador);

            return new AcessoColaborador(id, colaborador, dataHora);
        }
    }
}
