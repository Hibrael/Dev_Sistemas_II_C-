//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Infrastructure.Exceptions;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
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
                await GarantirBancoAsync(connectionString, databaseType, cancellationToken);
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

        private static async Task GarantirBancoAsync(string connectionString, DatabaseType databaseType, CancellationToken cancellationToken)
        {
            string nomeBanco;
            string conexaoServidor;

            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    var sqlBuilder = new SqlConnectionStringBuilder(connectionString);
                    nomeBanco = sqlBuilder.InitialCatalog;
                    sqlBuilder.InitialCatalog = "master";
                    conexaoServidor = sqlBuilder.ConnectionString;
                    break;

                case DatabaseType.MySql:
                    var mySqlBuilder = new MySqlConnectionStringBuilder(connectionString);
                    nomeBanco = mySqlBuilder.Database;
                    mySqlBuilder.Database = string.Empty;
                    conexaoServidor = mySqlBuilder.ConnectionString;
                    break;

                default:
                    return;
            }

            if (string.IsNullOrWhiteSpace(nomeBanco)) return;

            if (!nomeBanco.All(c => char.IsLetterOrDigit(c) || c == '_'))
                throw new InfrastructureException("NOME_BANCO_INVALIDO", $"Nome de banco inválido: '{nomeBanco}'.");

            var sql = databaseType == DatabaseType.SqlServer
                ? $"IF DB_ID(N'{nomeBanco}') IS NULL CREATE DATABASE [{nomeBanco}];"
                : $"CREATE DATABASE IF NOT EXISTS `{nomeBanco}`;";

            await using var connection = DbProvider.CreateConnection(conexaoServidor, databaseType);
            await connection.OpenAsync(cancellationToken);
            await using var command = DbProvider.CreateCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
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