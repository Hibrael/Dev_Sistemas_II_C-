// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AlunoTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro Teste", "Cidade Teste", "SP", "Brasil").Value!;

    private static Arquivo GetValidArquivo() =>
        Arquivo.Criar("foto.jpg", new byte[] { 1, 2, 3 }).Value!;

    private static Result<Aluno> CriarAluno(
        string nome = "João da Silva",
        string cpf = "529.982.247-25",
        DateOnly? dataNascimento = null,
        string telefone = "(11) 91234-5678",
        string email = "aluno@example.com",
        string numeroCasa = "123",
        string? complemento = null,
        string senha = "Abcdef12") =>
        Aluno.Criar(
            1,
            nome,
            cpf,
            dataNascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            telefone,
            email,
            GetValidLogradouro(),
            numeroCasa,
            complemento,
            senha,
            GetValidArquivo());

    [Theory(DisplayName = "Aluno: criação bem-sucedida com nome válido (trim aplicado)")]
    [InlineData(" João da Silva ", "João da Silva")]
    [InlineData("Maria", "Maria")]
    public void Deve_Criar_Com_Sucesso_Quando_NomeValido(string nome, string nomeEsperado)
    {
        var result = CriarAluno(nome: nome);

        Assert.True(result.IsSuccess);
        Assert.Equal(nomeEsperado, result.Value!.Nome);
    }

    [Theory(DisplayName = "Aluno: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = CriarAluno(nome: nome);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory(DisplayName = "Aluno: data de nascimento -> obrigatoriedade e idade mínima de 14 anos")]
    [InlineData("default", "DATA_NASCIMENTO_OBRIGATORIO")]
    [InlineData("menorDeIdade", "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoInvalida(string cenario, string mensagemEsperada)
    {
        var data = cenario == "default" ? default : DateOnly.FromDateTime(DateTime.Today.AddYears(-10));

        var result = CriarAluno(dataNascimento: data);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == mensagemEsperada);
    }

    [Fact(DisplayName = "Aluno: foto obrigatória -> FOTO_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_FotoNula()
    {
        var result = Aluno.Criar(
            1, "João da Silva", "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "(11) 91234-5678", "aluno@example.com",
            GetValidLogradouro(), "123", null, "Abcdef12", null!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "FOTO_OBRIGATORIA");
    }

    [Fact(DisplayName = "Aluno: CPF inválido -> notificação do ValueObject Cpf é propagada")]
    public void Deve_Propagar_Notificacao_Quando_CpfInvalido()
    {
        var result = CriarAluno(cpf: "111.111.111-11");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Fact(DisplayName = "Aluno: telefone inválido -> notificação do ValueObject Telefone é propagada")]
    public void Deve_Propagar_Notificacao_Quando_TelefoneInvalido()
    {
        var result = CriarAluno(telefone: "123");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "TELEFONE_INVALIDO");
    }

    [Fact(DisplayName = "Aluno: email inválido -> notificação do ValueObject Email é propagada")]
    public void Deve_Propagar_Notificacao_Quando_EmailInvalido()
    {
        var result = CriarAluno(email: "email-invalido");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "EMAIL_INVALIDO");
    }

    [Fact(DisplayName = "Aluno: senha fraca -> notificação do ValueObject Senha é propagada")]
    public void Deve_Propagar_Notificacao_Quando_SenhaInvalida()
    {
        var result = CriarAluno(senha: "123");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "SENHA_TAMANHO_MINIMO");
    }

    [Fact(DisplayName = "Aluno: número da casa vazio -> notificação do ValueObject Endereco é propagada")]
    public void Deve_Propagar_Notificacao_Quando_NumeroCasaInvalido()
    {
        var result = CriarAluno(numeroCasa: "");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "NUMERO_CASA_OBRIGATORIO");
    }

    [Fact(DisplayName = "Aluno: acumula notificações de vários campos inválidos simultaneamente")]
    public void Deve_Acumular_Multiplas_Notificacoes_Quando_VariosCamposInvalidos()
    {
        var result = CriarAluno(nome: "", cpf: "123", telefone: "123", email: "invalido", senha: "123");

        Assert.True(result.IsFailure);
        Assert.True(result.Notificacoes.Count >= 5);
    }

    [Fact(DisplayName = "Aluno: criação válida retorna sucesso com todos os dados corretos")]
    public void Deve_Criar_Com_Sucesso_Quando_TodosOsDadosValidos()
    {
        var result = CriarAluno();

        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value!.Cpf.Numero);
    }
}
