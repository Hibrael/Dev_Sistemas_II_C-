//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Registro de uma ida do Colaborador à academia: o instante da chegada (DataHora) e, depois
    /// que ele passa pela catraca na volta, o instante da saída (DataHoraSaida).
    ///
    /// Os instantes são guardados em UTC (DateTime.UtcNow), como já era feito na entrada; a
    /// conversão para o horário local fica a cargo de quem exibe.
    /// </summary>
    public sealed class AcessoColaborador : Entity, IAggregateRoot
    {
        public Colaborador Colaborador { get; private set; }

        /// <summary>Instante da chegada (UTC).</summary>
        public DateTime DataHora { get; private set; }

        /// <summary>Instante da saída (UTC); nulo enquanto o colaborador ainda está na academia.</summary>
        public DateTime? DataHoraSaida { get; private set; }

        /// <summary>Verdadeiro enquanto a saída não foi registrada (o colaborador está na academia).</summary>
        public bool EmAberto => DataHoraSaida is null;

        /// <summary>Tempo que o colaborador permaneceu na academia; nulo enquanto não sai.</summary>
        public TimeSpan? Permanencia => DataHoraSaida - DataHora;

        private AcessoColaborador(int id, Colaborador colaborador, DateTime dataHora, DateTime? dataHoraSaida) : base(id)
        {
            Colaborador = colaborador;
            DataHora = dataHora;
            DataHoraSaida = dataHoraSaida;
        }

        public static AcessoColaborador Criar(int id, Colaborador colaborador)
        {
            ArgumentNullException.ThrowIfNull(colaborador);

            return new AcessoColaborador(id, colaborador, DateTime.UtcNow, null);
        }

        /// <summary>
        /// Reidrata um acesso a partir de dados já persistidos (ex.: leitura da Infrastructure)
        /// — ver comentário equivalente em Colaborador.Restaurar.
        ///
        /// Diferente de Criar, aceita a DataHora gravada em vez de carimbar DateTime.UtcNow:
        /// sem isso um repositório não teria como devolver o instante real do check-in, já que
        /// o construtor é privado e não há mutator para a propriedade.
        /// </summary>
        public static AcessoColaborador Restaurar(int id, Colaborador colaborador, DateTime dataHora, DateTime? dataHoraSaida = null)
        {
            ArgumentNullException.ThrowIfNull(colaborador);

            return new AcessoColaborador(id, colaborador, dataHora, dataHoraSaida);
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

        /// <summary>
        /// Soma o tempo de permanência de vários acessos (ex.: todos os do dia). Um acesso ainda
        /// em aberto conta até o instante de referência, para a soma refletir o tempo real.
        /// </summary>
        public static TimeSpan SomarPermanencia(IEnumerable<AcessoColaborador> acessos, DateTime referenciaUtc)
        {
            ArgumentNullException.ThrowIfNull(acessos);

            return acessos.Aggregate(TimeSpan.Zero, (total, acesso) =>
            {
                var fim = acesso.DataHoraSaida ?? referenciaUtc;
                return fim > acesso.DataHora ? total + (fim - acesso.DataHora) : total;
            });
        }
    }
}
