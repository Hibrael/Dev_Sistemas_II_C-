//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    internal class Colaborador : Pessoa
    {
        public ColaboradorTipos Tipo { get; set; }
        public ColaboradorVinculo Vinculo { get; set; }
        public DateTime DataAdmissao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public decimal Salario { get; set; }
    }
}
