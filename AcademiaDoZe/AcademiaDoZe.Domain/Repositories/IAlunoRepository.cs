//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Repositories
{
    public interface IAlunoRepository
    {
        Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default);
        Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default);
        Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default);
        Task<bool> Remover(int id, CancellationToken cancellationToken = default);
        Task<Aluno?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default);
        Task<Aluno?> ObterPorEmail(Email email, CancellationToken cancellationToken = default);
        Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default);
        Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default);
        Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default);
    }
}
