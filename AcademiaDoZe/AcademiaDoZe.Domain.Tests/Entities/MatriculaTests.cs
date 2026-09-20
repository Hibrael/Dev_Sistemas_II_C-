// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);

    [Fact(DisplayName = "Matricula: criação bem-sucedida calcula DataFim e mantém os dados informados")]
    public void Deve_Criar_Matricula_Quando_Valida()
    {
        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, Hoje, "Condicionamento físico");

        Assert.True(result.IsSuccess);
        Assert.Equal("Condicionamento físico", result.Value!.Objetivo);
        Assert.Equal(MatriculaRestricoes.Nenhuma, result.Value.RestricoesMedicas);
        Assert.Null(result.Value.LaudoMedico);
        Assert.Null(result.Value.ObservacoesRestricoes);
    }

    [Theory(DisplayName = "Matricula: aluno inválido -> ALUNO_INVALIDO")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_AlunoIdInvalido(int alunoId)
    {
        var result = Matricula.Criar(1, alunoId, MatriculaPlano.Mensal, Hoje, "Objetivo qualquer");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ALUNO_INVALIDO");
    }

    [Fact(DisplayName = "Matricula: plano fora do enum -> PLANO_MATRICULA_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_PlanoInvalido()
    {
        var result = Matricula.Criar(1, 1, (MatriculaPlano)999, Hoje, "Objetivo qualquer");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "PLANO_MATRICULA_INVALIDO");
    }

    [Theory(DisplayName = "Matricula: objetivo vazio ou nulo -> OBJETIVO_MATRICULA_INVALIDO")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Deve_Falhar_Criacao_Quando_ObjetivoInvalido(string? objetivo)
    {
        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, Hoje, objetivo!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "OBJETIVO_MATRICULA_INVALIDO");
    }

    [Theory(DisplayName = "Matricula: calcular DataFim por plano (mensal/trimestral/semestral/anual)")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public void Deve_Calcular_DataFim_Corretamente(MatriculaPlano plano, int meses)
    {
        var result = Matricula.Criar(1, 1, plano, Hoje, "Objetivo qualquer");

        Assert.True(result.IsSuccess);
        Assert.Equal(Hoje.AddMonths(meses), result.Value!.DataFim);
    }

    [Fact(DisplayName = "Matricula: aceita combinação de restrições médicas via Flags (múltipla escolha)")]
    public void Deve_Aceitar_Combinacao_De_RestricoesMedicas_Via_Flags()
    {
        var restricoes = MatriculaRestricoes.Diabetes | MatriculaRestricoes.ProblemasCardiacos | MatriculaRestricoes.Labirintite;

        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, Hoje, "Objetivo qualquer", restricoes);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(result.Value.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasCardiacos));
        Assert.True(result.Value.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.False(result.Value.RestricoesMedicas.HasFlag(MatriculaRestricoes.CirurgiaDebilitante));
    }

    [Fact(DisplayName = "Matricula: restrição médica fora do enum Flags -> RESTRICAO_MEDICA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_RestricaoMedicaInvalida()
    {
        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, Hoje, "Objetivo qualquer", (MatriculaRestricoes)4096);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "RESTRICAO_MEDICA_INVALIDA");
    }

    [Fact(DisplayName = "Matricula: armazena laudo médico e observações da restrição quando informados")]
    public void Deve_Armazenar_LaudoMedico_E_Observacoes_Quando_Informados()
    {
        var laudo = Arquivo.Criar("laudo.jpg", [1, 2, 3]).Value!;

        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, Hoje, "Objetivo qualquer",
            MatriculaRestricoes.Labirintite, laudo, "Evitar exercícios de alto impacto");

        Assert.True(result.IsSuccess);
        Assert.Same(laudo, result.Value!.LaudoMedico);
        Assert.Equal("Evitar exercícios de alto impacto", result.Value.ObservacoesRestricoes);
    }

    [Fact(DisplayName = "Matricula: acumula notificações de vários campos inválidos simultaneamente")]
    public void Deve_Acumular_Multiplas_Notificacoes_Quando_VariosCamposInvalidos()
    {
        var result = Matricula.Criar(1, 0, (MatriculaPlano)999, Hoje, "");

        Assert.True(result.IsFailure);
        Assert.True(result.Notificacoes.Count >= 3);
    }
}
