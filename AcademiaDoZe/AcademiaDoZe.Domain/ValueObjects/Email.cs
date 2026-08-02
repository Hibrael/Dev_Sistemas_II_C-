//Hibrael Andre Cidade Xavier
using System;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Email
    {
        public string enderecoEmail { get; private set; }

        public Email(string enderecoEmail)
        {
            if (string.IsNullOrWhiteSpace(enderecoEmail))
                throw new ArgumentException("O e-mail não pode ser vazio.");

            string textoLimpo = enderecoEmail.Trim();

            if (!Regex.IsMatch(textoLimpo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("E-mail inválido.");

            enderecoEmail = textoLimpo.ToLowerInvariant();
        }

        public override string ToString() => enderecoEmail;
    }
}
