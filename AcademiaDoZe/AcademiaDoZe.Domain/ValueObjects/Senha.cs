//Hibrael Andre Cidade Xavier
using System;
using System.Security.Cryptography;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Senha
    {
        public string Hash { get; private set; }
        public string Salt { get; private set; }

        private Senha(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public static Senha Criar(string senhaTextoPlano)
        {
            if (string.IsNullOrWhiteSpace(senhaTextoPlano) || senhaTextoPlano.Length < 8)
                throw new ArgumentException("A senha deve ter ao menos 8 caracteres.");

            if (!ContemLetraENumero(senhaTextoPlano))
                throw new ArgumentException("A senha deve conter ao menos uma letra e um número.");

            var salt = GerarSalt();
            var hash = GerarHash(senhaTextoPlano, salt);
            return new Senha(hash, salt);
        }

        public static Senha Restaurar(string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Hash e salt são obrigatórios para restaurar a senha.");

            return new Senha(hash, salt);
        }

        public bool Verificar(string senhaTextoPlano)
        {
            if (string.IsNullOrWhiteSpace(senhaTextoPlano))
                return false;

            return GerarHash(senhaTextoPlano, Salt) == Hash;
        }

        private static bool ContemLetraENumero(string senha)
        {
            bool temLetra = false;
            bool temNumero = false;

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
