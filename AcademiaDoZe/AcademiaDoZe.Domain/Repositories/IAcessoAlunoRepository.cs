//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories
{
    public interface IAcessoAlunoRepository
    {
        Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default);
        Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default);
        Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default);
        Task<bool> Remover(int id, CancellationToken cancellationToken = default);
    }
}
