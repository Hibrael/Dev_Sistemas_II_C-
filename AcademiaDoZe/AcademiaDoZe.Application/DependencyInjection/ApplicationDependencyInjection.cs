//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Registra os serviços da camada de aplicação e as fábricas de repositório que eles
    /// consomem. Espera encontrar um RepositoryConfig já registrado no container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Registra os serviços da camada de aplicação.
        // AddScoped: uma instância por requisição HTTP.
        // AddSingleton: uma única instância durante toda a vida útil da aplicação.
        // AddTransient: uma nova instância a cada vez que o serviço é solicitado.
        services.AddTransient<ILogradouroService, LogradouroService>();
        services.AddTransient<IColaboradorService, ColaboradorService>();
        services.AddTransient<IAlunoService, AlunoService>();
        services.AddTransient<IMatriculaService, MatriculaService>();
        services.AddTransient<IAcessoAlunoService, AcessoAlunoService>();
        services.AddTransient<IAcessoColaboradorService, AcessoColaboradorService>();

        // Registra as fábricas Func<IRepo> para criar instâncias sob demanda nos services.
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<ILogradouroRepository>)(() => new LogradouroRepository(config.ConnectionString, config.DatabaseType));
        });

        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IColaboradorRepository>)(() => new ColaboradorRepository(config.ConnectionString, config.DatabaseType));
        });

        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IAlunoRepository>)(() => new AlunoRepository(config.ConnectionString, config.DatabaseType));
        });

        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IMatriculaRepository>)(() => new MatriculaRepository(config.ConnectionString, config.DatabaseType));
        });

        // As fábricas de IAcessoAlunoRepository e IAcessoColaboradorRepository ainda não são
        // registradas: a Infrastructure não possui AcessoAlunoRepository nem
        // AcessoColaboradorRepository. Enquanto isso, resolver IAcessoAlunoService ou
        // IAcessoColaboradorService falha no container — de propósito, para não mascarar a
        // pendência. Os dois repositórios entram junto com a Avaliação 03.

        return services;
    }
}
