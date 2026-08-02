//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Enums;
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class Matricula : Entity
    {
        public int AlunoId { get; private set; }
        public MatriculaPlano Plano { get; private set; }
        public MatriculaRestricoes Restricoes { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativa { get; private set; }

        private Matricula(int alunoId, MatriculaPlano plano, MatriculaRestricoes restricoes, DateTime dataInicio, DateTime dataFim, decimal valor)
        {
            AlunoId = alunoId;
            Plano = plano;
            Restricoes = restricoes;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Valor = valor;
            Ativa = true;
        }

        public static Matricula Criar(int alunoId, MatriculaPlano plano, MatriculaRestricoes restricoes, DateTime dataInicio, decimal valor)
        {
            if (alunoId <= 0)
                throw new ArgumentException("Aluno inválido.");

            if (!Enum.IsDefined(typeof(MatriculaPlano), plano))
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

        private static DateTime CalcularDataFim(DateTime dataInicio, MatriculaPlano plano)
        {
            return plano switch
            {
                MatriculaPlano.Mensal => dataInicio.AddMonths(1),
                MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
                MatriculaPlano.Semestral => dataInicio.AddMonths(6),
                MatriculaPlano.Anual => dataInicio.AddYears(1),
                _ => throw new ArgumentException("Plano de matrícula inválido.")
            };
        }
    }
}
