//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class Colaborador : Pessoa, IAggregateRoot
    {
        public DateOnly DataAdmissao { get; private set; }
        public DateOnly? DataDemissao { get; private set; }
        public ColaboradorTipo Tipo { get; private set; }
        public ColaboradorVinculo Vinculo { get; private set; }

        // Campos adicionais ao exemplo do material: cobrem regras de folha de pagamento
        // (salário) já presentes na versão anterior do projeto e mantidas aqui.
        public decimal Salario { get; private set; }

        private Colaborador(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email,
            Endereco endereco, Senha senha, Arquivo foto, DateOnly dataAdmissao, ColaboradorTipo tipo,
            ColaboradorVinculo vinculo, decimal salario)
            : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
        {
            DataAdmissao = dataAdmissao;
            Tipo = tipo;
            Vinculo = vinculo;
            Salario = salario;
        }

        public static Result<Colaborador> Criar(int id, string nome, string cpf, DateOnly dataNascimento,
            string telefone, string email, Logradouro endereco, string numeroCasa, string? complemento,
            string senha, Arquivo foto, DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo,
            decimal salario)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(nome))
                notificacoes.Add(new Notificacoes("Nome", "NOME_OBRIGATORIO"));
            else
                nome = NormalizadoService.LimparEspacos(nome);

            if (dataNascimento == default)
                notificacoes.Add(new Notificacoes("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
            else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
                // Valor conforme demonstrado no material; revisar se a idade mínima real
                // para colaborador na Academia do Zé deve ser 14, 16 ou 18 anos.
                notificacoes.Add(new Notificacoes("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

            if (dataAdmissao == default)
                notificacoes.Add(new Notificacoes("DataAdmissao", "DATA_ADMISSAO_OBRIGATORIO"));
            else if (dataAdmissao > DateOnly.FromDateTime(DateTime.Today))
                notificacoes.Add(new Notificacoes("DataAdmissao", "DATA_ADMISSAO_MAIOR_ATUAL"));

            if (!Enum.IsDefined(tipo))
                notificacoes.Add(new Notificacoes("Tipo", "TIPO_COLABORADOR_INVALIDO"));

            if (!Enum.IsDefined(vinculo))
                notificacoes.Add(new Notificacoes("Vinculo", "VINCULO_COLABORADOR_INVALIDO"));

            if (Enum.IsDefined(tipo) && Enum.IsDefined(vinculo) && tipo == ColaboradorTipo.Administrador && vinculo != ColaboradorVinculo.CLT)
                notificacoes.Add(new Notificacoes("Vinculo", "ADMINISTRADOR_CLT_INVALIDO"));

            if (salario <= 0)
                notificacoes.Add(new Notificacoes("Salario", "SALARIO_INVALIDO"));

            if (foto is null)
                notificacoes.Add(new Notificacoes("Foto", "FOTO_OBRIGATORIA"));

            // Instanciação e validação via Value Objects
            var cpfResult = Cpf.Criar(cpf);
            if (cpfResult.IsFailure) notificacoes.AddRange(cpfResult.Notificacoes);

            var telefoneResult = Telefone.Criar(telefone);
            if (telefoneResult.IsFailure) notificacoes.AddRange(telefoneResult.Notificacoes);

            var emailResult = Email.Criar(email);
            if (emailResult.IsFailure) notificacoes.AddRange(emailResult.Notificacoes);

            var senhaResult = Senha.Criar(senha);
            if (senhaResult.IsFailure) notificacoes.AddRange(senhaResult.Notificacoes);

            var enderecoResult = Endereco.Criar(endereco, numeroCasa, complemento);
            if (enderecoResult.IsFailure) notificacoes.AddRange(enderecoResult.Notificacoes);

            if (notificacoes.Count != 0)
                return Result<Colaborador>.Failure(notificacoes);

            var colaborador = new Colaborador(id, nome, cpfResult.Value!, dataNascimento, telefoneResult.Value!,
                emailResult.Value!, enderecoResult.Value!, senhaResult.Value!, foto!, dataAdmissao, tipo, vinculo, salario);

            return Result<Colaborador>.Success(colaborador);
        }

        public void Desligar(DateOnly dataDemissao)
        {
            if (DataDemissao is not null)
                throw new DomainException("Colaborador já foi demitido.");

            if (dataDemissao < DataAdmissao)
                throw new DomainException("Data de demissão não pode ser anterior à data de admissão.");

            if (dataDemissao > DateOnly.FromDateTime(DateTime.Today))
                throw new DomainException("Data de demissão não pode ser no futuro.");

            DataDemissao = dataDemissao;
        }
    }
}



