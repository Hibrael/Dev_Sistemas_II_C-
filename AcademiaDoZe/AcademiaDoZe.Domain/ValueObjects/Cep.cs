//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um CEP brasileiro: sem identidade própria,
    /// imutável e igual a outro Cep se os valores forem iguais (garantido pelo uso de record).
    /// </summary>
    public sealed record Cep
    {
        public string Numero { get; }

        private Cep(string numero) => Numero = numero;

        public static Result<Cep> Criar(string numero)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(numero))
            {
                notificacoes.Add(new Notificacoes("Cep", "CEP_OBRIGATORIO"));
                return Result<Cep>.Failure(notificacoes);
            }

            var digitos = NormalizadoService.ApenasDigitos(numero);

            if (digitos.Length != 8)
                notificacoes.Add(new Notificacoes("Cep", "CEP_INVALIDO"));

            if (notificacoes.Count != 0)
                return Result<Cep>.Failure(notificacoes);

            return Result<Cep>.Success(new Cep(digitos));
        }

        public override string ToString() => $"{Numero[..5]}-{Numero[5..]}";
    }
}



