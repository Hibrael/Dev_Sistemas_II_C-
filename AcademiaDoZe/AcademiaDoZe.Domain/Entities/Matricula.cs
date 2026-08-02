//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Enums;
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class Matricula : Entity
    {
        public int AlunoId { get; private set; }
        public MartriculaPlano Plano { get; private set; }
        public MatriculaRestricoes Restricoes { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativa { get; private set; }

        private Matricula(int alunoId, MartriculaPlano plano, MatriculaRestricoes restricoes, DateTime dataInicio, DateTime dataFim, decimal valor)
        {
            AlunoId = alunoId;
            Plano = plano;
            Restricoes = restricoes;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Valor = valor;
            Ativa = true;
        }

        public static Matricula Criar(int alunoId, MartriculaPlano plano, MatriculaRestricoes restricoes, DateTime dataInicio, decimal valor)
        {
            if (alunoId <= 0)
                throw new ArgumentException("Aluno inválido.");

            if (!Enum.IsDefined(typeof(MartriculaPlano), plano))
                throw new ArgumentException("Plano de matrícula inválido.");

            if (valor <= 0)
                throw new ArgumentException("Valor da matrícula deve ser maior que zero.");

            var dataFim = CalcularDataFim(dataInicio, plano);
            return new Matricula(alunoId, plano, restricoes, dataInicio, dataFim, valor);
        }

        public void Cancelar()
        {
            if (!Ativa)
                throw new InvalidOperationException("Matrícula já está cancelada.");

            Ativa = false;
        }

        private static DateTime CalcularDataFim(DateTime dataInicio, MartriculaPlano plano)
        {
            return plano switch
            {
                MartriculaPlano.mensal => dataInicio.AddMonths(1),
                MartriculaPlano.trimestral => dataInicio.AddMonths(3),
                MartriculaPlano.semestral => dataInicio.AddMonths(6),
                MartriculaPlano.anual => dataInicio.AddYears(1),
                _ => throw new ArgumentException("Plano de matrícula inválido.")
            };
        }
    }
}
