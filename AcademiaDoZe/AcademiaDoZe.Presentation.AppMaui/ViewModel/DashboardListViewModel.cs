using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private int _totalLogradouros;
    public int TotalLogradouros { get => _totalLogradouros; set => SetProperty(ref _totalLogradouros, value); }
    private int _totalAlunos;
    public int TotalAlunos { get => _totalAlunos; set => SetProperty(ref _totalAlunos, value); }
    private int _totalColaboradores;
    public int TotalColaboradores { get => _totalColaboradores; set => SetProperty(ref _totalColaboradores, value); }
    private int _totalMatriculas;
    public int TotalMatriculas { get => _totalMatriculas; set => SetProperty(ref _totalMatriculas, value); }

    public DashboardListViewModel()
    {
        Title = "Dashboard";
    }

    [RelayCommand]
    private Task LoadDashboardDataAsync()
    {
        if (IsBusy)
            return Task.CompletedTask;
        try
        {
            IsBusy = true;
            // os totais serão carregados dos services nas próximas etapas
            TotalLogradouros = 0;
            TotalAlunos = 0;
            TotalColaboradores = 0;
            TotalMatriculas = 0;
        }
        finally
        {
            IsBusy = false;
        }
        return Task.CompletedTask;
    }
    [RelayCommand]
    private async Task NavigateToLogradourosAsync() => await Shell.Current.GoToAsync("//logradouros");
    [RelayCommand]
    private async Task NavigateToAlunosAsync() => await Shell.Current.GoToAsync("//alunos");
    [RelayCommand]
    private async Task NavigateToColaboradoresAsync() => await Shell.Current.GoToAsync("//colaboradores");
    [RelayCommand]
    private async Task NavigateToMatriculasAsync() => await Shell.Current.GoToAsync("//matriculas");
}
