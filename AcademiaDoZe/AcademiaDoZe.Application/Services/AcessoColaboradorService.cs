//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services
{
    public sealed class AcessoColaboradorService : IAcessoColaboradorService
    {
        private readonly Func<IAcessoColaboradorRepository> _repoFactory;

        public AcessoColaboradorService(Func<IAcessoColaboradorRepository> repoFactory)
        {
            _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        }

        public async Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador do acesso do colaborador deve ser maior que zero.");

            var repository = _repoFactory();
            var entity = await repository.ObterPorId(id, cancellationToken);
            return entity?.ToDto();
        }

        public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var repository = _repoFactory();
            var entities = await repository.ObterTodos(cancellationToken);
            return entities.Select(entity => entity.ToDto()).ToList();
        }

        public async Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = dto.ToEntity();
            var repository = _repoFactory();
            var result = await repository.Adicionar(entity, cancellationToken);
            return result.ToDto();
        }

        public async Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var entity = dto.ToEntity();
            var repository = _repoFactory();
            var result = await repository.Atualizar(entity, cancellationToken);
            return result.ToDto();
        }

        public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador do acesso do colaborador deve ser maior que zero.");

            var repository = _repoFactory();
            return await repository.Remover(id, cancellationToken);
        }
    }
}
