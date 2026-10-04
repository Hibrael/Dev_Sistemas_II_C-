using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    private int _totalLogradouros;
    public int TotalLogradouros { get => _totalLogradouros; set => SetProperty(ref _totalLogradouros, value); }
    private int _totalAlunos;
    public int TotalAlunos { get => _totalAlunos; set => SetProperty(ref _totalAlunos, value); }
    private int _totalColaboradores;
    public int TotalColaboradores { get => _totalColaboradores; set => SetProperty(ref _totalColaboradores, value); }
    private int _totalMatriculas;
    public int TotalMatriculas { get => _totalMatriculas; set => SetProperty(ref _totalMatriculas, value); }

    // indicadores derivados, exibidos no mosaico da dashboard
    private int _matriculasAtivas;
    public int MatriculasAtivas { get => _matriculasAtivas; set => SetProperty(ref _matriculasAtivas, value); }
    private double _percentualMatriculasAtivas;
    public double PercentualMatriculasAtivas { get => _percentualMatriculasAtivas; set => SetProperty(ref _percentualMatriculasAtivas, value); }
    private int _alunosComMatriculaAtiva;
    public int AlunosComMatriculaAtiva { get => _alunosComMatriculaAtiva; set => SetProperty(ref _alunosComMatriculaAtiva, value); }
    private double _percentualAlunosMatriculados;
    public double PercentualAlunosMatriculados { get => _percentualAlunosMatriculados; set => SetProperty(ref _percentualAlunosMatriculados, value); }

    // data por extenso no cabeçalho, ex.: "Sábado, 03 de outubro de 2026"
    public string DataHoje
    {
        get
        {
            var texto = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));
            return char.ToUpper(texto[0]) + texto[1..];
        }
    }

    public DashboardListViewModel(ILogradouroService logradouroService, IAlunoService alunoService, IColaboradorService colaboradorService, IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradourosTask = _logradouroService.ObterTodosAsync(cts.Token);
            var alunosTask = _alunoService.ObterTodosAsync(cts.Token);
            var colaboradoresTask = _colaboradorService.ObterTodosAsync(cts.Token);
            var matriculasTask = _matriculaService.ObterTodasAsync(cts.Token);

            await Task.WhenAll(logradourosTask, alunosTask, colaboradoresTask, matriculasTask);

            var matriculas = (await matriculasTask).ToList();

            TotalLogradouros = (await logradourosTask).Count();
            TotalAlunos = (await alunosTask).Count();
            TotalColaboradores = (await colaboradoresTask).Count();
            TotalMatriculas = matriculas.Count;

            // matrícula ativa: hoje está entre a data de início e a data final do plano
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var ativas = matriculas.Where(m => m.DataInicio <= hoje && m.DataFim >= hoje).ToList();
            MatriculasAtivas = ativas.Count;
            PercentualMatriculasAtivas = TotalMatriculas > 0 ? (double)MatriculasAtivas / TotalMatriculas : 0;
            AlunosComMatriculaAtiva = ativas.Select(m => m.AlunoId).Distinct().Count();
            PercentualAlunosMatriculados = TotalAlunos > 0 ? (double)AlunosComMatriculaAtiva / TotalAlunos : 0;
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo Esgotado",
                "A conexão com o banco de dados expirou (timeout). Verifique se o caminho ou os dados de conexão estão corretos.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar dados do painel: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLogradourosAsync() => await Shell.Current.GoToAsync("//logradouros");    [RelayCommand]
    private async Task NavigateToAlunosAsync() => await Shell.Current.GoToAsync("//alunos");
    [RelayCommand]
    private async Task NavigateToColaboradoresAsync() => await Shell.Current.GoToAsync("//colaboradores");
    [RelayCommand]
    private async Task NavigateToMatriculasAsync() => await Shell.Current.GoToAsync("//matriculas");
}
