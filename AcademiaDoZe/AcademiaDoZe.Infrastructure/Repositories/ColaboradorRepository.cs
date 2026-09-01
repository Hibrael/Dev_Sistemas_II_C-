//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class ColaboradorRepository : BaseRepository, IColaboradorRepository
    {
        // Nome fixo usado ao reidratar a foto a partir do banco: a tabela guarda apenas os
        // bytes (BLOB), sem nome/extensão original, e Arquivo.Criar exige um nome válido.
        private const string FotoNomePadrao = "foto.jpg";
        private const string BaseSelectQuery = "SELECT c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo, c.salario, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_colaborador c INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";
        public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

        public async Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE c.id_colaborador = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE c.cpf = @Cpf", c => c.AddParameter("@Cpf", cpf.Numero, DbType.String), cancellationToken);
        public async Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE c.email = @Email", c => c.AddParameter("@Email", email.EnderecoEmail, DbType.String), cancellationToken);

        public async Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} ORDER BY c.nome", null, cancellationToken);
        public async Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE c.tipo = @Tipo ORDER BY c.nome", c => c.AddParameter("@Tipo", (int)tipo, DbType.Int32), cancellationToken);
        public async Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE c.vinculo = @Vinculo ORDER BY c.nome", c => c.AddParameter("@Vinculo", (int)vinculo, DbType.Int32), cancellationToken);

        private async Task<Colaborador?> QueryOne(string sql, Action<DbCommand> addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_COLABORADOR", ex.Message, ex); }
        }

        private async Task<IEnumerable<Colaborador>> QueryMany(string sql, Action<DbCommand>? addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters?.Invoke(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<Colaborador>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return result; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_COLABORADORES", ex.Message, ex); }
        }

        public static Colaborador Map(DbDataReader reader)
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
                    throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", "Falha ao reconstruir CPF/telefone/e-mail do colaborador a partir do banco.");

                return Colaborador.Restaurar(
                    reader.GetInt32Value("id_colaborador"),
                    reader.GetStringValue("nome"),
                    cpfResult.Value!,
                    reader.GetDateOnlyValue("nascimento"),
                    telefoneResult.Value!,
                    emailResult.Value!,
                    enderecoResult.Value!,
                    senha,
                    fotoResult.Value!,
                    reader.GetDateOnlyValue("admissao"),
                    (ColaboradorTipo)reader.GetInt32Value("tipo"),
                    (ColaboradorVinculo)reader.GetInt32Value("vinculo"),
                    reader.GetDecimalValue("salario"));
            }
            catch (InfrastructureException) { throw; }
            catch (Exception ex) { throw new InfrastructureException("ERRO_MAPEAMENTO_COLABORADOR", ex.Message, ex); }
        }

        // A coluna "senha" guarda "{hash}|{salt}" (Base64 nunca contém '|', então é um
        // separador seguro), já que o schema tem uma única coluna de senha.
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

        public async Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo, salario) VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo, @Salario)";
            try { await using var command = await CreateCommandAsync(FormatInsertQuery(sql), cancellationToken); AddParameters(command, entity); var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_COLABORADOR", "Falha ao obter ID inserido para o colaborador.", cancellationToken); typeof(Entity).GetProperty("Id")?.SetValue(entity, id); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_COLABORADOR", ex.Message, ex); }
        }

        public async Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE tb_colaborador SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, senha = @Senha, foto = @Foto, admissao = @Admissao, tipo = @Tipo, vinculo = @Vinculo, salario = @Salario WHERE id_colaborador = @Id";
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); command.AddParameter("@Id", entity.Id, DbType.Int32); AddParameters(command, entity); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum colaborador encontrado com ID {entity.Id} para atualização."); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_COLABORADOR", ex.Message, ex); }
        }

        private static void AddParameters(DbCommand command, Colaborador entity)
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
            command.AddParameter("@Admissao", entity.DataAdmissao, DbType.Date);
            command.AddParameter("@Tipo", (int)entity.Tipo, DbType.Int32);
            command.AddParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32);
            command.AddParameter("@Salario", entity.Salario, DbType.Decimal);
        }

        public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) => await ExecuteBoolean("DELETE FROM tb_colaborador WHERE id_colaborador = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default) => await ExecuteCount("SELECT COUNT(1) FROM tb_colaborador WHERE cpf = @Cpf AND (@Id IS NULL OR id_colaborador <> @Id)", c => { c.AddParameter("@Cpf", cpf.Numero, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);
        public async Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default) => await ExecuteCount("SELECT COUNT(1) FROM tb_colaborador WHERE email = @Email AND (@Id IS NULL OR id_colaborador <> @Id)", c => { c.AddParameter("@Email", email.EnderecoEmail, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);

        public async Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default)
        {
            try { await using var command = await CreateCommandAsync("UPDATE tb_colaborador SET senha = @Senha WHERE id_colaborador = @Id", cancellationToken); command.AddParameter("@Id", id, DbType.Int32); command.AddParameter("@Senha", novaSenha.TextoPlano, DbType.String); return await command.ExecuteNonQueryAsync(cancellationToken) > 0; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_TROCAR_SENHA", ex.Message, ex); }
        }

        private async Task<bool> ExecuteBoolean(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return await c.ExecuteNonQueryAsync(token) > 0; }
        private async Task<bool> ExecuteCount(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return Convert.ToInt32(await c.ExecuteScalarAsync(token)) > 0; }
    }
}
