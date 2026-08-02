using AcademiaDoZe.Domain.ValueObjects;
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class Pessoa : Entity
    {
        public string Nome { get; private set; }
        public Cpf Cpf { get; private set; }
        public string Telefone { get; private set; }
        public string Email { get; private set; }
        public DateTime DataNascimento { get; private set; }
        public Endereco Endereco { get; private set; }

        protected Pessoa(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco)
        {
            Nome = nome;
            Cpf = cpf;
            Telefone = telefone;
            Email = email;
            DataNascimento = dataNascimento;
            Endereco = endereco;
        }

        public static Pessoa Criar(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco)
        {
            ValidarDadosPessoais(nome, cpf, telefone, email, dataNascimento, endereco);
            return new Pessoa(nome, cpf, telefone, email, dataNascimento, endereco);
        }

        protected static void ValidarDadosPessoais(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco)
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
                throw new ArgumentException("Nome inválido: deve conter ao menos 3 caracteres.");

            if (cpf is null)
                throw new ArgumentException("CPF é obrigatório.");

            if (string.IsNullOrWhiteSpace(telefone))
                throw new ArgumentException("Telefone é obrigatório.");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new ArgumentException("E-mail inválido.");

            if (dataNascimento.Date > DateTime.Today)
                throw new ArgumentException("Data de nascimento não pode ser no futuro.");

            if (CalcularIdade(dataNascimento) < 14)
                throw new ArgumentException("Pessoa deve ter ao menos 14 anos.");

            if (endereco is null)
                throw new ArgumentException("Endereço é obrigatório.");
        }

        private static int CalcularIdade(DateTime dataNascimento)
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - dataNascimento.Year;
            if (dataNascimento.Date > hoje.AddYears(-idade))
                idade--;
            return idade;
        }
    }
}
