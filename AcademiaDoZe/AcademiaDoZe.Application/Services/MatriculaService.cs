//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

/// <summary>
/// Orquestra os casos de uso de Matricula.
///
/// A entidade Matricula referencia o aluno por AlunoId, porque Aluno e Matricula são Aggregate
/// Roots distintos. Para a apresentação não precisar de uma segunda consulta, este serviço
/// recebe também a fábrica do repositório de alunos e preenche MatriculaDto.AlunoMatricula na
/// leitura. Sem essa fábrica o serviço continua funcionando: o DTO apenas sai com AlunoId.
/// </summary>
public class MatriculaService : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _repoFactory;
    private readonly Func<IAlunoRepository>? _alunoRepoFactory;

    public MatriculaService(Func<IMatriculaRepository> repoFactory, Func<IAlunoRepository>? alunoRepoFactory = null)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        _alunoRepoFactory = alunoRepoFactory;
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var matricula = await _repoFactory().ObterPorId(id, cancellationToken);
        if (matricula is null) return null;

        return await EnriquecerAsync(matricula, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default) =>
        await EnriquecerAsync(await _repoFactory().ObterTodos(cancellationToken), cancellationToken);

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        ValidarAlunoId(alunoId);
        return await EnriquecerAsync(await _repoFactory().ObterPorAluno(alunoId, cancellationToken), cancellationToken);
    }

    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        ValidarAlunoId(alunoId);

        var matricula = await _repoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        if (matricula is null) return null;

        return await EnriquecerAsync(matricula, cancellationToken);
    }

    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        ValidarAlunoId(alunoId);
        return await _repoFactory().PossuiMatriculaAtiva(alunoId, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        // alunoId 0 é o padrão do repositório para "todas as matrículas ativas".
        if (alunoId < 0)
            throw new ArgumentException("O identificador do aluno não pode ser negativo.", nameof(alunoId));

        return await EnriquecerAsync(await _repoFactory().ObterAtivas(alunoId, cancellationToken), cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        if (dias < 0)
            throw new ArgumentException("A quantidade de dias não pode ser negativa.", nameof(dias));

        return await EnriquecerAsync(await _repoFactory().ObterVencendoEmDias(dias, cancellationToken), cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default) =>
        await EnriquecerAsync(await _repoFactory().ObterPorPlano(plano.ToDomain(), cancellationToken), cancellationToken);

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);
        ValidarAlunoId(matriculaDto.AlunoId);

        if (await _repoFactory().PossuiMatriculaAtiva(matriculaDto.AlunoId, cancellationToken))
            throw new InvalidOperationException($"O aluno {matriculaDto.AlunoId} já possui uma matrícula ativa.");

        var matricula = matriculaDto.ToEntity();
        var adicionada = await _repoFactory().Adicionar(matricula, cancellationToken);
        return await EnriquecerAsync(adicionada, cancellationToken);
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);
        ValidarAlunoId(matriculaDto.AlunoId);

        var matriculaExistente = await _repoFactory().ObterPorId(matriculaDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Matrícula com ID {matriculaDto.Id} não encontrada.");

        var matriculaAtualizada = matriculaExistente.UpdateFromDto(matriculaDto);
        var atualizada = await _repoFactory().Atualizar(matriculaAtualizada, cancellationToken);
        return await EnriquecerAsync(atualizada, cancellationToken);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var matricula = await _repoFactory().ObterPorId(id, cancellationToken);
        if (matricula is null) return false;

        return await _repoFactory().Remover(id, cancellationToken);
    }

    private async Task<MatriculaDto> EnriquecerAsync(Matricula matricula, CancellationToken cancellationToken)
    {
        var dto = matricula.ToDto();

        if (_alunoRepoFactory is not null)
        {
            var aluno = await _alunoRepoFactory().ObterPorId(matricula.AlunoId, cancellationToken);
            dto.AlunoMatricula = aluno?.ToDto();
        }

        return dto;
    }

    private async Task<IEnumerable<MatriculaDto>> EnriquecerAsync(IEnumerable<Matricula> matriculas, CancellationToken cancellationToken)
    {
        var lista = matriculas.ToList();
        if (lista.Count == 0) return [];

        var dtos = lista.Select(m => m.ToDto()).ToList();

        if (_alunoRepoFactory is not null)
        {
            // Carrega cada aluno uma única vez, mesmo que várias matrículas sejam do mesmo.
            var alunos = new Dictionary<int, AlunoDto>();

            foreach (var alunoId in lista.Select(m => m.AlunoId).Distinct())
            {
                var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken);
                if (aluno is not null) alunos[alunoId] = aluno.ToDto();
            }

            foreach (var dto in dtos)
                dto.AlunoMatricula = alunos.GetValueOrDefault(dto.AlunoId);
        }

        return dtos;
    }

    private static void ValidarAlunoId(int alunoId)
    {
        if (alunoId <= 0)
            throw new ArgumentException("O identificador do aluno deve ser maior que zero.", nameof(alunoId));
    }
}
