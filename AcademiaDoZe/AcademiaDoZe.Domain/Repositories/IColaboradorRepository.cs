//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Repositories
{
    public interface IColaboradorRepository
    {
        Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default);
        Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default);
        Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default);
        Task<bool> Remover(int id, CancellationToken cancellationToken = default);
        Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default);
        Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default);
        Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default);
        Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default);
        Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default);
        Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default);
    }
}
