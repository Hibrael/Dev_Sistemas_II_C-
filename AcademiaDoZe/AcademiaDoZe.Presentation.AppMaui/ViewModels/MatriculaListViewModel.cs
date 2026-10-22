//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    public ObservableCollection<string> FilterTypes { get; } = ["Ativas", "Id", "Id Aluno", "CPF", "Nome", "Plano", "Vencendo em dias"];

    private ObservableCollection<MatriculaDto> _matriculas = [];
    public ObservableCollection<MatriculaDto> Matriculas
    {
        get => _matriculas;
        set => SetProperty(ref _matriculas, value);
    }

    private MatriculaDto? _selectedMatricula;
    public MatriculaDto? SelectedMatricula
    {
        get => _selectedMatricula;
        set => SetProperty(ref _selectedMatricula, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Ativas"; // Ativas, Id, Id Aluno, CPF, Nome, Plano, Vencendo em dias
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set
        {
            if (SetProperty(ref _selectedFilterType, value))
                OnPropertyChanged(nameof(SearchPlaceholder));
        }
    }

    // Dica do campo de busca conforme o filtro escolhido
    public string SearchPlaceholder => SelectedFilterType switch
    {
        "Ativas" => "Id do aluno (vazio lista todas as ativas)",
        "Id" => "Número da matrícula",
        "Id Aluno" => "Id do aluno",
        "CPF" => "CPF do aluno (11 dígitos)",
        "Nome" => "Nome do aluno (ou parte dele)",
        "Plano" => ListarOpcoes<AppMatriculaPlano>(),
        "Vencendo em dias" => "Quantidade de dias (ex.: 30)",
        _ => "Digite o valor..."
    };

    public MatriculaListViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = "Matrículas";
    }

    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("matricula");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDto matricula)
    {
        try
        {
            if (matricula == null)
                return;

            await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            // outra operação (busca/exclusão) em andamento: a carga não rodaria, então libera o indicador do RefreshView
            IsRefreshing = false;
            return;
        }

        IsRefreshing = true;
        await LoadMatriculasAsync();
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            IEnumerable<MatriculaDto> resultados = [];
            var termo = SearchText.Trim();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Busca as matrículas de acordo com o filtro selecionado
            if (string.IsNullOrWhiteSpace(termo))
            {
                // sem valor: "Ativas" lista só as vigentes; os demais filtros listam todas
                resultados = SelectedFilterType == "Ativas"
                    ? await _matriculaService.ObterAtivasAsync(0, cts.Token) ?? []
                    : await _matriculaService.ObterTodasAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Ativas")
            {
                // com valor: ativas de um aluno específico, pelo Id do aluno
                if (!int.TryParse(termo, out int alunoId) || alunoId <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para filtrar as ativas de um aluno, informe o Id do aluno (número inteiro positivo) ou deixe o campo vazio para ver todas.", "OK");
                    return;
                }
                resultados = await _matriculaService.ObterAtivasAsync(alunoId, cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(termo, out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por ID, informe um número inteiro positivo válido.", "OK");
                    return;
                }
                var matricula = await _matriculaService.ObterPorIdAsync(id, cts.Token);
                if (matricula != null)
                    resultados = [matricula];
            }
            else if (SelectedFilterType == "Id Aluno")
            {
                if (!int.TryParse(termo, out int alunoId) || alunoId <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar pelo ID do aluno, informe um número inteiro positivo válido.", "OK");
                    return;
                }
                resultados = await _matriculaService.ObterPorAlunoIdAsync(alunoId, cts.Token) ?? [];
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpfLimpo = PessoaValidacao.ApenasDigitos(termo);
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
                // o CPF identifica o aluno; as matrículas são buscadas pelo Id dele
                var aluno = await _alunoService.ObterPorCpfAsync(cpfLimpo, cts.Token);
                if (aluno != null)
                    resultados = await _matriculaService.ObterPorAlunoIdAsync(aluno.Id, cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Nome")
            {
                // busca por trecho do nome, sem diferenciar maiúsculas/minúsculas (feita pelo Service de alunos)
                IEnumerable<AlunoDto> alunos = await _alunoService.ObterPorNomeAsync(termo, cts.Token) ?? [];
                var listaPorNome = new List<MatriculaDto>();
                foreach (var aluno in alunos)
                {
                    var itens = await _matriculaService.ObterPorAlunoIdAsync(aluno.Id, cts.Token);
                    if (itens != null) listaPorNome.AddRange(itens);
                }
                resultados = listaPorNome;
            }
            else if (SelectedFilterType == "Plano")
            {
                // casa pelo nome do enum ou pelo Display, sem diferenciar maiúsculas nem acentos
                var correspondentes = FiltrarEnum<AppMatriculaPlano>(termo);
                if (correspondentes.Count == 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", $"Informe um plano válido para filtrar (ex: {ListarOpcoes<AppMatriculaPlano>()}).", "OK");
                    return;
                }
                var listaPorPlano = new List<MatriculaDto>();
                foreach (var plano in correspondentes)
                {
                    var itens = await _matriculaService.ObterPorPlanoAsync(plano, cts.Token);
                    if (itens != null) listaPorPlano.AddRange(itens);
                }
                resultados = listaPorPlano.DistinctBy(m => m.Id);
            }
            else if (SelectedFilterType == "Vencendo em dias")
            {
                if (!int.TryParse(termo, out int dias) || dias < 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por vencimento, informe a quantidade de dias (número inteiro, zero ou mais).", "OK");
                    return;
                }
                resultados = await _matriculaService.ObterVencendoEmDiasAsync(dias, cts.Token) ?? [];
            }

            // Troca o conteúdo da lista na thread principal. A limpeza fica aqui, depois das validações,
            // para que um valor inválido não esvazie a lista e mostre "Nenhuma matrícula encontrada"
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();
                foreach (var item in resultados)
                {
                    Matriculas.Add(item);
                }
                OnPropertyChanged(nameof(Matriculas));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de matrículas expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // Limpa a lista atual antes de carregar novos dados
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();
                OnPropertyChanged(nameof(Matriculas));
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var matriculasList = await _matriculaService.ObterTodasAsync(cts.Token);

            if (matriculasList != null)
            {
                // Garantir que a atualização da UI aconteça na thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var matricula in matriculasList)
                    {
                        Matriculas.Add(matricula);
                    }
                    OnPropertyChanged(nameof(Matriculas));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento das matrículas expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto matricula)
    {
        if (matricula == null)
            return;

        var aluno = matricula.AlunoMatricula?.Nome ?? $"aluno {matricula.AlunoId}";
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a matrícula #{matricula.Id} de {aluno}?",
            "Sim", "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _matriculaService.RemoverAsync(matricula.Id, cts.Token);

            if (success)
            {
                Matriculas.Remove(matricula);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula excluída com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir a matrícula.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão da matrícula expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Contém, sem diferenciar maiúsculas nem acentos ("semestr" encontra "Semestral")
    private static bool Contem(string texto, string termo)
        => CultureInfo.InvariantCulture.CompareInfo.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    // Valores do enum cujo nome ou Display contém o termo digitado
    private static List<T> FiltrarEnum<T>(string texto) where T : struct, Enum
    {
        var termo = texto.Trim();
        return [.. Enum.GetValues<T>().Where(v => Contem(v.ToString(), termo) || Contem(v.GetDisplayName(), termo))];
    }

    // Lista os valores válidos para a mensagem de ajuda (ex: "Mensal, Trimestral, Semestral, Anual")
    private static string ListarOpcoes<T>() where T : struct, Enum
        => string.Join(", ", Enum.GetValues<T>().Select(v => v.GetDisplayName()));
}
