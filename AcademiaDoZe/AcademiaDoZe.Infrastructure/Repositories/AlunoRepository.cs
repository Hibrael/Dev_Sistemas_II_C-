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
    public class AlunoRepository : BaseRepository, IAlunoRepository
    {
        // Ver comentário equivalente em ColaboradorRepository.
        private const string FotoNomePadrao = "foto.jpg";
        private const string BaseSelectQuery = "SELECT a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_aluno a INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";
        public AlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

        public async Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE a.id_aluno = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<Aluno?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE a.cpf = @Cpf", c => c.AddParameter("@Cpf", cpf.Numero, DbType.String), cancellationToken);
        public async Task<Aluno?> ObterPorEmail(Email email, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE a.email = @Email", c => c.AddParameter("@Email", email.EnderecoEmail, DbType.String), cancellationToken);

        public async Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} ORDER BY a.nome", null, cancellationToken);

        private async Task<Aluno?> QueryOne(string sql, Action<DbCommand> addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_ALUNO", ex.Message, ex); }
        }

        private async Task<IEnumerable<Aluno>> QueryMany(string sql, Action<DbCommand>? addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters?.Invoke(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<Aluno>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return result; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_ALUNOS", ex.Message, ex); }
        }

        public static Aluno Map(DbDataReader reader)
        {
            try
            {
                var logradouro = LogradouroRepository.Map(reader, "logradouro_nome");
                var complemento = reader["complemento"] is DBNull ? null : reader.GetStringValue("complemento");
                var enderecoResult = Endereco.Criar(logradouro, reader.GetStringValue("numero"), complemento);
                if (enderecoResult.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", enderecoResult.Notificacoes.Select(n => n.Mensagem)));

                var (hash, salt, textoPlano) = SplitSenha(reader.GetStringValue("senha"));
                var senha = Senha.Restaurar(hash, salt, textoPlano);

                var fotoResult = Arquivo.Criar(FotoNomePadrao, reader.GetNullableBytes("foto") ?? []);
                if (fotoResult.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", fotoResult.Notificacoes.Select(n => n.Mensagem)));

                var cpfResult = Cpf.Criar(reader.GetStringValue("cpf"));
                var telefoneResult = Telefone.Criar(reader.GetStringValue("telefone"));
                var emailResult = Email.Criar(reader.GetStringValue("email"));
                if (cpfResult.IsFailure || telefoneResult.IsFailure || emailResult.IsFailure)
                    throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", "Falha ao reconstruir CPF/telefone/e-mail do aluno a partir do banco.");

                return Aluno.Restaurar(
                    reader.GetInt32Value("id_aluno"),
                    reader.GetStringValue("nome"),
                    cpfResult.Value!,
                    reader.GetDateOnlyValue("nascimento"),
                    telefoneResult.Value!,
                    emailResult.Value!,
                    enderecoResult.Value!,
                    senha,
                    fotoResult.Value!);
            }
            catch (InfrastructureException) { throw; }
            catch (Exception ex) { throw new InfrastructureException("ERRO_MAPEAMENTO_ALUNO", ex.Message, ex); }
        }

        private static (string Hash, string Salt, string TextoPlano) SplitSenha(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new InfrastructureException("ERRO_FORMATO_SENHA", "Valor de senha armazenado em formato inesperado.");

            if (!valor.Contains('|'))
                return (valor, string.Empty, valor);

            var partes = valor.Split('|', 2);
            if (partes.Length != 2) throw new InfrastructureException("ERRO_FORMATO_SENHA", "Valor de senha armazenado em formato inesperado.");
            return (partes[0], partes[1], partes[0]);
        }

        public async Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO tb_aluno (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto) VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto)";
            try { await using var command = await CreateCommandAsync(FormatInsertQuery(sql), cancellationToken); AddParameters(command, entity); var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ALUNO", "Falha ao obter ID inserido para o aluno.", cancellationToken); typeof(Entity).GetProperty("Id")?.SetValue(entity, id); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_ALUNO", ex.Message, ex); }
        }

        public async Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE tb_aluno SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, senha = @Senha, foto = @Foto WHERE id_aluno = @Id";
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); command.AddParameter("@Id", entity.Id, DbType.Int32); AddParameters(command, entity); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum aluno encontrado com ID {entity.Id} para atualização."); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_ALUNO", ex.Message, ex); }
        }

        private static void AddParameters(DbCommand command, Aluno entity)
        {
            command.AddParameter("@Cpf", entity.Cpf.Numero, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
            command.AddParameter("@Telefone", entity.Telefone.Numero, DbType.String);
            command.AddParameter("@Email", entity.Email.EnderecoEmail, DbType.String);
            command.AddParameter("@LogradouroId", entity.Endereco.Logradouro.Id, DbType.Int32);
            command.AddParameter("@Numero", entity.Endereco.NumeroCasa, DbType.String);
            command.AddParameter("@Complemento", entity.Endereco.Complemento, DbType.String);
            command.AddParameter("@Senha", entity.Senha.TextoPlano, DbType.String);
            command.AddParameter("@Foto", entity.Foto.Conteudo, DbType.Binary);
        }

        public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) => await ExecuteBoolean("DELETE FROM tb_aluno WHERE id_aluno = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default) => await ExecuteCount("SELECT COUNT(1) FROM tb_aluno WHERE cpf = @Cpf AND (@Id IS NULL OR id_aluno <> @Id)", c => { c.AddParameter("@Cpf", cpf.Numero, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);
        public async Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default) => await ExecuteCount("SELECT COUNT(1) FROM tb_aluno WHERE email = @Email AND (@Id IS NULL OR id_aluno <> @Id)", c => { c.AddParameter("@Email", email.EnderecoEmail, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);

        public async Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default)
        {
            try { await using var command = await CreateCommandAsync("UPDATE tb_aluno SET senha = @Senha WHERE id_aluno = @Id", cancellationToken); command.AddParameter("@Id", id, DbType.Int32); command.AddParameter("@Senha", novaSenha.TextoPlano, DbType.String); return await command.ExecuteNonQueryAsync(cancellationToken) > 0; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_TROCAR_SENHA", ex.Message, ex); }
        }

        private async Task<bool> ExecuteBoolean(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return await c.ExecuteNonQueryAsync(token) > 0; }
        private async Task<bool> ExecuteCount(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return Convert.ToInt32(await c.ExecuteScalarAsync(token)) > 0; }
    }
}
