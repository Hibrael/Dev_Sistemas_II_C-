//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Repositories
{
    public interface ILogradouroRepository
    {
        Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default);
        Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default);
        Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default);
        Task<bool> Remover(int id, CancellationToken cancellationToken = default);
        Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default);
        Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default);
        Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default);
    }
}