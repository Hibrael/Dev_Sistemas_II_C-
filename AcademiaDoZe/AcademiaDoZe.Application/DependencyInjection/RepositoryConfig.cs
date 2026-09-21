//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.DependencyInjection
{
    public sealed class RepositoryConfig
    {
        public Func<IAcessoAlunoRepository> AcessoAlunoRepositoryFactory { get; set; } = () => throw new InvalidOperationException("A factory for IAcessoAlunoRepository was not configured.");
        public Func<IAcessoColaboradorRepository> AcessoColaboradorRepositoryFactory { get; set; } = () => throw new InvalidOperationException("A factory for IAcessoColaboradorRepository was not configured.");
    }
}
