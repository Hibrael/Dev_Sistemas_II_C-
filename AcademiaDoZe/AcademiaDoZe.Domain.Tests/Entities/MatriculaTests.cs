// AcademiaDoZe.Domain.Tests
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    [Fact(DisplayName = "Matricula: criação bem-sucedida inicia ativa")]
    public void Deve_Criar_Matricula_Quando_Valida()
    {
        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), 150m);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Ativa);
    }

    [Theory(DisplayName = "Matricula: aluno inválido -> ALUNO_INVALIDO")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_AlunoIdInvalido(int alunoId)
    {
        var result = Matricula.Criar(1, alunoId, MatriculaPlano.Mensal, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), 150m);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "ALUNO_INVALIDO");
    }

    [Fact(DisplayName = "Matricula: plano fora do enum -> PLANO_MATRICULA_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_PlanoInvalido()
    {
        var result = Matricula.Criar(1, 1, (MatriculaPlano)999, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), 150m);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "PLANO_MATRICULA_INVALIDO");
    }

    [Theory(DisplayName = "Matricula: valor inválido -> VALOR_MATRICULA_INVALIDO")]
    [InlineData(0)]
    [InlineData(-50)]
    public void Deve_Falhar_Criacao_Quando_ValorInvalido(decimal valor)
    {
        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), valor);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notificacoes, n => n.Mensagem == "VALOR_MATRICULA_INVALIDO");
    }

    [Theory(DisplayName = "Matricula: calcular DataFim por plano (mensal/trimestral/semestral/anual)")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public void Deve_Calcular_DataFim_Corretamente(MatriculaPlano plano, int meses)
    {
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, 1, plano, MatriculaRestricoes.Nenhuma, inicio, 150m);

        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
    }

    [Fact(DisplayName = "Matricula: aceita combinação de restrições via Flags")]
    public void Deve_Aceitar_Combinacao_De_Restricoes_Via_Flags()
    {
        var restricoes = MatriculaRestricoes.Diabetes | MatriculaRestricoes.ProblemasCardiacos;

        var result = Matricula.Criar(1, 1, MatriculaPlano.Mensal, restricoes,
            DateOnly.FromDateTime(DateTime.Today), 150m);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Restricoes.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(result.Value.Restricoes.HasFlag(MatriculaRestricoes.ProblemasCardiacos));
    }

    [Fact(DisplayName = "Matricula: Cancelar uma matrícula ativa a torna inativa")]
    public void Deve_Cancelar_Matricula_Com_Sucesso()
    {
        var matricula = Matricula.Criar(1, 1, MatriculaPlano.Mensal, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), 150m).Value!;

        matricula.Cancelar();

        Assert.False(matricula.Ativa);
    }

    [Fact(DisplayName = "Matricula: Cancelar uma matrícula já cancelada lança DomainException")]
    public void Deve_Lancar_Excecao_Ao_Cancelar_Quando_JaCancelada()
    {
        var matricula = Matricula.Criar(1, 1, MatriculaPlano.Mensal, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), 150m).Value!;
        matricula.Cancelar();

        Assert.Throws<DomainException>(() => matricula.Cancelar());
    }

    [Fact(DisplayName = "Matricula: acumula notificações de vários campos inválidos simultaneamente")]
    public void Deve_Acumular_Multiplas_Notificacoes_Quando_VariosCamposInvalidos()
    {
        var result = Matricula.Criar(1, 0, (MatriculaPlano)999, MatriculaRestricoes.Nenhuma,
            DateOnly.FromDateTime(DateTime.Today), 0);

        Assert.True(result.IsFailure);
        Assert.True(result.Notificacoes.Count >= 3);
    }
}
