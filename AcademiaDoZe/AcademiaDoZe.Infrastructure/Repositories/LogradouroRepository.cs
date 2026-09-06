//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class LogradouroRepository : BaseRepository, ILogradouroRepository
    {
        private const string BaseSelectQuery = "SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais FROM tb_logradouro";
        public LogradouroRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

        public async Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE id_logradouro = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE cep = @Cep", c => c.AddParameter("@Cep", cep.Numero, DbType.String), cancellationToken);

        public async Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} ORDER BY nome", null, cancellationToken);
        public async Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE cidade = @Cidade ORDER BY bairro, nome", c => c.AddParameter("@Cidade", cidade, DbType.String), cancellationToken);
        public async Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE cidade = @Cidade AND bairro = @Bairro ORDER BY nome", c => { c.AddParameter("@Cidade", cidade, DbType.String); c.AddParameter("@Bairro", bairro, DbType.String); }, cancellationToken);

        private async Task<Logradouro?> QueryOne(string sql, Action<DbCommand> addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_LOGRADOURO", ex.Message, ex); }
        }

        private async Task<IEnumerable<Logradouro>> QueryMany(string sql, Action<DbCommand>? addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters?.Invoke(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<Logradouro>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return result; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_LOGRADOUROS", ex.Message, ex); }
        }

        public static Logradouro Map(DbDataReader reader, string nomeColumn = "nome")
        {
            try
            {
                var result = Logradouro.Criar(reader.GetInt32Value("id_logradouro"), reader.GetStringValue("cep"), reader.GetStringValue(nomeColumn), reader.GetStringValue("bairro"), reader.GetStringValue("cidade"), reader.GetStringValue("estado"), reader.GetStringValue("pais"));
                if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notificacoes.Select(n => n.Mensagem)));
                return result.Value!;
            }
            catch (InfrastructureException) { throw; }
            catch (Exception ex) { throw new InfrastructureException("ERRO_MAPEAMENTO_LOGRADOURO", ex.Message, ex); }
        }

        public async Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais) VALUES (@Cep, @Nome, @Bairro, @Cidade, @Estado, @Pais)";
            try { await using var command = await CreateCommandAsync(FormatInsertQuery(sql), cancellationToken); AddParameters(command, entity); var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_LOGRADOURO", "Falha ao obter ID inserido para o logradouro.", cancellationToken); typeof(Entity).GetProperty("Id")?.SetValue(entity, id); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_LOGRADOURO", ex.Message, ex); }
        }

        public async Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE tb_logradouro SET cep = @Cep, nome = @Nome, bairro = @Bairro, cidade = @Cidade, estado = @Estado, pais = @Pais WHERE id_logradouro = @Id";
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); command.AddParameter("@Id", entity.Id, DbType.Int32); AddParameters(command, entity); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum logradouro encontrado com ID {entity.Id} para atualização."); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_LOGRADOURO", ex.Message, ex); }
        }

        private static void AddParameters(DbCommand command, Logradouro entity) { command.AddParameter("@Cep", entity.Cep.Numero, DbType.String); command.AddParameter("@Nome", entity.Nome, DbType.String); command.AddParameter("@Bairro", entity.Bairro, DbType.String); command.AddParameter("@Cidade", entity.Cidade, DbType.String); command.AddParameter("@Estado", entity.Estado, DbType.String); command.AddParameter("@Pais", entity.Pais, DbType.String); }

        public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) => await ExecuteBoolean("DELETE FROM tb_logradouro WHERE id_logradouro = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default) => await ExecuteCount("SELECT COUNT(1) FROM tb_logradouro WHERE cep = @Cep AND (@Id IS NULL OR id_logradouro <> @Id)", c => { c.AddParameter("@Cep", cep.Numero, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);
        private async Task<bool> ExecuteBoolean(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return await c.ExecuteNonQueryAsync(token) > 0; }
        private async Task<bool> ExecuteCount(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return Convert.ToInt32(await c.ExecuteScalarAsync(token)) > 0; }
    }
}