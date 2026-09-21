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
    }
}
