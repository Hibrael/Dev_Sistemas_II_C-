//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Application.DTOs;

public class ArquivoDto
{
    public required byte[] Conteudo { get; set; }

    /// <summary>
    /// Nome do arquivo com extensão. O Value Object Arquivo do domínio valida a extensão
    /// contra uma lista permitida (.jpg, .jpeg, .png), então o nome precisa trafegar junto
    /// com o conteúdo. Quando ausente, os mappings assumem "foto.jpg".
    /// </summary>
    public string? Nome { get; set; }
}
