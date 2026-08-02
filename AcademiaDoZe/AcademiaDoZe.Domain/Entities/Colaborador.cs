//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class Colaborador : Pessoa
    {
        public ColaboradorTipos Tipo { get; private set; }
        public ColaboradorVinculo Vinculo { get; private set; }
        public DateTime DataAdmissao { get; private set; }
        public DateTime? DataDemissao { get; private set; }
        public decimal Salario { get; private set; }

        private Colaborador(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco,
            ColaboradorTipos tipo, ColaboradorVinculo vinculo, DateTime dataAdmissao, decimal salario)
            : base(nome, cpf, telefone, email, dataNascimento, endereco)
        {
            Tipo = tipo;
            Vinculo = vinculo;
            DataAdmissao = dataAdmissao;
            Salario = salario;
        }

        public static Colaborador Registrar(string nome, Cpf cpf, string telefone, string email, DateTime dataNascimento, Endereco endereco,
            ColaboradorTipos tipo, ColaboradorVinculo vinculo, DateTime dataAdmissao, decimal salario)
        {
            ValidarDadosPessoais(nome, cpf, telefone, email, dataNascimento, endereco);

            if (!Enum.IsDefined(typeof(ColaboradorTipos), tipo))
                throw new ArgumentException("Tipo de colaborador inválido.");

            if (!Enum.IsDefined(typeof(ColaboradorVinculo), vinculo))
                throw new ArgumentException("Vínculo de colaborador inválido.");

            if (dataAdmissao.Date > DateTime.Today)
                throw new ArgumentException("Data de admissão não pode ser no futuro.");

            if (salario <= 0)
                throw new ArgumentException("Salário deve ser maior que zero.");

            return new Colaborador(nome, cpf, telefone, email, dataNascimento, endereco, tipo, vinculo, dataAdmissao, salario);
        }

        public void Desligar(DateTime dataDemissao)
        {
            if (DataDemissao is not null)
                throw new InvalidOperationException("Colaborador já foi demitido.");

            if (dataDemissao.Date < DataAdmissao.Date)
                throw new ArgumentException("Data de demissão não pode ser anterior à data de admissão.");

            if (dataDemissao.Date > DateTime.Today)
                throw new ArgumentException("Data de demissão não pode ser no futuro.");

            DataDemissao = dataDemissao;
        }
    }
}
