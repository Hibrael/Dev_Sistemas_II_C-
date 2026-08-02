//Hibrael Andre Cidade Xavier
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoAluno : Entity
    {
        public int AlunoId { get; private set; }
        public string Codigo { get; private set; }
        public bool Ativo { get; private set; }

        private AcessoAluno(int alunoId, string codigo)
        {
            AlunoId = alunoId;
            Codigo = codigo;
            Ativo = true;
        }

        public static AcessoAluno Criar(int alunoId, string codigo)
        {
            if (alunoId <= 0)
                throw new ArgumentException("Aluno inválido.");

            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("Código de acesso é obrigatório.");

            return new AcessoAluno(alunoId, codigo);
        }

        public void Bloquear()
        {
            if (!Ativo)
                throw new InvalidOperationException("Acesso já está bloqueado.");

            Ativo = false;
        }

        public void Liberar()
        {
            if (Ativo)
                throw new InvalidOperationException("Acesso já está liberado.");

            Ativo = true;
        }
    }
}
