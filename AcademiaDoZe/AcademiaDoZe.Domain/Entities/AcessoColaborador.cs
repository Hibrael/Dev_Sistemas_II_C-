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
    }
}
