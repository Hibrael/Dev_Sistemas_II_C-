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
    public class MatriculaRepository : BaseRepository, IMatriculaRepository
    {
        // Nome fixo usado ao reidratar o laudo médico a partir do banco: a tabela guarda apenas
        // os bytes (BLOB), sem nome/extensão original, e Arquivo.Criar exige um nome válido
        // (mesma solução já usada para a foto em AlunoRepository/ColaboradorRepository).
        private const string LaudoNomePadrao = "laudo.jpg";
        private const string BaseSelectQuery = "SELECT m.id_matricula, m.aluno_id, m.plano, m.data_inicio, m.data_fim, m.objetivo, m.restricao_medica, m.obs_restricao, m.laudo_medico FROM tb_matricula m";
        public MatriculaRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

        public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default) => await QueryOne($"{BaseSelectQuery} WHERE m.id_matricula = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);
        public async Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} ORDER BY m.data_inicio DESC", null, cancellationToken);
        public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId ORDER BY m.data_inicio DESC", c => c.AddParameter("@AlunoId", alunoId, DbType.Int32), cancellationToken);
        public async Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default) => await QueryMany($"{BaseSelectQuery} WHERE m.plano = @Plano ORDER BY m.data_inicio DESC", c => c.AddParameter("@Plano", (int)plano, DbType.Int32), cancellationToken);

        // "Matrícula ativa" = data_fim ainda não vencida (ver comentário na entidade Matricula).
        // Comparar direto data_fim >= <data corrente do SGBD> evita LOWER()/UPPER() em coluna e
        // deixa o banco usar índice normal, além de já vir corrigido para o fuso/relógio do servidor.
        public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default) =>
            await QueryOne($"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId AND m.data_fim >= {GetCurrentDateFunction()} ORDER BY m.data_fim DESC", c => c.AddParameter("@AlunoId", alunoId, DbType.Int32), cancellationToken);

        public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default) =>
            await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken) is not null;

        public async Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
        {
            var query = $"{BaseSelectQuery} WHERE m.data_fim >= {GetCurrentDateFunction()} {(alunoId > 0 ? "AND m.aluno_id = @AlunoId" : "")} ORDER BY m.data_fim ASC";
            return await QueryMany(query, alunoId > 0 ? c => c.AddParameter("@AlunoId", alunoId, DbType.Int32) : null, cancellationToken);
        }

        public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
        {
            var dataAtual = GetCurrentDateFunction();
            var dataLimite = GetDateAddDaysExpression(dataAtual, "@Dias");
            var query = $"{BaseSelectQuery} WHERE m.data_fim >= {dataAtual} AND m.data_fim <= {dataLimite} ORDER BY m.data_fim ASC";
            return await QueryMany(query, c => c.AddParameter("@Dias", dias, DbType.Int32), cancellationToken);
        }

        private async Task<Matricula?> QueryOne(string sql, Action<DbCommand> addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); return await reader.ReadAsync(cancellationToken) ? Map(reader) : null; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_MATRICULA", ex.Message, ex); }
        }

        private async Task<IEnumerable<Matricula>> QueryMany(string sql, Action<DbCommand>? addParameters, CancellationToken cancellationToken)
        {
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); addParameters?.Invoke(command); await using var reader = await command.ExecuteReaderAsync(cancellationToken); var result = new List<Matricula>(); while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader)); return result; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_CONSULTAR_MATRICULAS", ex.Message, ex); }
        }

        public static Matricula Map(DbDataReader reader)
        {
            var id = reader.GetInt32Value("id_matricula");
            try
            {
                var laudoBytes = reader.GetNullableBytes("laudo_medico");
                var laudoResult = laudoBytes is null ? null : Arquivo.Criar(LaudoNomePadrao, laudoBytes);
                if (laudoResult is not null && laudoResult.IsFailure)
                    throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", laudoResult.Notificacoes.Select(n => n.Mensagem)));

                var obsRestricao = reader["obs_restricao"] is DBNull ? null : reader.GetStringValue("obs_restricao");

                var result = Matricula.Criar(
                    id,
                    reader.GetInt32Value("aluno_id"),
                    (MatriculaPlano)reader.GetInt32Value("plano"),
                    reader.GetDateOnlyValue("data_inicio"),
                    reader.GetStringValue("objetivo"),
                    (MatriculaRestricoes)reader.GetInt32Value("restricao_medica"),
                    laudoResult?.Value,
                    obsRestricao);

                if (result.IsFailure)
                    throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear matrícula ID {id}: {string.Join(", ", result.Notificacoes.Select(n => n.Mensagem))}");

                return result.Value!;
            }
            catch (InfrastructureException) { throw; }
            catch (Exception ex) when (ex is not InfrastructureException) { throw new InfrastructureException("ERRO_MAPEAMENTO_MATRICULA", $"Erro ao mapear dados da matrícula ID {id}: {ex.Message}", ex); }
        }

        public async Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico) VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObsRestricao, @LaudoMedico)";
            try { await using var command = await CreateCommandAsync(FormatInsertQuery(sql), cancellationToken); AddParameters(command, entity); var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_MATRICULA", "Falha ao obter ID inserido para a matrícula.", cancellationToken); typeof(Entity).GetProperty("Id")?.SetValue(entity, id); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_MATRICULA", $"Erro ao adicionar matrícula para o aluno ID {entity.AlunoId}: {ex.Message}", ex); }
        }

        public async Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE tb_matricula SET aluno_id = @AlunoId, plano = @Plano, data_inicio = @DataInicio, data_fim = @DataFim, objetivo = @Objetivo, restricao_medica = @RestricaoMedica, obs_restricao = @ObsRestricao, laudo_medico = @LaudoMedico WHERE id_matricula = @Id";
            try { await using var command = await CreateCommandAsync(sql, cancellationToken); command.AddParameter("@Id", entity.Id, DbType.Int32); AddParameters(command, entity); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhuma matrícula encontrada com o ID {entity.Id} para atualização."); return entity; }
            catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_MATRICULA", $"Erro ao atualizar matrícula ID {entity.Id} do aluno ID {entity.AlunoId}: {ex.Message}", ex); }
        }

        private static void AddParameters(DbCommand command, Matricula entity)
        {
            command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio, DbType.Date);
            command.AddParameter("@DataFim", entity.DataFim, DbType.Date);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", entity.ObservacoesRestricoes, DbType.String);
            command.AddParameter("@LaudoMedico", entity.LaudoMedico?.Conteudo, DbType.Binary);
        }

        public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) => await ExecuteBoolean("DELETE FROM tb_matricula WHERE id_matricula = @Id", c => c.AddParameter("@Id", id, DbType.Int32), cancellationToken);

        private async Task<bool> ExecuteBoolean(string sql, Action<DbCommand> add, CancellationToken token) { await using var c = await CreateCommandAsync(sql, token); add(c); return await c.ExecuteNonQueryAsync(token) > 0; }
    }
}
