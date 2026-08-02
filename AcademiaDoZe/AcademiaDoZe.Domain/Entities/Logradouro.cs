//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.ValueObjects;
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class Logradouro : Entity
    {
        public string NomeLogradouro { get; private set; }
        public Cep Cep { get; private set; }
        public string Pais { get; private set; }
        public string Cidade { get; private set; }
        public string Bairro { get; private set; }
        public string Rua { get; private set; }

        private Logradouro(string nomeLogradouro, Cep cep, string pais, string cidade, string bairro, string rua)
        {
            NomeLogradouro = nomeLogradouro;
            Cep = cep;
            Pais = pais;
            Cidade = cidade;
            Bairro = bairro;
            Rua = rua;
        }

        public static Logradouro Criar(string nomeLogradouro, Cep cep, string pais, string cidade, string bairro, string rua)
        {
            if (string.IsNullOrWhiteSpace(nomeLogradouro))
                throw new ArgumentException("Nome do logradouro é obrigatório.");

            if (cep is null)
                throw new ArgumentException("CEP é obrigatório.");

            if (string.IsNullOrWhiteSpace(pais))
                throw new ArgumentException("País é obrigatório.");

            if (string.IsNullOrWhiteSpace(cidade))
                throw new ArgumentException("Cidade é obrigatória.");

            if (string.IsNullOrWhiteSpace(bairro))
                throw new ArgumentException("Bairro é obrigatório.");

            if (string.IsNullOrWhiteSpace(rua))
                throw new ArgumentException("Rua é obrigatória.");

            return new Logradouro(nomeLogradouro, cep, pais, cidade, bairro, rua);
        }
    }
}
