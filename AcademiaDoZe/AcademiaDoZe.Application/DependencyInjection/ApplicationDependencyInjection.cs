//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, RepositoryConfig repositoryConfig)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(repositoryConfig);

            services.AddTransient<Func<IAcessoAlunoRepository>>(_ => repositoryConfig.AcessoAlunoRepositoryFactory);
            services.AddTransient<Func<IAcessoColaboradorRepository>>(_ => repositoryConfig.AcessoColaboradorRepositoryFactory);

            services.AddTransient<IAcessoAlunoService, AcessoAlunoService>();
            services.AddTransient<IAcessoColaboradorService, AcessoColaboradorService>();

            return services;
        }
    }
}
