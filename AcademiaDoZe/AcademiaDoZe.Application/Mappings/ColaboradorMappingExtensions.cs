//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    private const string FotoNomePadrao = "foto.jpg";

    public static ColaboradorDto ToDto(this Colaborador colaborador)
    {
        ArgumentNullException.ThrowIfNull(colaborador);

        return new ColaboradorDto
        {
            Id = colaborador.Id,
            Nome = colaborador.Nome,
            Cpf = colaborador.Cpf.Numero,
            DataNascimento = colaborador.DataNascimento,
            Telefone = colaborador.Telefone.Numero,
            Email = colaborador.Email.EnderecoEmail,
            Endereco = colaborador.Endereco.Logradouro.ToDto(),
            Numero = colaborador.Endereco.NumeroCasa,
            Complemento = colaborador.Endereco.Complemento,
            // A senha nunca sai da camada: ver PessoaDto.Senha.
            Senha = null,
            Foto = new ArquivoDto { Conteudo = colaborador.Foto.Conteudo, Nome = colaborador.Foto.Nome },
            DataAdmissao = colaborador.DataAdmissao,
            Tipo = colaborador.Tipo.ToApp(),
            Vinculo = colaborador.Vinculo.ToApp(),
            Salario = colaborador.Salario,
            DataDemissao = colaborador.DataDemissao
        };
    }

    public static Colaborador ToEntity(this ColaboradorDto colaboradorDto)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        return CriarOuFalhar(colaboradorDto.Id, colaboradorDto);
    }

    /// <summary>
    /// Reconstrói a entidade com os dados do DTO preservando o Id de quem já está persistido
    /// — ver a explicação em LogradouroMappingExtensions.UpdateFromDto.
    /// </summary>
    public static Colaborador UpdateFromDto(this Colaborador colaborador, ColaboradorDto colaboradorDto)
    {
        ArgumentNullException.ThrowIfNull(colaborador);
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        return CriarOuFalhar(colaborador.Id, colaboradorDto);
    }

    private static Colaborador CriarOuFalhar(int id, ColaboradorDto dto)
    {
        if (dto.Endereco is null)
            throw new InvalidOperationException("Endereco: LOGRADOURO_OBRIGATORIO");

        if (dto.Foto is null)
            throw new InvalidOperationException("Foto: FOTO_OBRIGATORIA");

        var senha = dto.Senha;
        if (string.IsNullOrWhiteSpace(senha))
            throw new InvalidOperationException("Senha: SENHA_OBRIGATORIA");

        var logradouro = dto.Endereco.ToEntity();

        var fotoResult = Arquivo.Criar(dto.Foto.Nome ?? FotoNomePadrao, dto.Foto.Conteudo);
        if (fotoResult.IsFailure)
            throw new InvalidOperationException(FormatErrors(fotoResult.Notificacoes));

        // DataDemissao não entra aqui: o desligamento é a operação de domínio
        // Colaborador.Desligar, com regras próprias, e não faz parte do cadastro.
        var resultado = Colaborador.Criar(
            id,
            dto.Nome,
            dto.Cpf,
            dto.DataNascimento,
            dto.Telefone,
            dto.Email ?? string.Empty,
            logradouro,
            dto.Numero,
            dto.Complemento,
            senha,
            fotoResult.Value!,
            dto.DataAdmissao,
            dto.Tipo.ToDomain(),
            dto.Vinculo.ToDomain(),
            dto.Salario);

        if (resultado.IsFailure)
            throw new InvalidOperationException(FormatErrors(resultado.Notificacoes));

        return resultado.Value!;
    }

    private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
        string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
}
