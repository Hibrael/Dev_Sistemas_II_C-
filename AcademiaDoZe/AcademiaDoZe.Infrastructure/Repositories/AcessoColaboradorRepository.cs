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
    /// Persiste os acessos de colaborador na tabela polimórfica tb_acesso, filtrando sempre por
    /// pessoa_tipo = Colaborador — ver PessoaTipo. A reconstrução da entidade Colaborador é
    /// delegada a ColaboradorRepository.Map para não duplicar aqui a lógica de Value Objects.
    /// </summary>
    public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
    {
        // Os nomes/aliases das colunas do colaborador e do logradouro precisam bater exatamente
        // com os que ColaboradorRepository.Map espera ler.
        private const string BaseSelectQuery = "SELECT ac.id_acesso, ac.data_hora, c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo, c.salario, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_acesso ac INNER JOIN tb_colaborador c ON ac.pessoa_id = c.id_colaborador INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

        public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

        public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE ac.id_acesso = @Id AND ac.pessoa_tipo = @PessoaTipo", c => { c.AddParameter("@Id", id, DbType.Int32); AddPessoaTipo(c); }, cancellationToken);

        public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE ac.pessoa_tipo = @PessoaTipo ORDER BY ac.data_hora DESC", AddPessoaTipo, cancellationToken);

        private async Task<AcessoColaborador?> QueryOne(string sql, Action<DbCommand> addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_ACESSO_COLABORADOR", ex.Message, ex); }
        }

        private async Task<IEnumerable<AcessoColaborador>> QueryMany(string sql, Action<DbCommand>? addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters?.Invoke(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<AcessoColaborador>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return result; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_ACESSOS_COLABORADOR", ex.Message, ex); }
        }

        public static AcessoColaborador Map(DbDataReader reader)
        {
            try
            {
                var colaborador = ColaboradorRepository.Map(reader);

                // Restaurar (e não Criar) porque a DataHora tem de ser a gravada no banco:
                // Criar carimbaria DateTime.UtcNow e perderia o instante real do check-in.
                return AcessoColaborador.Restaurar(reader.GetInt32Value("id_acesso"), colaborador, reader.GetDateTimeValue("data_hora"));
            }
            catch (InfrastructureException) { throw; }
            catch (Exception ex) { throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_COLABORADOR", ex.Message, ex); }
        }

        public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)";
            try
            {
                await using var command = await CreateCommandAsync(FormatInsertQuery(sql), cancellationToken);
                AddParameters(command, entity);
                var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_COLABORADOR", "Falha ao obter ID inserido para o acesso do colaborador.", cancellationToken);

                // A entidade é imutável (Id com setter protegido e sem mutator), então devolve-se
                // uma nova instância já com o Id gerado, em vez de escrever na propriedade.
                return AcessoColaborador.Restaurar(id, entity.Colaborador, entity.DataHora);
            }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_COLABORADOR", ex.Message, ex); }
        }

        public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            try
            {
                await using var command = await CreateCommandAsync(sql, cancellationToken);
                command.AddParameter("@Id", entity.Id, DbType.Int32);
                AddParameters(command, entity);
                if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de colaborador encontrado com ID {entity.Id} para atualização.");
                return entity;
            }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_COLABORADOR", ex.Message, ex); }
        }

        private static void AddParameters(DbCommand command, AcessoColaborador entity)
        {
            AddPessoaTipo(command);
            command.AddParameter("@PessoaId", entity.Colaborador.Id, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        }

        private static void AddPessoaTipo(DbCommand command) => command.AddParameter("@PessoaTipo", (int)PessoaTipo.Colaborador, DbType.Int32);

        public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo", cancellationToken);
                command.AddParameter("@Id", id, DbType.Int32);
                AddPessoaTipo(command);
                return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
            }
            catch (DbException ex) { throw new InfrastructureException("ERRO_REMOVER_ACESSO_COLABORADOR", ex.Message, ex); }
        }
    }
}
