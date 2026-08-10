//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{

    public sealed record Cpf
    {
        public string Numero { get; }

        private Cpf(string numero) => Numero = numero;

        public static Result<Cpf> Criar(string numero)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(numero))
            {
                notificacoes.Add(new Notificacoes("Cpf", "CPF_OBRIGATORIO"));
                return Result<Cpf>.Failure(notificacoes);
            }

            var digitos = NormalizadoService.ApenasDigitos(numero);

            if (!ValidarCpf(digitos))
                notificacoes.Add(new Notificacoes("Cpf", "CPF_INVALIDO"));

            if (notificacoes.Count != 0)
                return Result<Cpf>.Failure(notificacoes);

            return Result<Cpf>.Success(new Cpf(digitos));
        }

        private static bool ValidarCpf(string cpf)
        {
            if (cpf.Length != 11)
                return false;


            if (new string(cpf[0], 11) == cpf)
                return false;

            var primeiroDigito = CalcularDigitoVerificador(cpf, 9);
            if (primeiroDigito != cpf[9] - '0')
                return false;

            var segundoDigito = CalcularDigitoVerificador(cpf, 10);
            return segundoDigito == cpf[10] - '0';
        }

        private static int CalcularDigitoVerificador(string cpf, int tamanho)
        {
            var soma = 0;
            var peso = tamanho + 1;

            for (var i = 0; i < tamanho; i++)
            {
                soma += (cpf[i] - '0') * peso;
                peso--;
            }

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        public override string ToString() =>
            $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";
    }
}


