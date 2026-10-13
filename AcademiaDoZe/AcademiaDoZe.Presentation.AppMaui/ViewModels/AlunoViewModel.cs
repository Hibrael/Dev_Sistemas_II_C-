//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    // OnAppearing pode voltar a disparar ao retornar da câmera/galeria (Android);
    // sem esta trava, um cadastro novo seria limpo no meio do preenchimento
    private bool _inicializado;

    private AlunoDto _aluno = CriarAlunoPadrao();
    public AlunoDto Aluno
    {
        get => _aluno;
        set
        {
            if (SetProperty(ref _aluno, value))
                NotificarStatus();
        }
    }

    private int _alunoId;
    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
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

    public DateTime Hoje => DateTime.Today;

    public bool HasEnderecoVinculado => Aluno?.Endereco != null && Aluno.Endereco.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;
    public bool HasFoto => Aluno?.Foto?.Conteudo is { Length: > 0 };
    public bool HasNoFoto => !HasFoto;

    public AlunoViewModel(IAlunoService alunoService, ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = "Detalhes do Aluno";
    }

    public async Task InitializeAsync()
    {
        if (_inicializado)
            return;
        _inicializado = true;

        ConfirmarSenha = string.Empty;

        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar Aluno";
            await LoadAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Aluno";
            Aluno = CriarAlunoPadrao();
        }
    }

    private void NotificarStatus()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
        OnPropertyChanged(nameof(HasFoto));
        OnPropertyChanged(nameof(HasNoFoto));
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

    private static AlunoDto CriarAlunoPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Endereco = CriarEnderecoVazio(),
        Numero = string.Empty,
        Complemento = string.Empty
    };

    // Prepara um aluno vindo do banco para edição na tela
    private void CarregarNaTela(AlunoDto alunoData)
    {
        // Garante que o Endereço não seja nulo para evitar erros de bind
        alunoData.Endereco ??= CriarEnderecoVazio();

        // A senha não é retornada por segurança; o Service exige que seja informada de novo ao salvar
        alunoData.Senha = string.Empty;
        ConfirmarSenha = string.Empty;

        Aluno = alunoData;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task LoadAlunoAsync()
    {
        if (AlunoId <= 0)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunoData = await _alunoService.ObterPorIdAsync(AlunoId, cts.Token);
            if (alunoData != null)
            {
                CarregarNaTela(alunoData);
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno não encontrado.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Aluno.Cpf))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = PessoaValidacao.ApenasDigitos(Aluno.Cpf);
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
            var alunoData = await _alunoService.ObterPorCpfAsync(apenasDigitos, cts.Token);
            if (alunoData != null)
            {
                CarregarNaTela(alunoData);
                AlunoId = alunoData.Id;
                IsEditMode = true;
                Title = "Editar Aluno";
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno já cadastrado! Dados carregados para edição.", "OK");
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
        if (Aluno.Endereco == null || string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = PessoaValidacao.ApenasDigitos(Aluno.Endereco.Cep);
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
                Aluno.Endereco = logradouroData;
                OnPropertyChanged(nameof(Aluno));
                NotificarStatus();
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP encontrado! Endereço preenchido automaticamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP não encontrado no cadastro de logradouros. Cadastre o logradouro primeiro.", "OK");
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
    private async Task SaveAlunoAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidateAlunoAsync(Aluno, ConfirmarSenha))
            return;

        // o Service troca a senha pelo hash no próprio DTO: se a gravação falhar, a tela volta a ter a senha digitada
        var senhaDigitada = Aluno.Senha;
        var salvo = false;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Se o CEP foi digitado mas o logradouro ainda não foi vinculado, tenta buscar pelo CEP
            if (Aluno.Endereco != null && Aluno.Endereco.Id <= 0 && !string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
            {
                var cepLimpo = PessoaValidacao.ApenasDigitos(Aluno.Endereco.Cep);
                if (cepLimpo.Length != 8)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Informe um CEP válido com 8 dígitos para o endereço.", "OK");
                    return;
                }

                var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                if (logradouroData == null)
                {
                    await Shell.Current.DisplayAlertAsync("Endereço Obrigatório", "O CEP informado não está cadastrado como Logradouro. Cadastre-o previamente.", "OK");
                    return;
                }
                Aluno.Endereco = logradouroData;
                NotificarStatus();
            }

            // Normaliza os dados antes de persistir
            PessoaValidacao.Normalizar(Aluno);

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno atualizado com sucesso!", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno criado com sucesso!", "OK");
            }

            salvo = true;
            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do aluno expirou. Verifique a conexão.", "OK");
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
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar aluno: {ex.Message}", "OK");
        }
        finally
        {
            if (!salvo && Aluno.Senha != senhaDigitada)
            {
                Aluno.Senha = senhaDigitada;
                OnPropertyChanged(nameof(Aluno));
            }
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        var foto = await FotoHelper.SelecionarAsync();
        if (foto is null)
            return;

        Aluno.Foto = foto;
        OnPropertyChanged(nameof(Aluno));
        NotificarStatus();
    }

    /*
    Validação utilizando o padrão Notification Pattern, retornando uma lista de erros para o usuário, em vez de fail-fast exceptions.
    Isso permite que o usuário veja todos os problemas de uma vez e corrija-os antes de tentar salvar novamente.
    */
    private static async Task<bool> ValidateAlunoAsync(AlunoDto aluno, string confirmarSenha)
    {
        var errors = new List<string>();

        // Nome, CPF, nascimento (mínimo 14 anos), telefone, e-mail, senha, foto e endereço
        PessoaValidacao.ValidarDadosPessoais(aluno, confirmarSenha, idadeMinima: 14, rotulo: "aluno", errors);

        return await PessoaValidacao.ExibirErrosAsync(errors);
    }
}
