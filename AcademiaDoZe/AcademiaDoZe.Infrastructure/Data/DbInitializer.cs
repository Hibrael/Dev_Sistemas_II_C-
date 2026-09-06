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
                await GarantirColunaSalarioAsync(connection, databaseType, cancellationToken);
                BancosInicializados.TryAdd(key, true);
            }
            catch (DbException ex)
            {
                throw new InfrastructureException("ERRO_INICIALIZAR_BANCO", $"Erro ao inicializar banco de dados: {ex.Message}", ex);
            }
        }

        // "CREATE TABLE IF NOT EXISTS" (ou o equivalente com IF OBJECT_ID em SQL Server) não altera uma
        // tabela que já existe. Como tb_colaborador já fazia parte do script desde antes da coluna
        // "salario" ser adicionada, qualquer banco SQL Server/MySQL usado em testes anteriores ficaria
        // com a tabela desatualizada e o INSERT do ColaboradorRepository falharia. Este passo cobre
        // exatamente esse cenário, sem exigir apagar o banco manualmente. SQLite não precisa disso: o
        // TestBase recria o arquivo do zero a cada execução.
        private static async Task GarantirColunaSalarioAsync(DbConnection connection, DatabaseType databaseType, CancellationToken cancellationToken)
        {
            if (databaseType is not (DatabaseType.SqlServer or DatabaseType.MySql)) return;

            var verificacaoSql = databaseType == DatabaseType.SqlServer
                ? "SELECT COL_LENGTH('tb_colaborador', 'salario')"
                : "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'tb_colaborador' AND column_name = 'salario'";

            await using var verificacaoCommand = DbProvider.CreateCommand(verificacaoSql, connection);
            var resultado = await verificacaoCommand.ExecuteScalarAsync(cancellationToken);
            var colunaExiste = databaseType == DatabaseType.SqlServer ? resultado is not null and not DBNull : Convert.ToInt32(resultado) > 0;
            if (colunaExiste) return;

            await using var alterCommand = DbProvider.CreateCommand("ALTER TABLE tb_colaborador ADD salario DECIMAL(10,2) NOT NULL DEFAULT 0", connection);
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
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