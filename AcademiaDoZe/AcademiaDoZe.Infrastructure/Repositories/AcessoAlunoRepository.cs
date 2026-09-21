//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories
{
    /// <summary>
    /// Persiste os acessos de aluno na tabela polimórfica tb_acesso, filtrando sempre por
    /// pessoa_tipo = Aluno — ver PessoaTipo. Todas as consultas carregam o aluno completo pelo
    /// mesmo JOIN usado em AlunoRepository, e a reconstrução da entidade Aluno é delegada a
    /// AlunoRepository.Map para não duplicar aqui a lógica de Value Objects.
    /// </summary>
    public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
    {
        // Os nomes/aliases das colunas do aluno e do logradouro precisam bater exatamente com
        // os que AlunoRepository.Map espera ler.
        private const string BaseSelectQuery = "SELECT ac.id_acesso, ac.data_hora, a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_acesso ac INNER JOIN tb_aluno a ON ac.pessoa_id = a.id_aluno INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

        public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

        public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE ac.id_acesso = @Id AND ac.pessoa_tipo = @PessoaTipo", c => { c.AddParameter("@Id", id, DbType.Int32); AddPessoaTipo(c); }, cancellationToken);

        public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE ac.pessoa_tipo = @PessoaTipo ORDER BY ac.data_hora DESC", AddPessoaTipo, cancellationToken);

        private async Task<AcessoAluno?> QueryOne(string sql, Action<DbCommand> addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_ACESSO_ALUNO", ex.Message, ex); }
        }

        private async Task<IEnumerable<AcessoAluno>> QueryMany(string sql, Action<DbCommand>? addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters?.Invoke(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<AcessoAluno>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return result; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_ACESSOS_ALUNO", ex.Message, ex); }
        }

        public static AcessoAluno Map(DbDataReader reader)
        {
            try
            {
                var aluno = AlunoRepository.Map(reader);

                // Restaurar (e não Criar) porque a DataHora tem de ser a gravada no banco:
                // Criar carimbaria DateTime.UtcNow e perderia o instante real do check-in.
                return AcessoAluno.Restaurar(reader.GetInt32Value("id_acesso"), aluno, reader.GetDateTimeValue("data_hora"));
            }
            catch (InfrastructureException) { throw; }
            catch (Exception ex) { throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", ex.Message, ex); }
        }

        public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)";
            try
            {
                await using var command = await CreateCommandAsync(FormatInsertQuery(sql), cancellationToken);
                AddParameters(command, entity);
                var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter ID inserido para o acesso do aluno.", cancellationToken);

                // A entidade é imutável (Id com setter protegido e sem mutator), então devolve-se
                // uma nova instância já com o Id gerado, em vez de escrever na propriedade.
                return AcessoAluno.Restaurar(id, entity.Aluno, entity.DataHora);
            }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", ex.Message, ex); }
        }

        public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            try
            {
                await using var command = await CreateCommandAsync(sql, cancellationToken);
                command.AddParameter("@Id", entity.Id, DbType.Int32);
                AddParameters(command, entity);
                if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com ID {entity.Id} para atualização.");
                return entity;
            }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", ex.Message, ex); }
        }

        private static void AddParameters(DbCommand command, AcessoAluno entity)
        {
            AddPessoaTipo(command);
            command.AddParameter("@PessoaId", entity.Aluno.Id, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        }

        private static void AddPessoaTipo(DbCommand command) => command.AddParameter("@PessoaTipo", (int)PessoaTipo.Aluno, DbType.Int32);

        public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo", cancellationToken);
                command.AddParameter("@Id", id, DbType.Int32);
                AddPessoaTipo(command);
                return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
            }
            catch (DbException ex) { throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", ex.Message, ex); }
        }
    }
}
