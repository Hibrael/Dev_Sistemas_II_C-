//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces
{
    public interface IAcessoColaboradorService
    {
        Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto dto, CancellationToken cancellationToken = default);
        Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto dto, CancellationToken cancellationToken = default);
        Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Catraca — entrada do colaborador: nega quando a soma dos acessos do dia já atingiu a
        /// jornada máxima do vínculo (8 h CLT/PJ, 6 h estágio).
        /// </summary>
        Task<RegistroAcessoResultadoDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Catraca — saída do colaborador: fecha o acesso em aberto do mesmo colaborador que entrou
        /// e informa o tempo de permanência somando todos os registros do dia.
        /// </summary>
        Task<RegistroAcessoResultadoDto> RegistrarSaidaAsync(int colaboradorId, CancellationToken cancellationToken = default);

        /// <summary>Colaboradores que entraram e ainda não saíram.</summary>
        Task<IEnumerable<AcessoColaboradorDto>> ObterEmAbertoAsync(CancellationToken cancellationToken = default);
    }
}
