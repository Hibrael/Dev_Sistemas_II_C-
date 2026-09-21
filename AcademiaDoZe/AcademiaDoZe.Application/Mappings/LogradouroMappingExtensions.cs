//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(logradouro);

        return new LogradouroDto
        {
            Id = logradouro.Id,
            // Cep é um Value Object: o texto cru fica em Numero (apenas dígitos).
            Cep = logradouro.Cep.Numero,
            Nome = logradouro.Nome,
            Bairro = logradouro.Bairro,
            Cidade = logradouro.Cidade,
            Estado = logradouro.Estado,
            Pais = logradouro.Pais
        };
    }

    public static Logradouro ToEntity(this LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);

        return CriarOuFalhar(logradouroDto.Id, logradouroDto);
    }

    /// <summary>
    /// Reconstrói a entidade com os dados do DTO preservando o Id de quem já está persistido.
    /// As entidades do domínio têm setters privados e nenhum mutator de cadastro, então
    /// "atualizar" é recriar pela fábrica Criar — que reaplica todas as validações — usando a
    /// identidade da entidade existente em vez da que veio do DTO.
    /// </summary>
    public static Logradouro UpdateFromDto(this Logradouro logradouro, LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouro);
        ArgumentNullException.ThrowIfNull(logradouroDto);

        return CriarOuFalhar(logradouro.Id, logradouroDto);
    }

    private static Logradouro CriarOuFalhar(int id, LogradouroDto dto)
    {
        var resultado = Logradouro.Criar(id, dto.Cep, dto.Nome, dto.Bairro, dto.Cidade, dto.Estado, dto.Pais);

        if (resultado.IsFailure)
            throw new InvalidOperationException(FormatErrors(resultado.Notificacoes));

        return resultado.Value!;
    }

    private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
        string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
}
