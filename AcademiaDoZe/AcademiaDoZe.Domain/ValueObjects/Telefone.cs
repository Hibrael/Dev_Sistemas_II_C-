//Hibrael Andre Cidade Xavier
using System;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Telefone
    {
        public string Numero { get; private set; }

        public Telefone(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("O telefone não pode ser vazio.");

            string textoLimpo = Regex.Replace(numero, @"[^\d]", "");

            if (textoLimpo.Length != 10 && textoLimpo.Length != 11)
                throw new ArgumentException("Telefone inválido: deve conter 10 ou 11 dígitos, incluindo o DDD.");

            Numero = textoLimpo;
        }

        public override string ToString()
        {
            return Numero.Length == 11
                ? $"({Numero.Substring(0, 2)}) {Numero.Substring(2, 5)}-{Numero.Substring(7)}"
                : $"({Numero.Substring(0, 2)}) {Numero.Substring(2, 4)}-{Numero.Substring(6)}";
        }
    }
}
