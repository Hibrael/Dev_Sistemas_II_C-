//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public sealed record Telefone
    {
        public string Numero { get; }

        private Telefone(string numero) => Numero = numero;

        public static Result<Telefone> Criar(string numero)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(numero))
            {
                notificacoes.Add(new Notificacoes("Telefone", "TELEFONE_OBRIGATORIO"));
                return Result<Telefone>.Failure(notificacoes);
            }

            var digitos = NormalizadoService.ApenasDigitos(numero);

            if (digitos.Length != 10 && digitos.Length != 11)
                notificacoes.Add(new Notificacoes("Telefone", "TELEFONE_INVALIDO"));

            if (notificacoes.Count != 0)
                return Result<Telefone>.Failure(notificacoes);

            return Result<Telefone>.Success(new Telefone(digitos));
        }

        public override string ToString() => Numero.Length == 11
            ? $"({Numero[..2]}) {Numero[2..7]}-{Numero[7..]}"
            : $"({Numero[..2]}) {Numero[2..6]}-{Numero[6..]}";
    }
}



