//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

/// <summary>
/// Pessoa encontrada na busca da catraca (aluno ou colaborador), com os botões de entrada e
/// saída. NaAcademia indica se ela tem uma entrada sem saída — só para o selo do cartão; quem
/// decide se a entrada/saída é permitida é a camada de aplicação.
/// </summary>
public sealed class PessoaCatracaItem
{
    public required int Id { get; init; }
    public required bool IsAluno { get; init; }
    public required string Nome { get; init; }
    public required string Cpf { get; init; }
    public ArquivoDto? Foto { get; init; }

    /// <summary>Ex.: "Aluno" ou "Instrutor · Estagiário · jornada de 6 h".</summary>
    public required string Detalhe { get; init; }

    public bool NaAcademia { get; init; }
}

/// <summary>Pessoa que entrou e ainda não saiu, na lista "Na academia agora".</summary>
public sealed class PresencaItem
{
    public required int PessoaId { get; init; }
    public required bool IsAluno { get; init; }
    public required string Nome { get; init; }
    public byte[]? Foto { get; init; }

    /// <summary>Horário local da entrada.</summary>
    public required DateTime Entrada { get; init; }

    public string TipoTexto => IsAluno ? "Aluno" : "Colaborador";

    public string EntradaTexto => Entrada.Date == DateTime.Today
        ? $"Entrou às {Entrada:HH:mm}"
        : $"Entrou em {Entrada:dd/MM} às {Entrada:HH:mm}";

    public string TempoTexto => $"há {RegistroAcessoResultadoDto.FormatarDuracao(DateTime.Now - Entrada)}";
}

/// <summary>Linha de detalhe do resultado da catraca: ícone (Material Icons) + texto.</summary>
public sealed record DetalheResultado(string Icone, string Texto);
