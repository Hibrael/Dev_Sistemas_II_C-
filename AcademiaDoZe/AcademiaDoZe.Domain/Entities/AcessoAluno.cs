//Hibrael Andre Cidade Xavier
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoAluno : Entity
    {
        public Aluno Aluno { get; private set; }
        public DateTime Datahora { get; private set; }

        private AcessoAluno(Aluno aluno)
        {
            Aluno = aluno;
            Datahora = DateTime.UtcNow;
        }

        public static AcessoAluno Criar(Aluno aluno)
        {
            return new AcessoAluno(aluno);
        }

    }
}
