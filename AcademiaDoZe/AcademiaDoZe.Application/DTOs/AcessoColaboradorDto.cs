//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Application.DTOs
{
    public sealed class AcessoColaboradorDto
    {
        public required int Id { get; init; }
        public required int ColaboradorId { get; init; }
        public required string NomeColaborador { get; init; }
        public required string Cpf { get; init; }
        public required DateOnly DataNascimento { get; init; }
        public required string Telefone { get; init; }
        public required string Email { get; init; }
        public required int LogradouroId { get; init; }
        public required string Cep { get; init; }
        public required string NomeLogradouro { get; init; }
        public required string Bairro { get; init; }
        public required string Cidade { get; init; }
        public required string Estado { get; init; }
        public required string Pais { get; init; }
        public required string NumeroCasa { get; init; }
        public string? Complemento { get; init; }

        /// <summary>
        /// Senha em texto plano, usada apenas na ENTRADA (ToEntity), onde é obrigatória.
        /// Nunca é preenchida na SAÍDA: ToDto devolve sempre null, para que a credencial
        /// não escape da camada de aplicação junto com os dados cadastrais.
        /// </summary>
        public string? Senha { get; init; }

        public required byte[] Foto { get; init; }

        /// <summary>
        /// Nome original do arquivo da foto, preservado para não perder a extensão no
        /// caminho de ida e volta. Quando ausente, ToEntity assume "foto.jpg".
        /// </summary>
        public string? FotoNomeArquivo { get; init; }

        public required DateOnly DataAdmissao { get; init; }
        public required string Tipo { get; init; }
        public required string Vinculo { get; init; }
        public required decimal Salario { get; init; }

        /// <summary>
        /// Preenchido na SAÍDA (ToDto). Ignorado na ENTRADA: AcessoColaborador.Criar sempre
        /// carimba DateTime.UtcNow, e a entidade não expõe um Restaurar que permitisse
        /// reidratar o instante persistido — por isso este campo não é required.
        /// </summary>
        public DateTime DataHora { get; init; }
    }
}
