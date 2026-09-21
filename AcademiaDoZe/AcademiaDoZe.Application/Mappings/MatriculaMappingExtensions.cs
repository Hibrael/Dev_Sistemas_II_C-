//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    // O Value Object Arquivo aceita apenas .jpg, .jpeg e .png, então o laudo trafega como
    // imagem; o nome só é usado quando o DTO não informa o original.
    private const string LaudoNomePadrao = "laudo.jpg";

    public static MatriculaDto ToDto(this Matricula matricula)
    {
        ArgumentNullException.ThrowIfNull(matricula);

        return new MatriculaDto
        {
            Id = matricula.Id,
            AlunoId = matricula.AlunoId,
            // AlunoMatricula fica nulo aqui: a entidade referencia o aluno por identidade.
            // Quem preenche é MatriculaService, que tem acesso ao repositório de alunos.
            AlunoMatricula = null,
            Plano = matricula.Plano.ToApp(),
            DataInicio = matricula.DataInicio,
            DataFim = matricula.DataFim,
            Objetivo = matricula.Objetivo,
            RestricoesMedicas = matricula.RestricoesMedicas.ToApp(),
            ObservacoesRestricoes = matricula.ObservacoesRestricoes,
            LaudoMedico = matricula.LaudoMedico is null
                ? null
                : new ArquivoDto { Conteudo = matricula.LaudoMedico.Conteudo, Nome = matricula.LaudoMedico.Nome }
        };
    }

    public static Matricula ToEntity(this MatriculaDto matriculaDto)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);

        return CriarOuFalhar(matriculaDto.Id, matriculaDto);
    }

    /// <summary>
    /// Reconstrói a entidade com os dados do DTO preservando o Id de quem já está persistido
    /// — ver a explicação em LogradouroMappingExtensions.UpdateFromDto.
    /// </summary>
    public static Matricula UpdateFromDto(this Matricula matricula, MatriculaDto matriculaDto)
    {
        ArgumentNullException.ThrowIfNull(matricula);
        ArgumentNullException.ThrowIfNull(matriculaDto);

        return CriarOuFalhar(matricula.Id, matriculaDto);
    }

    private static Matricula CriarOuFalhar(int id, MatriculaDto dto)
    {
        Arquivo? laudo = null;

        if (dto.LaudoMedico is not null)
        {
            var laudoResult = Arquivo.Criar(dto.LaudoMedico.Nome ?? LaudoNomePadrao, dto.LaudoMedico.Conteudo);
            if (laudoResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(laudoResult.Notificacoes));

            laudo = laudoResult.Value!;
        }

        // DataFim não é informada: o domínio a calcula a partir de DataInicio e do Plano.
        var resultado = Matricula.Criar(
            id,
            dto.AlunoId,
            dto.Plano.ToDomain(),
            dto.DataInicio,
            dto.Objetivo,
            dto.RestricoesMedicas.ToDomain(),
            laudo,
            dto.ObservacoesRestricoes);

        if (resultado.IsFailure)
            throw new InvalidOperationException(FormatErrors(resultado.Notificacoes));

        return resultado.Value!;
    }

    private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
        string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
}
