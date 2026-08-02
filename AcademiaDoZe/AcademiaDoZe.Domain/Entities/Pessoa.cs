//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public class Pessoa: Entity
    {
        public string Nome { get; private set; }
        public Cpf Cpf { get; private set; }
        public string Telefone { get; private set; }
        public string Email { get; private set; }
        public DateTime DataNascimento { get; private set; }
        public Endereco Endereco { get; private set; }

        public Pessoa(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco)
        {
            Nome = nome;
            Cpf = cpf;
            Telefone = telefone;
            Email = email;
            DataNascimento = dataNascimento;
            Endereco = endereco;
        }
    }
}


