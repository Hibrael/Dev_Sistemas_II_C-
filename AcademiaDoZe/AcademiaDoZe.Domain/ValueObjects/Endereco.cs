//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Endereco
    {
        public Logradouro logradouro { get; set; }
        public string NumeroCasa { get; set; }
        public string? Complemento { get; set; }
    }

        private Endereco(Logradouro logradouro, string numeroCasa, string? complemento)
        {
            Logradouro = logradouro;
            NumeroCasa = numeroCasa;
            Complemento = complemento;
        }

        public static Endereco Adicioanr(Logradouro logradouro, string numeroCasa, string? complemento = null)
        {
            if (logradouro is null)
                throw new ArgumentException("Logradouro é obrigatório.");

            if (string.IsNullOrWhiteSpace(numeroCasa))
                throw new ArgumentException("Número da casa é obrigatório.");

            return new Endereco(logradouro, numeroCasa, complemento);
        }
    }
