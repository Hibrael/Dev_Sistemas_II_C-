//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Application.DTOs;

/// <summary>
/// Base comum de AlunoDto e ColaboradorDto, espelhando a entidade abstrata Pessoa do domínio.
/// O endereço é exposto como LogradouroDto + Numero + Complemento porque é exatamente essa a
/// assinatura que Aluno.Criar/Colaborador.Criar esperam.
/// </summary>
public abstract class PessoaDto
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cpf { get; set; }
    public required DateOnly DataNascimento { get; set; }
    public required string Telefone { get; set; }
    public string? Email { get; set; }
    public LogradouroDto? Endereco { get; set; }
    public required string Numero { get; set; }
    public string? Complemento { get; set; }

    /// <summary>
    /// Senha em texto plano. É entrada: o hash é gerado pelo Value Object Senha do domínio,
    /// dentro de Aluno.Criar/Colaborador.Criar. Nunca é devolvida preenchida pelos ToDto.
    /// </summary>
    public string? Senha { get; set; }

    public ArquivoDto? Foto { get; set; }
}
