//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    // Fontes dos Pickers: o texto exibido vem do [Display] via EnumDisplayConverter
    public IEnumerable<AppColaboradorTipo> ColaboradorTipos { get; } = Enum.GetValues<AppColaboradorTipo>();
    public IEnumerable<AppColaboradorVinculo> ColaboradorVinculos { get; } = Enum.GetValues<AppColaboradorVinculo>();

    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;
    private readonly LogradouroModalService _logradouroModal;

    // OnAppearing pode voltar a disparar ao retornar da câmera/galeria (Android);
    // sem esta trava, um cadastro novo seria limpo no meio do preenchimento
    private bool _inicializado;

    private ColaboradorDto _colaborador = CriarColaboradorPadrao();
    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set
        {
            if (SetProperty(ref _colaborador, value))
                NotificarStatus();
        }
    }

    private int _colaboradorId;
    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _confirmarSenha = string.Empty;
    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    // Salário digitado como texto (pt-BR): o Entry não converte decimal com vírgula de forma confiável
    private string _salarioTexto = string.Empty;
    public string SalarioTexto
    {
        get => _salarioTexto;
        set => SetProperty(ref _salarioTexto, value);
    }

    public DateTime Hoje => DateTime.Today;

    public bool HasEnderecoVinculado => Colaborador?.Endereco != null && Colaborador.Endereco.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;
    public bool HasFoto => Colaborador?.Foto?.Conteudo is { Length: > 0 };
    public bool HasNoFoto => !HasFoto;

    // DataDemissao é somente leitura (operação de domínio Colaborador.Desligar): apenas exibida
    public bool HasDataDemissao => Colaborador?.DataDemissao != null;

    public ColaboradorViewModel(IColaboradorService colaboradorService, ILogradouroService logradouroService, LogradouroModalService logradouroModal)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;
        _logradouroModal = logradouroModal;
        Title = "Detalhes do Colaborador";
    }

    public async Task InitializeAsync()
    {
        if (_inicializado)
            return;
        _inicializado = true;

        ConfirmarSenha = string.Empty;

        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = "Editar Colaborador";
            await LoadColaboradorAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Colaborador";
            Colaborador = CriarColaboradorPadrao();
            SalarioTexto = string.Empty;
        }
    }

    private void NotificarStatus()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
        OnPropertyChanged(nameof(HasFoto));
        OnPropertyChanged(nameof(HasNoFoto));
        OnPropertyChanged(nameof(HasDataDemissao));
    }

    private static LogradouroDto CriarEnderecoVazio() => new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    private static ColaboradorDto CriarColaboradorPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Endereco = CriarEnderecoVazio(),
        Numero = string.Empty,
        Complemento = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Atendente,
        Vinculo = AppColaboradorVinculo.CLT,
        Salario = 0
    };

    // Prepara um colaborador vindo do banco para edição na tela
    private void CarregarNaTela(ColaboradorDto colaboradorData)
    {
        // Garante que o Endereço não seja nulo para evitar erros de bind
        colaboradorData.Endereco ??= CriarEnderecoVazio();

        // A senha não é retornada por segurança; o Service exige que seja informada de novo ao salvar
        colaboradorData.Senha = string.Empty;
        ConfirmarSenha = string.Empty;

        SalarioTexto = colaboradorData.Salario > 0 ? colaboradorData.Salario.ToString("N2", PtBr) : string.Empty;
        Colaborador = colaboradorData;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task LoadColaboradorAsync()
    {
        if (ColaboradorId <= 0)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradorData = await _colaboradorService.ObterPorIdAsync(ColaboradorId, cts.Token);
            if (colaboradorData != null)
            {
                CarregarNaTela(colaboradorData);
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Colaborador não encontrado.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do colaborador expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Colaborador.Cpf))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = PessoaValidacao.ApenasDigitos(Colaborador.Cpf);
        if (apenasDigitos.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CPF deve conter exatamente 11 dígitos numéricos.", "OK");
            return;
        }
        if (!PessoaValidacao.CpfValido(apenasDigitos))
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CPF informado é inválido (dígitos verificadores não conferem).", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradorData = await _colaboradorService.ObterPorCpfAsync(apenasDigitos, cts.Token);
            if (colaboradorData != null)
            {
                CarregarNaTela(colaboradorData);
                ColaboradorId = colaboradorData.Id;
                IsEditMode = true;
                Title = "Editar Colaborador";
                await Shell.Current.DisplayAlertAsync("Aviso", "Colaborador já cadastrado! Dados carregados para edição.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CPF não encontrado. Continue o preenchimento para cadastrar.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca por CPF expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        if (Colaborador.Endereco == null || string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = PessoaValidacao.ApenasDigitos(Colaborador.Endereco.Cep);
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
                Colaborador.Endereco = logradouroData;
                OnPropertyChanged(nameof(Colaborador));
                NotificarStatus();
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP encontrado! Endereço preenchido automaticamente.", "OK");
            }
            else
            {
                await OferecerCadastroLogradouroAsync(apenasDigitos, "CEP não encontrado no cadastro de logradouros.");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de CEP expirou. Verifique a conexão com o banco.", "OK");
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
    private async Task SaveColaboradorAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidateColaboradorAsync(Colaborador, ConfirmarSenha, SalarioTexto))
            return;

        // o Service troca a senha pelo hash no próprio DTO: se a gravação falhar, a tela volta a ter a senha digitada
        var senhaDigitada = Colaborador.Senha;
        var salvo = false;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Se o CEP foi digitado mas o logradouro ainda não foi vinculado, tenta buscar pelo CEP
            if (Colaborador.Endereco != null && Colaborador.Endereco.Id <= 0 && !string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
            {
                var cepLimpo = PessoaValidacao.ApenasDigitos(Colaborador.Endereco.Cep);
                if (cepLimpo.Length != 8)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Informe um CEP válido com 8 dígitos para o endereço.", "OK");
                    return;
                }

                var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                if (logradouroData == null)
                {
                    await OferecerCadastroLogradouroAsync(cepLimpo, "O CEP informado não está cadastrado como Logradouro.");
                    return;
                }
                Colaborador.Endereco = logradouroData;
                NotificarStatus();
            }

            // Normaliza os dados antes de persistir
            PessoaValidacao.Normalizar(Colaborador);
            TryParseSalario(SalarioTexto, out var salario);
            Colaborador.Salario = salario;

            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador atualizado com sucesso!", "OK");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador criado com sucesso!", "OK");
            }

            salvo = true;
            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do colaborador expirou. Verifique a conexão.", "OK");
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
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            if (!salvo && Colaborador.Senha != senhaDigitada)
            {
                Colaborador.Senha = senhaDigitada;
                OnPropertyChanged(nameof(Colaborador));
            }
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AdicionarLogradouroAsync()
    {
        // leva o CEP já digitado para o modal, se estiver completo
        var cep = PessoaValidacao.ApenasDigitos(Colaborador.Endereco?.Cep);
        await CadastrarLogradouroAsync(cep.Length == 8 ? cep : null);
    }

    // CEP sem logradouro: oferece cadastrá-lo ali mesmo, num modal
    private async Task OferecerCadastroLogradouroAsync(string cep, string motivo)
    {
        var cadastrar = await Shell.Current.DisplayAlertAsync("Logradouro não cadastrado",
            $"{motivo}\n\nDeseja cadastrá-lo agora?", "Cadastrar", "Agora não");
        if (cadastrar)
            await CadastrarLogradouroAsync(cep);
    }

    private async Task CadastrarLogradouroAsync(string? cep)
    {
        try
        {
            IsBusy = false;
            var novo = await _logradouroModal.CadastrarAsync(cep);
            if (novo is null)
                return;

            Colaborador.Endereco = novo;
            OnPropertyChanged(nameof(Colaborador));
            NotificarStatus();
            await Shell.Current.DisplayAlertAsync("Endereço", "Logradouro cadastrado e vinculado ao colaborador. Confira os dados e clique em Salvar.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao abrir o cadastro de logradouro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        var foto = await FotoHelper.SelecionarAsync();
        if (foto is null)
            return;

        Colaborador.Foto = foto;
        OnPropertyChanged(nameof(Colaborador));
        NotificarStatus();
    }

    /*
    Validação utilizando o padrão Notification Pattern, retornando uma lista de erros para o usuário, em vez de fail-fast exceptions.
    Isso permite que o usuário veja todos os problemas de uma vez e corrija-os antes de tentar salvar novamente.
    */
    private static async Task<bool> ValidateColaboradorAsync(ColaboradorDto colaborador, string confirmarSenha, string salarioTexto)
    {
        var errors = new List<string>();

        // Nome, CPF, nascimento (mínimo 12 anos), telefone, e-mail, senha, foto e endereço
        PessoaValidacao.ValidarDadosPessoais(colaborador, confirmarSenha, idadeMinima: 12, rotulo: "colaborador", errors);

        if (colaborador.DataAdmissao == default)
            errors.Add("• Data de admissão é obrigatória.");
        else if (colaborador.DataAdmissao > DateOnly.FromDateTime(DateTime.Today))
            errors.Add("• Data de admissão não pode ser maior que a data atual.");

        if (string.IsNullOrWhiteSpace(salarioTexto))
            errors.Add("• Salário é obrigatório.");
        else if (!TryParseSalario(salarioTexto, out var salario))
            errors.Add("• Salário inválido. Use o formato 2500 ou 2.500,50.");
        else if (salario <= 0)
            errors.Add("• Salário deve ser maior que zero.");

        // Regra de Vínculo de Administrador: Administrador precisa ser CLT (mesma regra do domínio)
        if (colaborador.Tipo == AppColaboradorTipo.Administrador && colaborador.Vinculo != AppColaboradorVinculo.CLT)
            errors.Add($"• Colaborador com tipo {AppColaboradorTipo.Administrador.GetDisplayName()} deve ter vínculo {AppColaboradorVinculo.CLT.GetDisplayName()}.");

        return await PessoaValidacao.ExibirErrosAsync(errors);
    }

    /// <summary>
    /// Lê o salário em pt-BR ("2500", "2500,50", "2.500,50", com ou sem "R$").
    /// "2500.50" também é aceito: ponto seguido de 1 ou 2 dígitos no fim é tratado como decimal.
    /// </summary>
    private static bool TryParseSalario(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var t = texto.Replace("R$", string.Empty).Trim();

        var ultimoPonto = t.LastIndexOf('.');
        if (!t.Contains(',') && t.Count(c => c == '.') == 1 && t.Length - ultimoPonto - 1 is 1 or 2)
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);

        return decimal.TryParse(t, NumberStyles.Number, PtBr, out valor);
    }
}
