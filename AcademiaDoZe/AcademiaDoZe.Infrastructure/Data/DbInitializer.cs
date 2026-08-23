//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDoZe.Infrastructure.Data
{
    public static class DbInitializer
    {
        private static readonly ConcurrentDictionary<string, bool> BancosInicializados = new();

        public static async Task InicializarAsync(string connectionString, DatabaseType databaseType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return;
            var key = $"{databaseType}:{connectionString}";
            if (BancosInicializados.ContainsKey(key)) return;
            try
            {
                await using var connection = DbProvider.CreateConnection(connectionString, databaseType);
                await connection.OpenAsync(cancellationToken);
                await using var command = DbProvider.CreateCommand(ObterScript(databaseType), connection);
                await command.ExecuteNonQueryAsync(cancellationToken);
                BancosInicializados.TryAdd(key, true);
            }
            catch (DbException ex)
            {
                throw new InfrastructureException("ERRO_INICIALIZAR_BANCO", $"Erro ao inicializar banco de dados: {ex.Message}", ex);
            }
        }

        public static string ObterScript(DatabaseType databaseType)
        {
            var scriptName = DbProvider.GetScriptName(databaseType);
            var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(scriptName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InfrastructureException("SCRIPT_EMBARCADO_NAO_ENCONTRADO", $"Script SQL embarcado '{scriptName}' não encontrado.");
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                ?? throw new InfrastructureException("ERRO_LEITURA_SCRIPT", $"Erro ao carregar o script '{scriptName}'.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}