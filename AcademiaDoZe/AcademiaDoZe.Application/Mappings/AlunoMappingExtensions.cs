//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class AlunoMappingExtensions
{
    private const string FotoNomePadrao = "foto.jpg";

    public static AlunoDto ToDto(this Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(aluno);

        return new AlunoDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Cpf = aluno.Cpf.Numero,
            DataNascimento = aluno.DataNascimento,
            Telefone = aluno.Telefone.Numero,
            Email = aluno.Email.EnderecoEmail,
            // Endereco no domínio é um Value Object que compõe o Logradouro com número e
            // complemento; o DTO devolve essas três partes separadas.
            Endereco = aluno.Endereco.Logradouro.ToDto(),
            Numero = aluno.Endereco.NumeroCasa,
            Complemento = aluno.Endereco.Complemento,
            // A senha nunca sai da camada: ver PessoaDto.Senha.
            Senha = null,
            Foto = new ArquivoDto { Conteudo = aluno.Foto.Conteudo, Nome = aluno.Foto.Nome }
        };
    }

    public static Aluno ToEntity(this AlunoDto alunoDto)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        return CriarOuFalhar(alunoDto.Id, alunoDto);
    }

    /// <summary>
    /// Reconstrói a entidade com os dados do DTO preservando o Id de quem já está persistido
    /// — ver a explicação em LogradouroMappingExtensions.UpdateFromDto.
    /// </summary>
    public static Aluno UpdateFromDto(this Aluno aluno, AlunoDto alunoDto)
    {
        ArgumentNullException.ThrowIfNull(aluno);
        ArgumentNullException.ThrowIfNull(alunoDto);

        return CriarOuFalhar(aluno.Id, alunoDto);
    }

    private static Aluno CriarOuFalhar(int id, AlunoDto dto)
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

        // Cpf, Telefone, Email e Senha são entregues como texto: Aluno.Criar constrói cada
        // Value Object e agrega todas as notificações de uma vez, então validar aqui antes
        // apenas duplicaria a regra e reportaria só a primeira falha.
        var resultado = Aluno.Criar(
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
            fotoResult.Value!);

        if (resultado.IsFailure)
            throw new InvalidOperationException(FormatErrors(resultado.Notificacoes));

        return resultado.Value!;
    }

    private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
        string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
}
