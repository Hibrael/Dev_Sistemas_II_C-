//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Classe base para as pessoas que interagem com a Academia do Zé (Aluno e Colaborador),
    /// concentrando os dados cadastrais e de acesso comuns a ambas.
    ///
    /// Pessoa é abstrata e NÃO possui um método "Criar" próprio: cada subclasse concentra
    /// suas próprias regras de negócio (e, por isso, seu próprio método de fábrica estático)
    /// e apenas repassa os valores já validados para este construtor protegido via base(...).
    /// Isso evita uma validação "genérica" que teria que ser sobrescrita/duplicada por Aluno
    /// e Colaborador sempre que suas regras específicas divergirem.
    /// </summary>
    public abstract class Pessoa : Entity
    {
        public string Nome { get; private set; }
        public Cpf Cpf { get; private set; }
        public DateOnly DataNascimento { get; private set; }
        public Telefone Telefone { get; private set; }
        public Email Email { get; private set; }
        public Endereco Endereco { get; private set; }
        public Senha Senha { get; private set; }
        public Arquivo Foto { get; private set; }

        protected Pessoa(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone,
            Email email, Endereco endereco, Senha senha, Arquivo foto) : base(id)
        {
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            Telefone = telefone;
            Email = email;
            Endereco = endereco;
            Senha = senha;
            Foto = foto;
        }
    }
}
