using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly LocalidadesService _localidades;

    // ===== Seleção de País / Estado / Cidade =====
    // Brasil: Estado e Cidade são escolhidos em listas (UFs e municípios do IBGE).
    // Outros países: Estado (2 letras) e Cidade são digitados, mas só aceitam letras.

    public ObservableCollection<string> Paises { get; }
    public ObservableCollection<string> Ufs { get; }
    public ObservableCollection<string> Cidades { get; } = [];

    // evita que a sincronização (ao carregar um registro) limpe os campos dependentes
    private bool _sincronizando;

    private bool _isBrasil = true;
    public bool IsBrasil
    {
        get => _isBrasil;
        set { if (SetProperty(ref _isBrasil, value)) OnPropertyChanged(nameof(IsOutroPais)); }
    }
    public bool IsOutroPais => !IsBrasil;

    private string? _paisSelecionado;
    public string? PaisSelecionado
    {
        get => _paisSelecionado;
        set
        {
            if (!SetProperty(ref _paisSelecionado, value))
                return;

            Logradouro.Pais = value ?? string.Empty;
            IsBrasil = value == LocalidadesService.Brasil;

            if (!_sincronizando)
            {
                // trocar o país invalida estado e cidade escolhidos antes
                Logradouro.Estado = string.Empty;
                Logradouro.Cidade = string.Empty;
                _estadoSelecionado = null;
                _cidadeSelecionada = null;
                Cidades.Clear();
                OnPropertyChanged(nameof(EstadoSelecionado));
                OnPropertyChanged(nameof(CidadeSelecionada));
                OnPropertyChanged(nameof(Logradouro));
            }
        }
    }

    private string? _estadoSelecionado;
    public string? EstadoSelecionado
    {
        get => _estadoSelecionado;
        set
        {
            if (!SetProperty(ref _estadoSelecionado, value))
                return;

            Logradouro.Estado = value ?? string.Empty;

            if (!_sincronizando)
            {
                // trocar a UF limpa a cidade e recarrega a lista de municípios
                CidadeSelecionada = null;
                _ = CarregarCidadesAsync(value);
            }
        }
    }

    private string? _cidadeSelecionada;
    public string? CidadeSelecionada
    {
        get => _cidadeSelecionada;
        set
        {
            if (SetProperty(ref _cidadeSelecionada, value))
                Logradouro.Cidade = value ?? string.Empty;
        }
    }

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

    private int _logradouroId;
    public int LogradouroId
    {
        get => _logradouroId;
        set => SetProperty(ref _logradouroId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public LogradouroViewModel(ILogradouroService logradouroService, LocalidadesService localidades)
    {
        _logradouroService = logradouroService;
        _localidades = localidades;
        Paises = new ObservableCollection<string>(localidades.Paises);
        Ufs = new ObservableCollection<string>(localidades.Ufs);
        Title = "Detalhes do Logradouro";
    }

    private async Task CarregarCidadesAsync(string? uf)
    {
        try
        {
            var cidades = await _localidades.ObterCidadesAsync(uf);
            Cidades.Clear();
            foreach (var cidade in cidades)
                Cidades.Add(cidade);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar a lista de cidades: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Reflete o logradouro carregado (edição, busca por CEP ou novo) nas listas da tela.
    /// Valores antigos que não existem nas listas são incluídos, para não se perderem ao editar;
    /// a validação ao salvar ainda exige que, para o Brasil, UF e cidade sejam válidas.
    /// </summary>
    private async Task SincronizarSelecoesAsync()
    {
        // captura os valores antes de mexer nas listas: ao limpar/recarregar um Picker, ele pode
        // devolver "nenhuma seleção" para o ViewModel e apagar o valor original do logradouro
        var paisOriginal = Logradouro.Pais;
        var ufOriginal = Logradouro.Estado;
        var cidadeOriginal = Logradouro.Cidade;

        _sincronizando = true;
        try
        {
            var pais = string.IsNullOrWhiteSpace(paisOriginal) ? LocalidadesService.Brasil : paisOriginal.Trim();
            pais = Paises.FirstOrDefault(p => string.Equals(p, pais, StringComparison.CurrentCultureIgnoreCase)) ?? pais;
            if (!Paises.Contains(pais))
                Paises.Add(pais);
            PaisSelecionado = pais;

            if (!IsBrasil)
            {
                _estadoSelecionado = null;
                _cidadeSelecionada = null;
                Cidades.Clear();
                Logradouro.Estado = ufOriginal;
                Logradouro.Cidade = cidadeOriginal;
                return;
            }

            var uf = ufOriginal?.Trim().ToUpperInvariant() ?? string.Empty;
            await CarregarCidadesAsync(uf);
            if (uf.Length > 0 && !Ufs.Contains(uf))
                Ufs.Add(uf);
            EstadoSelecionado = uf.Length > 0 ? uf : null;
            Logradouro.Estado = uf;

            var cidade = cidadeOriginal?.Trim() ?? string.Empty;
            var cidadeDaLista = Cidades.FirstOrDefault(c => string.Equals(c, cidade, StringComparison.CurrentCultureIgnoreCase));
            if (cidadeDaLista == null && cidade.Length > 0)
            {
                Cidades.Add(cidade);
                cidadeDaLista = cidade;
            }
            CidadeSelecionada = cidadeDaLista;
            Logradouro.Cidade = cidadeDaLista ?? string.Empty;
        }
        finally
        {
            _sincronizando = false;
            OnPropertyChanged(nameof(Logradouro));
        }
    }

    public async Task InitializeAsync()
    {
        if (LogradouroId > 0)
        {
            IsEditMode = true;
            Title = "Editar Logradouro";
            await LoadLogradouroAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Logradouro";
            Logradouro = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = LocalidadesService.Brasil
            };
            await SincronizarSelecoesAsync();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task LoadLogradouroAsync()
    {
        if (LogradouroId <= 0)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorIdAsync(LogradouroId, cts.Token);
            if (logradouroData != null)
            {
                Logradouro = logradouroData;
                await SincronizarSelecoesAsync();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do logradouro expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouro: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        if (string.IsNullOrWhiteSpace(Logradouro.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = new string([.. Logradouro.Cep.Where(char.IsDigit)]);
        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CEP deve conter exatamente 8 dígitos numéricos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorCepAsync(apenasDigitos, cts.Token);
            if (logradouroData != null)
            {
                Logradouro = logradouroData;
                await SincronizarSelecoesAsync();
                LogradouroId = logradouroData.Id;
                IsEditMode = true;
                Title = "Editar Logradouro";
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP já cadastrado! Dados carregados para edição.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP não encontrado.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca do CEP expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLogradouroAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidateLogradouroAsync(Logradouro))
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Normaliza os dados antes de persistir
            Logradouro.Cep = new string([.. Logradouro.Cep.Where(char.IsDigit)]);
            Logradouro.Estado = Logradouro.Estado.Trim().ToUpperInvariant();
            Logradouro.Nome = Logradouro.Nome.Trim();
            Logradouro.Bairro = Logradouro.Bairro.Trim();
            Logradouro.Cidade = Logradouro.Cidade.Trim();
            Logradouro.Pais = string.IsNullOrWhiteSpace(Logradouro.Pais) ? "Brasil" : Logradouro.Pais.Trim();

            if (IsEditMode)
            {
                await _logradouroService.AtualizarAsync(Logradouro, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro atualizado com sucesso!", "OK");
            }
            else
            {
                await _logradouroService.AdicionarAsync(Logradouro, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro criado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do logradouro expirou. Verifique a conexão.", "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync("Regra de Negócio", ex.Message, "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync("Validação", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar logradouro: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /*
    Validação utilizando o padrão Notification Pattern, retornando uma lista de erros para o usuário, em vez de fail-fast exceptions.
    Isso permite que o usuário veja todos os problemas de uma vez e corrija-os antes de tentar salvar novamente.
    */
    private async Task<bool> ValidateLogradouroAsync(LogradouroDto logradouro)
    {
        var errors = new List<string>();

        // Validação do CEP
        if (string.IsNullOrWhiteSpace(logradouro.Cep))
        {
            errors.Add("• CEP é obrigatório.");
        }
        else
        {
            var cepDigitos = new string([.. logradouro.Cep.Where(char.IsDigit)]);
            if (cepDigitos.Length != 8)
            {
                errors.Add("• O CEP deve conter exatamente 8 dígitos numéricos.");
            }
        }

        // Validação do Logradouro / Nome
        if (string.IsNullOrWhiteSpace(logradouro.Nome))
        {
            errors.Add("• Logradouro / Rua é obrigatório.");
        }

        // Validação do Bairro
        if (string.IsNullOrWhiteSpace(logradouro.Bairro))
        {
            errors.Add("• Bairro é obrigatório.");
        }

        var brasil = string.Equals(logradouro.Pais?.Trim(), LocalidadesService.Brasil, StringComparison.CurrentCultureIgnoreCase);

        // Validação do Estado (UF)
        if (string.IsNullOrWhiteSpace(logradouro.Estado))
        {
            errors.Add(brasil ? "• Selecione o Estado (UF)." : "• Estado é obrigatório.");
        }
        else
        {
            var estadoLimpo = logradouro.Estado.Trim();
            if (estadoLimpo.Length != 2 || !estadoLimpo.All(char.IsLetter))
            {
                errors.Add("• O Estado deve conter uma sigla de 2 letras (ex: SC, SP).");
            }
            else if (brasil && !_localidades.UfValida(estadoLimpo))
            {
                errors.Add($"• \"{estadoLimpo}\" não é uma UF brasileira válida.");
            }
        }

        // Validação da Cidade
        if (string.IsNullOrWhiteSpace(logradouro.Cidade))
        {
            errors.Add(brasil ? "• Selecione a Cidade." : "• Cidade é obrigatória.");
        }
        else if (brasil)
        {
            // registros antigos podem trazer uma cidade fora da lista oficial: força a correção
            var cidadesDaUf = await _localidades.ObterCidadesAsync(logradouro.Estado);
            if (!cidadesDaUf.Any(c => string.Equals(c, logradouro.Cidade.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            {
                errors.Add($"• \"{logradouro.Cidade.Trim()}\" não é um município de {logradouro.Estado.Trim().ToUpperInvariant()}. Selecione a cidade na lista.");
            }
        }
        else if (!logradouro.Cidade.Trim().All(c => char.IsLetter(c) || c == ' ' || c == '-' || c == '\''))
        {
            errors.Add("• A Cidade deve conter apenas letras.");
        }

        // Validação do País
        if (string.IsNullOrWhiteSpace(logradouro.Pais))
        {
            errors.Add("• País é obrigatório.");
        }

        if (errors.Count > 0)
        {
            var mensagem = "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", errors);
            await Shell.Current.DisplayAlertAsync("Erros de Validação", mensagem, "OK");
            return false;
        }

        return true;
    }
}
