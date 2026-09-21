//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Application.DTOs
{
    public sealed class AcessoAlunoDto
    {
        public required int Id { get; init; }
        public required int AlunoId { get; init; }
        public required string NomeAluno { get; init; }
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

        /// <summary>
        /// Instante do acesso. Não é required porque tem dois papéis: deixado em branco,
        /// marca um check-in novo e o domínio carimba a hora atual; preenchido, o instante
        /// informado é preservado via AcessoAluno.Restaurar — o que mantém o horário real
        /// do acesso numa atualização.
        /// </summary>
        public DateTime DataHora { get; init; }
    }
}
