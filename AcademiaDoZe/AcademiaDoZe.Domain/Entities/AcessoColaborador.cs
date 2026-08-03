//Hibrael Andre Cidade Xavier
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoColaborador : Entity
    {
        public Colaborador Colaborador { get; private set; }
        public DateTime DataHora { get; private set; }

        private AcessoColaborador(Colaborador colaborador)
        {
            Colaborador = colaborador;
            DataHora = DateTime.UtcNow  ;
        }

        public static AcessoColaborador Criar(Colaborador colaborador)
        {
            return new AcessoColaborador(colaborador);
        }
    }
}
