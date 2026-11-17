//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

/// <summary>
/// Tela da catraca: localiza o aluno ou colaborador (CPF ou nome) e registra a entrada e a saída.
///
/// As regras (matrícula ativa, dias de plano, jornada de 8 h/6 h, soma do dia) ficam na camada de
/// aplicação (AcessoAlunoService/AcessoColaboradorService); aqui só se exibe o resultado. A saída
/// é sempre registrada pelo Id da própria pessoa, então é o mesmo aluno/colaborador que entrou.
/// </summary>
public partial class CatracaViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;

    public ObservableCollection<PessoaCatracaItem> Pessoas { get; } = [];
    public ObservableCollection<PresencaItem> Presentes { get; } = [];

    private bool _isAluno = true;
    /// <summary>Segmento selecionado: true = Aluno, false = Colaborador.</summary>
    public bool IsAluno
    {
        get => _isAluno;
        set
        {
            if (SetProperty(ref _isAluno, value))
            {
                OnPropertyChanged(nameof(IsColaborador));
                OnPropertyChanged(nameof(SearchPlaceholder));
                OnPropertyChanged(nameof(TituloLista));
            }
        }
    }
    public bool IsColaborador => !IsAluno;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string SearchPlaceholder => IsAluno ? "CPF ou nome do aluno (vazio lista todos)" : "CPF ou nome do colaborador (vazio lista todos)";
    public string TituloLista => IsAluno ? "Alunos" : "Colaboradores";

    // ---- Último resultado da catraca ----

    private RegistroAcessoResultadoDto? _resultado;
    public RegistroAcessoResultadoDto? Resultado
    {
        get => _resultado;
        private set
        {
            if (SetProperty(ref _resultado, value))
            {
                OnPropertyChanged(nameof(TemResultado));
                OnPropertyChanged(nameof(ResultadoCor));
                OnPropertyChanged(nameof(ResultadoFundo));
                OnPropertyChanged(nameof(ResultadoIcone));
                OnPropertyChanged(nameof(ResultadoDetalhes));
            }
        }
    }

    public bool TemResultado => Resultado is not null;

    // verde = liberado, vermelho = negado, âmbar = saída com jornada ultrapassada
    public Color ResultadoCor => Resultado switch
    {
        null => Colors.Transparent,
        { Permitido: false } => Color.FromArgb("#EF4444"),
        { UltrapassouJornada: true } => Color.FromArgb("#F59E0B"),
        _ => Color.FromArgb("#22C55E")
    };

    public Color ResultadoFundo => ResultadoCor.WithAlpha(0.12f);

    // Material Icons: block, warning, logout, login
    public string ResultadoIcone => Resultado switch
    {
        { Permitido: false } => "",
        { UltrapassouJornada: true } => "",
        { Saida: not null } => "",
        _ => ""
    };

    public IReadOnlyList<DetalheResultado> ResultadoDetalhes => MontarDetalhes(Resultado);

    public CatracaViewModel(IAlunoService alunoService, IColaboradorService colaboradorService,
        IAcessoAlunoService acessoAlunoService, IAcessoColaboradorService acessoColaboradorService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        Title = "Catraca";
    }

    [RelayCommand]
    private async Task SelecionarTipoAsync(string tipo)
    {
        var aluno = tipo == "Aluno";
        if (aluno == IsAluno) return;

        IsAluno = aluno;
        SearchText = string.Empty;
        await BuscarAsync();
    }

    /// <summary>Carga inicial da tela: lista do tipo selecionado e quem está na academia.</summary>
    [RelayCommand]
    private async Task CarregarAsync() => await BuscarAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        IsRefreshing = true;
        await BuscarAsync();
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // presentes primeiro: o selo "Na academia" dos cartões depende deles
            var presentes = await ObterPresentesAsync(cts.Token);
            var resultados = await ObterPessoasAsync(SearchText.Trim(), presentes, cts.Token);
            if (resultados is null)
                return; // termo inválido: a mensagem já foi exibida e a lista atual é mantida

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Presentes.Clear();
                foreach (var item in presentes) Presentes.Add(item);

                Pessoas.Clear();
                foreach (var item in resultados) Pessoas.Add(item);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar a catraca: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private Task RegistrarEntradaAsync(PessoaCatracaItem? pessoa) =>
        pessoa is null ? Task.CompletedTask : RegistrarAsync(pessoa.Id, pessoa.IsAluno, entrada: true);

    [RelayCommand]
    private Task RegistrarSaidaAsync(PessoaCatracaItem? pessoa) =>
        pessoa is null ? Task.CompletedTask : RegistrarAsync(pessoa.Id, pessoa.IsAluno, entrada: false);

    [RelayCommand]
    private Task RegistrarSaidaPresenteAsync(PresencaItem? presente) =>
        presente is null ? Task.CompletedTask : RegistrarAsync(presente.PessoaId, presente.IsAluno, entrada: false);

    [RelayCommand]
    private void FecharResultado() => Resultado = null;

    private async Task RegistrarAsync(int pessoaId, bool isAluno, bool entrada)
    {
        if (IsBusy)
            return;

        RegistroAcessoResultadoDto resultado;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            resultado = (isAluno, entrada) switch
            {
                (true, true) => await _acessoAlunoService.RegistrarEntradaAsync(pessoaId, cts.Token),
                (true, false) => await _acessoAlunoService.RegistrarSaidaAsync(pessoaId, cts.Token),
                (false, true) => await _acessoColaboradorService.RegistrarEntradaAsync(pessoaId, cts.Token),
                (false, false) => await _acessoColaboradorService.RegistrarSaidaAsync(pessoaId, cts.Token)
            };
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O registro na catraca expirou. Verifique a conexão com o banco.", "OK");
            return;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao registrar na catraca: {ex.Message}", "OK");
            return;
        }
        finally
        {
            IsBusy = false;
        }

        Resultado = resultado;

        // atualiza os selos e a lista de presentes com o novo estado
        await BuscarAsync();

        await Shell.Current.DisplayAlertAsync(resultado.Titulo, $"{resultado.NomePessoa}\n\n{resultado.Mensagem}", "OK");
    }

    private async Task<List<PresencaItem>> ObterPresentesAsync(CancellationToken cancellationToken)
    {
        var alunos = await _acessoAlunoService.ObterEmAbertoAsync(cancellationToken);
        var colaboradores = await _acessoColaboradorService.ObterEmAbertoAsync(cancellationToken);

        return alunos
            .Select(a => new PresencaItem { PessoaId = a.AlunoId, IsAluno = true, Nome = a.NomeAluno, Foto = a.Foto, Entrada = ParaLocal(a.DataHora) })
            .Concat(colaboradores.Select(c => new PresencaItem { PessoaId = c.ColaboradorId, IsAluno = false, Nome = c.NomeColaborador, Foto = c.Foto, Entrada = ParaLocal(c.DataHora) }))
            .OrderByDescending(p => p.Entrada)
            .ToList();
    }

    /// <summary>
    /// Busca pelo tipo selecionado: 11 dígitos = CPF; outro texto = trecho do nome; vazio = todos.
    /// Devolve null quando o termo é inválido (mensagem já exibida).
    /// </summary>
    private async Task<List<PessoaCatracaItem>?> ObterPessoasAsync(string termo, List<PresencaItem> presentes, CancellationToken cancellationToken)
    {
        var dentro = presentes.Where(p => p.IsAluno == IsAluno).Select(p => p.PessoaId).ToHashSet();
        var digitos = PessoaValidacao.ApenasDigitos(termo);
        var buscaPorCpf = termo.Length > 0 && digitos.Length == termo.Count(c => !".- ".Contains(c));

        if (buscaPorCpf && (digitos.Length != 11 || !PessoaValidacao.CpfValido(digitos)))
        {
            await Shell.Current.DisplayAlertAsync("Validação", "Informe um CPF válido com 11 dígitos, ou parte do nome.", "OK");
            return null;
        }

        if (IsAluno)
        {
            IEnumerable<AlunoDto> alunos;
            if (termo.Length == 0)
                alunos = await _alunoService.ObterTodosAsync(cancellationToken);
            else if (buscaPorCpf)
                alunos = await _alunoService.ObterPorCpfAsync(digitos, cancellationToken) is { } aluno ? [aluno] : [];
            else
                alunos = await _alunoService.ObterPorNomeAsync(termo, cancellationToken);

            return alunos
                .OrderBy(a => a.Nome)
                .Select(a => new PessoaCatracaItem
                {
                    Id = a.Id, IsAluno = true, Nome = a.Nome, Cpf = a.Cpf, Foto = a.Foto,
                    Detalhe = "Aluno", NaAcademia = dentro.Contains(a.Id)
                })
                .ToList();
        }

        IEnumerable<ColaboradorDto> colaboradores;
        if (buscaPorCpf)
            colaboradores = await _colaboradorService.ObterPorCpfAsync(digitos, cancellationToken) is { } colaborador ? [colaborador] : [];
        else
        {
            // o serviço de colaboradores não tem busca por nome: filtra a lista completa
            var todos = await _colaboradorService.ObterTodosAsync(cancellationToken);
            colaboradores = termo.Length == 0 ? todos : todos.Where(c => Contem(c.Nome, termo));
        }

        return colaboradores
            .OrderBy(c => c.Nome)
            .Select(c => new PessoaCatracaItem
            {
                Id = c.Id, IsAluno = false, Nome = c.Nome, Cpf = c.Cpf, Foto = c.Foto,
                Detalhe = $"{c.Tipo.GetDisplayName()} · {c.Vinculo.GetDisplayName()} · jornada de {(c.Vinculo == AppColaboradorVinculo.Estagio ? 6 : 8)} h",
                NaAcademia = dentro.Contains(c.Id)
            })
            .ToList();
    }

    private static List<DetalheResultado> MontarDetalhes(RegistroAcessoResultadoDto? r)
    {
        var detalhes = new List<DetalheResultado>();
        if (r is null) return detalhes;

        string Dur(TimeSpan t) => RegistroAcessoResultadoDto.FormatarDuracao(t);

        if (r.Entrada is { } entrada)
            detalhes.Add(new("", $"Entrada: {entrada:dd/MM/yyyy HH:mm:ss}"));
        if (r.Saida is { } saida)
            detalhes.Add(new("", $"Saída: {saida:dd/MM/yyyy HH:mm:ss}"));
        if (r.Permanencia is { } permanencia)
            detalhes.Add(new("", $"Tempo nesta ida: {Dur(permanencia)}"));
        if (r.FimPlano is { } fim && r.DiasRestantesPlano is { } dias)
            detalhes.Add(new("", $"Plano até {fim:dd/MM/yyyy} — {(dias == 0 ? "último dia" : $"{dias} dia(s) restante(s)")}"));
        if (r.PermanenciaNoDia is { } dia && r.JornadaDiariaMaxima is { } limite)
            detalhes.Add(new("", $"Total do dia: {Dur(dia)} de {Dur(limite)}"));
        if (r.Permitido && r.JornadaRestante is { } restante)
            detalhes.Add(new("", r.UltrapassouJornada ? "Jornada diária ultrapassada" : $"Ainda pode permanecer hoje: {Dur(restante)}"));

        return detalhes;
    }

    private static bool Contem(string texto, string termo)
        => CultureInfo.InvariantCulture.CompareInfo.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    // os acessos são gravados em UTC (ver AcessoAluno/AcessoColaborador)
    private static DateTime ParaLocal(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
}
