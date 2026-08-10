//Hibrael Andre Cidade Xavier
using System.Text.RegularExpressions;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public sealed partial record Email
    {
        public string EnderecoEmail { get; }

        private Email(string endereco) => EnderecoEmail = endereco;

        public static Result<Email> Criar(string endereco)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(endereco))
            {
                notificacoes.Add(new Notificacoes("Email", "EMAIL_OBRIGATORIO"));
                return Result<Email>.Failure(notificacoes);
            }

            var textoLimpo = NormalizadoService.ParaMinusculo(NormalizadoService.LimparTodosEspacos(endereco));

            if (!FormatoEmailRegex().IsMatch(textoLimpo))
                notificacoes.Add(new Notificacoes("Email", "EMAIL_INVALIDO"));

            if (notificacoes.Count != 0)
                return Result<Email>.Failure(notificacoes);

            return Result<Email>.Success(new Email(textoLimpo));
        }

        public override string ToString() => EnderecoEmail;

        [GeneratedRegex(@"^[^@.]+@[^@.]+.[^@.]+$")]
        private static partial Regex FormatoEmailRegex();
    }
}



