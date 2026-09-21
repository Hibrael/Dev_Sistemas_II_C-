//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces
{
    public interface IAcessoAlunoService
    {
        Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<AcessoAlunoDto> AdicionarAsync(AcessoAlunoDto dto, CancellationToken cancellationToken = default);
        Task<AcessoAlunoDto> AtualizarAsync(AcessoAlunoDto dto, CancellationToken cancellationToken = default);
        Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    }
}
