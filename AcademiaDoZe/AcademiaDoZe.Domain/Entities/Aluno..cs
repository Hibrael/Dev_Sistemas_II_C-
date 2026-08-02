//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    internal class Aluno: Pessoa
    {
        public string Matricula { get; private set; }

        public Aluno(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco, string matricula)
            : base(nome, cpf, telefone, email, dataNascimento, endereco)
        {
            Matricula = matricula;
        }
    }
}
