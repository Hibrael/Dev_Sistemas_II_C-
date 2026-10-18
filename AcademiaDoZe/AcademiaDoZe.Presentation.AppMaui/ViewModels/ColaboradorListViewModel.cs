//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "Id", "Email", "Tipo", "Vínculo"];

    private ObservableCollection<ColaboradorDto> _colaboradores = [];
    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set => SetProperty(ref _colaboradores, value);
    }

    private ColaboradorDto? _selectedColaborador;
    public ColaboradorDto? SelectedColaborador
    {
        get => _selectedColaborador;
        set => SetProperty(ref _selectedColaborador, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome"; // Nome, CPF, Id, Email, Tipo, Vínculo
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = "Colaboradores";
    }

    [RelayCommand]
    private async Task AddColaboradorAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("colaborador");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditColaboradorAsync(ColaboradorDto colaborador)
    {
        try
        {
            if (colaborador == null)
                return;

            await Shell.Current.GoToAsync($"colaborador?Id={colaborador.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }

    [RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // Limpa a lista atual
            await MainThread.InvokeOnMainThreadAsync(() => Colaboradores.Clear());

            IEnumerable<ColaboradorDto> resultados = [];

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Busca os colaboradores de acordo com o filtro selecionado
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _colaboradorService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Nome")
            {
                // IColaboradorService não tem busca por nome: filtra a listagem por trecho do nome
                var todos = await _colaboradorService.ObterTodosAsync(cts.Token) ?? [];
                var termo = SearchText.Trim();
                resultados = todos.Where(c => Contem(c.Nome, termo));
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpfLimpo = PessoaValidacao.ApenasDigitos(SearchText);
                if (cpfLimpo.Length != 11)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por CPF, informe os 11 dígitos numéricos.", "OK");
                    return;
                }
                if (!PessoaValidacao.CpfValido(cpfLimpo))
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "O CPF informado é inválido (dígitos verificadores não conferem).", "OK");
                    return;
                }
                var colaborador = await _colaboradorService.ObterPorCpfAsync(cpfLimpo, cts.Token);
                if (colaborador != null)
                    resultados = [colaborador];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por ID, informe um número inteiro positivo válido.", "OK");
                    return;
                }
                var colaborador = await _colaboradorService.ObterPorIdAsync(id, cts.Token);
                if (colaborador != null)
                    resultados = [colaborador];
            }
            else if (SelectedFilterType == "Email")
            {
                var emailBusca = SearchText.Trim();
                if (!PessoaValidacao.EmailValido(emailBusca))
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por e-mail, informe o endereço completo (ex: nome@dominio.com).", "OK");
                    return;
                }
                var colaborador = await _colaboradorService.ObterPorEmailAsync(emailBusca, cts.Token);
                if (colaborador != null)
                    resultados = [colaborador];
            }
            else if (SelectedFilterType == "Tipo")
            {
                // casa pelo nome do enum (AuxiliarDeLimpeza) ou pelo Display ("Auxiliar de Limpeza")
                var correspondentes = FiltrarEnum<AppColaboradorTipo>(SearchText);
                if (correspondentes.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", $"Informe um tipo válido para filtrar (ex: {ListarOpcoes<AppColaboradorTipo>()}).", "OK");
                    return;
                }
                var listaPorTipo = new List<ColaboradorDto>();
                foreach (var t in correspondentes)
                {
                    var itens = await _colaboradorService.ObterPorTipoAsync(t, cts.Token);
                    if (itens != null) listaPorTipo.AddRange(itens);
                }
                resultados = listaPorTipo.DistinctBy(c => c.Id);
            }
            else if (SelectedFilterType == "Vínculo")
            {
                var correspondentes = FiltrarEnum<AppColaboradorVinculo>(SearchText);
                if (correspondentes.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", $"Informe um vínculo válido para filtrar (ex: {ListarOpcoes<AppColaboradorVinculo>()}).", "OK");
                    return;
                }
                var listaPorVinculo = new List<ColaboradorDto>();
                foreach (var v in correspondentes)
                {
                    var itens = await _colaboradorService.ObterPorVinculoAsync(v, cts.Token);
                    if (itens != null) listaPorVinculo.AddRange(itens);
                }
                resultados = listaPorVinculo.DistinctBy(c => c.Id);
            }

            // Atualiza a coleção na thread principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                {
                    Colaboradores.Add(item);
                }
                OnPropertyChanged(nameof(Colaboradores));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de colaboradores expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // Limpa a lista atual antes de carregar novos dados
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();
                OnPropertyChanged(nameof(Colaboradores));
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradoresList = await _colaboradorService.ObterTodosAsync(cts.Token);

            if (colaboradoresList != null)
            {
                // Garantir que a atualização da UI aconteça na thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var colaborador in colaboradoresList)
                    {
                        Colaboradores.Add(colaborador);
                    }
                    OnPropertyChanged(nameof(Colaboradores));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento dos colaboradores expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteColaboradorAsync(ColaboradorDto colaborador)
    {
        if (colaborador == null)
            return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o colaborador {colaborador.Nome}?",
            "Sim", "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _colaboradorService.RemoverAsync(colaborador.Id, cts.Token);

            if (success)
            {
                Colaboradores.Remove(colaborador);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o colaborador.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão do colaborador expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Não Permitido",
                    "Este colaborador não pode ser excluído pois possui registros vinculados no sistema (ex: acessos).", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir colaborador: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Contém, sem diferenciar maiúsculas nem acentos ("estagio" encontra "Estagiário", "ze" encontra "Zé")
    private static bool Contem(string texto, string termo)
        => CultureInfo.InvariantCulture.CompareInfo.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    // Valores do enum cujo nome ou Display contém o termo digitado
    private static List<T> FiltrarEnum<T>(string texto) where T : struct, Enum
    {
        var termo = texto.Trim();
        return [.. Enum.GetValues<T>().Where(v => Contem(v.ToString(), termo) || Contem(v.GetDisplayName(), termo))];
    }

    // Lista os valores válidos para a mensagem de ajuda (ex: "CLT, PJ, Estagiário")
    private static string ListarOpcoes<T>() where T : struct, Enum
        => string.Join(", ", Enum.GetValues<T>().Select(v => v.GetDisplayName()));
}
