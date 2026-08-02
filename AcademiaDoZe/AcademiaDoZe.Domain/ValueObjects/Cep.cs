//Hibrael Andre Cidade Xavier
using System;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Cep
    {
        public string Numero { get; private set; }

        public Cep(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("O CEP não pode ser vazio.");

            string textoLimpo = numero.Replace("-", "").Replace(".", "").Trim();

            if (!Regex.IsMatch(textoLimpo, @"^\d{8}$"))
                throw new ArgumentException("CEP inválido: deve conter 8 dígitos numéricos.");

            Numero = textoLimpo;
        }

        public override string ToString() => $"{Numero.Substring(0, 5)}-{Numero.Substring(5)}";
    }
}
