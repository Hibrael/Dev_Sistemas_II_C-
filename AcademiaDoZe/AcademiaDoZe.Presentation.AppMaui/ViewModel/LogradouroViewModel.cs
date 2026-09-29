using AcademiaDoZe.Application.DTOs;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroViewModel : BaseViewModel
{
    private LogradouroDto _logradouro = new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = string.Empty
    };
    public LogradouroDto Logradouro
    {
        get => _logradouro;
        set => SetProperty(ref _logradouro, value);
    }

    public LogradouroViewModel()
    {
        Title = "Cadastro de Logradouro";
    }

    // busca e gravação serão ligadas ao ILogradouroService nas próximas etapas

    [RelayCommand]
    private Task SearchByCepAsync() => Task.CompletedTask;

    [RelayCommand]
    private Task SaveLogradouroAsync() => Task.CompletedTask;

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
