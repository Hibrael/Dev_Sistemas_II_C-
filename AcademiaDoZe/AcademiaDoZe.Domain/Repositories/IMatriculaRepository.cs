//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Repositories
{
    public interface IMatriculaRepository
    {
        Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default);
        Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default);
        Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default);
        Task<bool> Remover(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default);
        Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default);
        Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default);
        Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default);
        Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default);
    }
}
