// AcademiaDoZe.Domain.Tests
//
// Testa a implementação REAL dos ValueObjects (arquivos enviados em 16/08/2026), não a
// versão mostrada no material em PDF, que já está desatualizada em alguns pontos (ex.:
// Cep.Numero em vez de Cep.Valor; regra de Senha exige letra+número, não maiúscula).
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro Teste", "Cidade Teste", "SP", "Brasil").Value!;

    // ---------- Cep ----------

    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio(string? input)
    {
        var result = Cep.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cep: quantidade de dígitos inválida -> CEP_INVALIDO")]
    [InlineData("123")]
    [InlineData("123456789")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos(string input)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "CEP_INVALIDO");
    }

    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData("12345-678", "12345678")]
    [InlineData("12345678", "12345678")]
    public void Deve_Criar_Cep_Quando_Valido(string input, string esperado)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.Numero);
    }

    [Fact(DisplayName = "Cep: ToString formata como 00000-000")]
    public void Deve_Formatar_Cep_Corretamente_No_ToString()
    {
        var cep = Cep.Criar("12345678").Value!;

        Assert.Equal("12345-678", cep.ToString());
    }

    // ---------- Cpf ----------

    [Theory(DisplayName = "Cpf: obrigatório -> CPF_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cpf: quantidade de dígitos inválida -> CPF_INVALIDO")]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Criacao_Quando_CpfComQuantidadeDigitosInvalida(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "CPF_INVALIDO");
    }

    // Dados validados manualmente contra o algoritmo em Cpf.ValidarCpf/CalcularDigitoVerificador:
    // 111.111.111-11 e 000.000.000-00 caem na checagem de dígitos repetidos;
    // 529.982.247-26 e 111.444.777-36 têm o segundo dígito verificador incorreto
    // (as versões corretas, 529.982.247-25 e 111.444.777-35, são usadas no teste de sucesso).
    [Theory(DisplayName = "Cpf: dígitos repetidos ou dígito verificador incorreto -> CPF_INVALIDO")]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    [InlineData("529.982.247-26")]
    [InlineData("111.444.777-36")]
    public void Deve_Falhar_Criacao_Quando_CpfInvalido(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Theory(DisplayName = "Cpf: valor válido -> sucesso, mantém apenas os dígitos")]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("111.444.777-35", "11144477735")]
    [InlineData("52998224725", "52998224725")]
    public void Deve_Criar_Cpf_Quando_Valido(string input, string esperado)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.Numero);
    }

    [Fact(DisplayName = "Cpf: ToString formata como 000.000.000-00")]
    public void Deve_Formatar_Cpf_Corretamente_No_ToString()
    {
        var cpf = Cpf.Criar("52998224725").Value!;

        Assert.Equal("529.982.247-25", cpf.ToString());
    }

    // ---------- Email ----------

    [Theory(DisplayName = "Email: obrigatório -> EMAIL_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_EmailNuloOuVazio(string? input)
    {
        var result = Email.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "EMAIL_OBRIGATORIO");
    }

    [Theory(DisplayName = "Email: formato inválido -> EMAIL_INVALIDO")]
    [InlineData("userexemplo.com")]
    [InlineData("@exemplo.com")]
    [InlineData("user@")]
    [InlineData("user@@exemplo.com")]
    public void Deve_Falhar_Criacao_Quando_FormatoEmailInvalido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "EMAIL_INVALIDO");
    }

    [Theory(DisplayName = "Email: formato válido -> sucesso, normalizado em minúsculo")]
    [InlineData("user@exemplo.com", "user@exemplo.com")]
    [InlineData("USER@EXEMPLO.COM", "user@exemplo.com")]
    public void Deve_Criar_Email_Quando_Valido(string input, string esperado)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.EnderecoEmail);
    }

    [Theory(DisplayName = "Email: espaços (nas bordas ou internos) são removidos ao criar")]
    [InlineData("  User@Exemplo.com  ", "user@exemplo.com")]
    [InlineData("us er@exemplo.com", "user@exemplo.com")]
    public void Deve_Criar_Email_E_RemoverEspacos_Quando_InputTemEspacos(string input, string esperado)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.EnderecoEmail);
    }

    // ---------- Endereco ----------

    [Fact(DisplayName = "Endereco: logradouro obrigatório -> LOGRADOURO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_LogradouroNulo()
    {
        var result = Endereco.Criar(null!, "10");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "LOGRADOURO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Endereco: número da casa obrigatório -> NUMERO_CASA_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_NumeroCasaNuloOuVazio(string? numeroCasa)
    {
        var result = Endereco.Criar(GetValidLogradouro(), numeroCasa!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "NUMERO_CASA_OBRIGATORIO");
    }

    [Fact(DisplayName = "Endereco: criação válida com número e complemento")]
    public void Deve_Criar_Endereco_Quando_Valido()
    {
        var logradouro = GetValidLogradouro();
        var result = Endereco.Criar(logradouro, "10", "Bloco A");

        Assert.True(result.IsSuccess);
        Assert.Equal(logradouro, result.Value!.Logradouro);
        Assert.Equal("10", result.Value.NumeroCasa);
        Assert.Equal("Bloco A", result.Value.Complemento);
    }

    [Fact(DisplayName = "Endereco: complemento é opcional e permanece nulo quando não informado")]
    public void Deve_Criar_Endereco_Quando_Valido_Sem_Complemento()
    {
        var result = Endereco.Criar(GetValidLogradouro(), "10");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Complemento);
    }

    [Fact(DisplayName = "Endereco: espaços de número e complemento são normalizados")]
    public void Deve_Limpar_Espacos_Do_NumeroCasa_E_Complemento()
    {
        var result = Endereco.Criar(GetValidLogradouro(), "  10  ", "  Bloco   A  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("10", result.Value!.NumeroCasa);
        Assert.Equal("Bloco A", result.Value.Complemento);
    }

    [Fact(DisplayName = "Endereco: ToString concatena logradouro, número e complemento quando presente")]
    public void Deve_Formatar_Endereco_Com_Complemento_No_ToString()
    {
        var logradouro = GetValidLogradouro();
        var endereco = Endereco.Criar(logradouro, "10", "Bloco A").Value!;

        Assert.Equal($"{logradouro.Nome}, 10 - Bloco A", endereco.ToString());
    }

    [Fact(DisplayName = "Endereco: ToString omite complemento quando ausente")]
    public void Deve_Formatar_Endereco_Sem_Complemento_No_ToString()
    {
        var logradouro = GetValidLogradouro();
        var endereco = Endereco.Criar(logradouro, "10").Value!;

        Assert.Equal($"{logradouro.Nome}, 10", endereco.ToString());
    }

    // ---------- Senha ----------

    [Theory(DisplayName = "Senha: menor que o tamanho mínimo -> SENHA_TAMANHO_MINIMO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc123")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia(string? input)
    {
        var result = Senha.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "SENHA_TAMANHO_MINIMO");
    }

    // Regra real (Senha.ContemLetraENumero) exige letra E número — diferente da regra de
    // "maiúscula" mostrada no material em PDF, que não existe na implementação atual.
    [Theory(DisplayName = "Senha: precisa conter letra e número -> SENHA_REQUISITOS_INVALIDOS")]
    [InlineData("12345678")]
    [InlineData("abcdefgh")]
    public void Deve_Falhar_Criacao_Quando_SenhaSemRequisitosMinimos(string input)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "SENHA_REQUISITOS_INVALIDOS");
    }

    [Fact(DisplayName = "Senha: criação válida gera Hash e Salt")]
    public void Deve_Criar_Senha_Quando_Valida()
    {
        var result = Senha.Criar("abc12345");

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.Hash));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.Salt));
    }

    [Fact(DisplayName = "Senha: duas criações com a mesma senha geram Salt/Hash diferentes")]
    public void Deve_Gerar_Salt_Diferente_A_Cada_Criacao()
    {
        var senha1 = Senha.Criar("abc12345").Value!;
        var senha2 = Senha.Criar("abc12345").Value!;

        Assert.NotEqual(senha1.Salt, senha2.Salt);
        Assert.NotEqual(senha1.Hash, senha2.Hash);
    }

    [Fact(DisplayName = "Senha: Verificar retorna true para senha correta e false para incorreta")]
    public void Deve_Verificar_Senha_Corretamente()
    {
        var senha = Senha.Criar("abc12345").Value!;

        Assert.True(senha.Verificar("abc12345"));
        Assert.False(senha.Verificar("outraSenha1"));
    }

    [Fact(DisplayName = "Senha: Restaurar reconstrói a senha a partir de Hash e Salt existentes")]
    public void Deve_Restaurar_Senha_A_Partir_De_Hash_E_Salt()
    {
        var original = Senha.Criar("abc12345").Value!;

        var restaurada = Senha.Restaurar(original.Hash, original.Salt);

        Assert.True(restaurada.Verificar("abc12345"));
    }

    // ---------- Telefone ----------

    [Theory(DisplayName = "Telefone: obrigatório -> TELEFONE_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    [Theory(DisplayName = "Telefone: quantidade de dígitos inválida -> TELEFONE_INVALIDO")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "TELEFONE_INVALIDO");
    }

    [Theory(DisplayName = "Telefone: formatos válidos (fixo com 10 e celular com 11 dígitos)")]
    [InlineData("1123456789", "1123456789")]
    [InlineData("(11) 91234-5678", "11912345678")]
    public void Deve_Criar_Telefone_Quando_Valido(string input, string esperado)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.Numero);
    }

    [Fact(DisplayName = "Telefone: ToString formata fixo (10 dígitos) como (00) 0000-0000")]
    public void Deve_Formatar_Telefone_Fixo_No_ToString()
    {
        var telefone = Telefone.Criar("1123456789").Value!;

        Assert.Equal("(11) 2345-6789", telefone.ToString());
    }

    [Fact(DisplayName = "Telefone: ToString formata celular (11 dígitos) como (00) 00000-0000")]
    public void Deve_Formatar_Telefone_Celular_No_ToString()
    {
        var telefone = Telefone.Criar("11987654321").Value!;

        Assert.Equal("(11) 98765-4321", telefone.ToString());
    }

    // ---------- Arquivo ----------

    [Theory(DisplayName = "Arquivo: nome obrigatório -> ARQUIVO_NOME_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_ArquivoNomeNuloOuVazio(string? nome)
    {
        var result = Arquivo.Criar(nome!, new byte[] { 1, 2, 3 });

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ARQUIVO_NOME_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: conteúdo nulo -> ARQUIVO_CONTEUDO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoConteudoNulo()
    {
        var result = Arquivo.Criar("foto.jpg", null!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ARQUIVO_CONTEUDO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: conteúdo vazio -> ARQUIVO_CONTEUDO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoConteudoVazio()
    {
        var result = Arquivo.Criar("foto.jpg", Array.Empty<byte>());

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ARQUIVO_CONTEUDO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Arquivo: extensão fora da lista permitida -> ARQUIVO_EXTENSAO_INVALIDA")]
    [InlineData("foto.gif")]
    [InlineData("documento.pdf")]
    [InlineData("arquivo")]
    public void Deve_Falhar_Criacao_Quando_ArquivoExtensaoInvalida(string nome)
    {
        var result = Arquivo.Criar(nome, new byte[] { 1, 2, 3 });

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ARQUIVO_EXTENSAO_INVALIDA");
    }

    [Fact(DisplayName = "Arquivo: tamanho acima de 5 MB -> ARQUIVO_TAMANHO_EXCEDIDO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoTamanhoExcedeLimite()
    {
        var conteudo = new byte[5 * 1024 * 1024 + 1];

        var result = Arquivo.Criar("foto.jpg", conteudo);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ARQUIVO_TAMANHO_EXCEDIDO");
    }

    [Theory(DisplayName = "Arquivo: extensões permitidas -> sucesso, extensão normalizada em minúsculo")]
    [InlineData("foto.jpg", ".jpg")]
    [InlineData("foto.jpeg", ".jpeg")]
    [InlineData("FOTO.PNG", ".png")]
    public void Deve_Criar_Arquivo_Quando_Valido(string nome, string extensaoEsperada)
    {
        var result = Arquivo.Criar(nome, new byte[] { 1, 2, 3 });

        Assert.True(result.IsSuccess);
        Assert.Equal(extensaoEsperada, result.Value!.Extensao);
        Assert.Equal(3L, result.Value.TamanhoBytes);
    }
}
