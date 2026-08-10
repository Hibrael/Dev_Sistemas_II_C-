//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que compõe um Logradouro (já validado, com identidade própria) com os
    /// dados que variam por pessoa: número da casa/apartamento e complemento.
    /// </summary>
    public sealed record Endereco
    {
        public Logradouro Logradouro { get; }
        public string NumeroCasa { get; }
        public string? Complemento { get; }

        private Endereco(Logradouro logradouro, string numeroCasa, string? complemento)
        {
            Logradouro = logradouro;
            NumeroCasa = numeroCasa;
            Complemento = complemento;
        }

        public static Result<Endereco> Criar(Logradouro logradouro, string numeroCasa, string? complemento = null)
        {
            var notificacoes = new List<Notificacoes>();

            if (logradouro is null)
                notificacoes.Add(new Notificacoes("Logradouro", "LOGRADOURO_OBRIGATORIO"));

            if (NormalizadoService.TextoVazioOuNulo(numeroCasa))
                notificacoes.Add(new Notificacoes("NumeroCasa", "NUMERO_CASA_OBRIGATORIO"));
            else
                numeroCasa = NormalizadoService.LimparEspacos(numeroCasa);

            if (!NormalizadoService.TextoVazioOuNulo(complemento))
                complemento = NormalizadoService.LimparEspacos(complemento!);

            if (notificacoes.Count != 0)
                return Result<Endereco>.Failure(notificacoes);

            return Result<Endereco>.Success(new Endereco(logradouro!, numeroCasa, complemento));
        }

        public override string ToString() =>
            Complemento is null
                ? $"{Logradouro.Nome}, {NumeroCasa}"
                : $"{Logradouro.Nome}, {NumeroCasa} - {Complemento}";
    }
}



