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
        public required string Senha { get; init; }
        public required byte[] Foto { get; init; }
        public required DateOnly DataAdmissao { get; init; }
        public required string Tipo { get; init; }
        public required string Vinculo { get; init; }
        public required decimal Salario { get; init; }
        public required DateTime DataHora { get; init; }
    }
}
