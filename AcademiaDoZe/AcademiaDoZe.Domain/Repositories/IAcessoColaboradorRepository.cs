//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories
{
    public interface IAcessoColaboradorRepository
    {
        Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default);
        Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default);
        Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default);
        Task<bool> Remover(int id, CancellationToken cancellationToken = default);

        /// <summary>Acesso do colaborador ainda sem saída (ele está na academia), se houver.</summary>
        Task<AcessoColaborador?> ObterEmAbertoPorColaborador(int colaboradorId, CancellationToken cancellationToken = default);

        /// <summary>Todos os acessos ainda sem saída: os colaboradores que estão na academia agora.</summary>
        Task<IEnumerable<AcessoColaborador>> ObterEmAberto(CancellationToken cancellationToken = default);

        /// <summary>
        /// Acessos do colaborador cuja entrada está no intervalo [inicio, fim) — usado para somar
        /// a permanência do dia. Os limites seguem o mesmo fuso gravado em data_hora (UTC).
        /// </summary>
        Task<IEnumerable<AcessoColaborador>> ObterPorColaboradorNoPeriodo(int colaboradorId, DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
    }
}
