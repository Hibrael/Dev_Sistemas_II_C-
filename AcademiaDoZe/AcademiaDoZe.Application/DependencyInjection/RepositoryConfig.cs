//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.DependencyInjection;

/// <summary>
/// Dados de conexão que o compositor da aplicação registra no container para que
/// ApplicationDependencyInjection consiga construir os repositórios concretos sob demanda.
/// </summary>
public class RepositoryConfig
{
    public required string ConnectionString { get; set; }
    public required DatabaseType DatabaseType { get; set; }
}
