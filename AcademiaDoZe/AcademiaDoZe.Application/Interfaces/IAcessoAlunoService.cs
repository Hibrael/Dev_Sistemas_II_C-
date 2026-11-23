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

        /// <summary>
        /// Catraca — entrada do aluno: exige matrícula ativa (senão "Acesso negado") e informa
        /// quantos dias ainda restam de plano.
        /// </summary>
        Task<RegistroAcessoResultadoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Catraca — saída do aluno: fecha o acesso em aberto do mesmo aluno que entrou e informa
        /// o tempo que ele permaneceu na academia.
        /// </summary>
        Task<RegistroAcessoResultadoDto> RegistrarSaidaAsync(int alunoId, CancellationToken cancellationToken = default);

        /// <summary>Alunos que entraram e ainda não saíram.</summary>
        Task<IEnumerable<AcessoAlunoDto>> ObterEmAbertoAsync(CancellationToken cancellationToken = default);
    }
}
