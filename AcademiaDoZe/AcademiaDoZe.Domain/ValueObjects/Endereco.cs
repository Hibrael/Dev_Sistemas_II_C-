//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Endereco: Logradouro
    {
        public string NumeroCasa { get; set; }
        public string Complemento { get; set; }
    }
}
