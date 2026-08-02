//Hibrael Andre Cidade Xavier
using System;

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoColaborador : Entity
    {
        public int ColaboradorId { get; private set; }
        public string Codigo { get; private set; }
        public bool Ativo { get; private set; }

        private AcessoColaborador(int colaboradorId, string codigo)
        {
            ColaboradorId = colaboradorId;
            Codigo = codigo;
            Ativo = true;
        }

        public static AcessoColaborador Criar(int colaboradorId, string codigo)
        {
            if (colaboradorId <= 0)
                throw new ArgumentException("Colaborador inválido.");

            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("Código de acesso é obrigatório.");

            return new AcessoColaborador(colaboradorId, codigo);
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
