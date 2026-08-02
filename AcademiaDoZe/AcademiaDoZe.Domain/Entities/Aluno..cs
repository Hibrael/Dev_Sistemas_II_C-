using AcademiaDoZe.Domain.ValueObjects;
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class Aluno : Pessoa
    {
        public string Matricula { get; private set; }

        private Aluno(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco, string matricula)
            : base(nome, cpf, telefone, email, dataNascimento, endereco)
        {
            Matricula = matricula;
        }

        public static Aluno MatricularAluno(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco, string matricula)
        {
            ValidarDadosPessoais(nome, cpf, telefone, email, dataNascimento, endereco);

            if (string.IsNullOrWhiteSpace(matricula))
                throw new ArgumentException("Matrícula é obrigatória.");

            return new Aluno(nome, cpf, telefone, email, dataNascimento, endereco, matricula);
        }
    }
}
