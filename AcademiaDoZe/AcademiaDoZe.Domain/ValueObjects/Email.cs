//Hibrael Andre Cidade Xavier
using System;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Email
    {
        public string enderecoEmail { get; private set; }

        public Email(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("O e-mail não pode ser vazio.");

            string textoLimpo = valor.Trim();

            if (!Regex.IsMatch(textoLimpo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("E-mail inválido.");

            enderecoEmail = textoLimpo.ToLowerInvariant();
        }

        public override string ToString() => enderecoEmail;
    }
}
