//Hibrael Andre Cidade Xavier
using System.Security.Cryptography;
using System.Text;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public sealed record Senha
    {
        public string Hash { get; }
        public string Salt { get; }

        private Senha(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        /// <summary>Cria uma nova senha a partir de texto puro digitado pelo usuário, validando a força mínima.</summary>
        public static Result<Senha> Criar(string senhaTextoPlano)
        {
            var notificacoes = new List<Notification>();

            if (NormalizadoService.TextoVazioOuNulo(senhaTextoPlano) || senhaTextoPlano.Length < 8)
                notificacoes.Add(new Notification("Senha", "SENHA_TAMANHO_MINIMO"));
            else if (!ContemLetraENumero(senhaTextoPlano))
                notificacoes.Add(new Notification("Senha", "SENHA_REQUISITOS_INVALIDOS"));

            if (notificacoes.Count != 0)
                return Result<Senha>.Failure(notificacoes);

            var salt = GerarSalt();
            var hash = GerarHash(senhaTextoPlano, salt);
            return Result<Senha>.Success(new Senha(hash, salt));
        }

        public static Senha Restaurar(string hash, string salt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(hash);
            ArgumentException.ThrowIfNullOrWhiteSpace(salt);

            return new Senha(hash, salt);
        }

        public bool Verificar(string senhaTextoPlano)
        {
            if (NormalizadoService.TextoVazioOuNulo(senhaTextoPlano))
                return false;

            return GerarHash(senhaTextoPlano, Salt) == Hash;
        }

        private static bool ContemLetraENumero(string senha)
        {
            var temLetra = false;
            var temNumero = false;

            foreach (var caractere in senha)
            {
                if (char.IsLetter(caractere)) temLetra = true;
                if (char.IsDigit(caractere)) temNumero = true;
            }

            return temLetra && temNumero;
        }

        private static string GerarSalt()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(bytes);
        }

        private static string GerarHash(string senhaTextoPlano, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(senhaTextoPlano + salt);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}



