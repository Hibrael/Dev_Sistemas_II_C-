//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    // Abaixo desta idade o laudo médico é obrigatório (o cadastro de Aluno já exige no mínimo 14 anos)
    private const int IdadeLaudoObrigatorio = 16;
    private const string RemoverLaudoOpcao = "Remover laudo";

    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    // OnAppearing pode voltar a disparar ao retornar da câmera/galeria (Android);
    // sem esta trava, o formulário seria recarregado no meio do preenchimento
    private bool _inicializado;

    // Aluno da matrícula ao abrir a edição: trocar de aluno exige que o novo não tenha outra matrícula ativa
    // (quando a matrícula aberta continuar vigente; ver FicaraAtiva)
    private int _alunoIdOriginal;

    // Flag de suspensão: evita gravar no DTO enquanto os CheckBoxes são sincronizados a partir dele
    private bool _suspendSelectionSync;

    public IEnumerable<AppMatriculaPlano> PlanoTipos { get; } = Enum.GetValues<AppMatriculaPlano>();

    // Itens para múltipla seleção (CheckBoxes) - restrições médicas - flags
    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = [];

    private MatriculaDto _matricula = CriarMatriculaPadrao();
    public MatriculaDto Matricula
    {
        get => _matricula;
        set
        {
            // quando trocar a matrícula, atualiza a seleção dos CheckBoxes
            if (SetProperty(ref _matricula, value))
                SyncSelectionFromFlags();
        }
    }

    private int _matriculaId;
    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    // CPF digitado para a busca. Fica separado do aluno vinculado para que editar o campo
    // não altere os dados do aluno já selecionado
    private string _cpfBusca = string.Empty;
    public string CpfBusca
    {
        get => _cpfBusca;
        set => SetProperty(ref _cpfBusca, value);
    }

    // Término exibido na tela: recalculado a cada troca de plano ou de início
    public DateOnly DataTermino => Matricula.DataFim;

    public bool HasAlunoVinculado => Matricula.AlunoMatricula is { Id: > 0 };
    public bool HasNoAlunoVinculado => !HasAlunoVinculado;
    public bool IsMenor16 => Matricula.AlunoMatricula is { } aluno && ExigeLaudoPorIdade(aluno.DataNascimento);
    public bool HasRestricoes => Matricula.RestricoesMedicas != AppMatriculaRestricoes.Nenhuma;
    public bool ExigeLaudo => IsMenor16 || HasRestricoes;
    public bool HasLaudo => Matricula.LaudoMedico?.Conteudo is { Length: > 0 };
    public bool HasNoLaudo => !HasLaudo;
    public bool LaudoPendente => ExigeLaudo && !HasLaudo;
    public bool LaudoOpcional => !ExigeLaudo && !HasLaudo;

    public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = "Detalhes da Matrícula";
        BuildRestricoesMulti();
    }

    public async Task InitializeAsync()
    {
        if (_inicializado)
            return;
        _inicializado = true;

        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar Matrícula";
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova Matrícula";
            CpfBusca = string.Empty;
            Matricula = CriarMatriculaPadrao();
            SyncSelectionFromFlags();
        }

        NotificarPropriedadesVisuais();
    }

    public void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
        OnPropertyChanged(nameof(IsMenor16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(HasNoLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
        OnPropertyChanged(nameof(LaudoOpcional));
    }

    /// <summary>
    /// Calcula a data final a partir do plano e da data de início, igual ao domínio
    /// (que recalcula ao salvar). Chamado pela página quando o plano ou o início mudam.
    /// </summary>
    public void CalcularDataFim()
    {
        var inicio = Matricula.DataInicio;
        Matricula.DataFim = Matricula.Plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };
        OnPropertyChanged(nameof(DataTermino));
    }

    private static MatriculaDto CriarMatriculaPadrao() => new()
    {
        AlunoId = 0,
        AlunoMatricula = null,
        Plano = AppMatriculaPlano.Anual,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        DataFim = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
        Objetivo = string.Empty,
        RestricoesMedicas = AppMatriculaRestricoes.Nenhuma,
        ObservacoesRestricoes = string.Empty,
        LaudoMedico = null
    };

    // Regra do material: alunos menores de 16 anos devem apresentar laudo médico
    private static bool ExigeLaudoPorIdade(DateOnly nascimento)
        => nascimento != default && nascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-IdadeLaudoObrigatorio));

    #region enum Flags <-> seleção múltipla (CheckBoxes)

    // Monta a lista de checkboxes a partir do enum (ignora Nenhuma)
    private void BuildRestricoesMulti()
    {
        RestricoesMulti.Clear();
        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.Nenhuma)
                continue;

            var item = new RestricaoSelectable
            {
                Value = value,
                IsSelected = Matricula.RestricoesMedicas.HasFlag(value)
            };
            item.PropertyChanged += RestricaoItem_PropertyChanged;
            RestricoesMulti.Add(item);
        }
    }

    // Atualiza o DTO quando o usuário marca/desmarca
    private void RestricaoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RestricaoSelectable.IsSelected))
        {
            if (_suspendSelectionSync)
                return; // evita write-back durante a sincronização

            RecomputeRestricoesFlags();
        }
    }

    // Recalcula o bitmask a partir das seleções
    private void RecomputeRestricoesFlags()
    {
        var flags = AppMatriculaRestricoes.Nenhuma;
        foreach (var item in RestricoesMulti)
        {
            if (item.IsSelected)
                flags |= item.Value;
        }

        Matricula.RestricoesMedicas = flags;
        NotificarPropriedadesVisuais();
    }

    // Sincroniza a seleção visual a partir do DTO (quando carrega ou substitui a matrícula)
    private void SyncSelectionFromFlags()
    {
        _suspendSelectionSync = true;
        try
        {
            foreach (var item in RestricoesMulti)
            {
                item.IsSelected = Matricula.RestricoesMedicas.HasFlag(item.Value);
            }
        }
        finally
        {
            _suspendSelectionSync = false;
        }

        NotificarPropriedadesVisuais();
    }

    #endregion

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId, cts.Token);
            if (matriculaData != null)
            {
                _alunoIdOriginal = matriculaData.AlunoId;
                CpfBusca = matriculaData.AlunoMatricula?.Cpf ?? string.Empty;
                Matricula = matriculaData;
                SyncSelectionFromFlags();
            }
            else
            {
                // sem a matrícula não há o que editar: volta para a lista em vez de deixar um formulário vazio
                await Shell.Current.DisplayAlertAsync("Aviso", "Matrícula não encontrada.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento da matrícula expirou. Verifique a conexão.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(CpfBusca))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = PessoaValidacao.ApenasDigitos(CpfBusca);
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
            if (alunoData == null)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno não encontrado. Confira o CPF ou cadastre o aluno no menu Alunos.", "OK");
                return;
            }

            // Regra: não permitir nova matrícula se o aluno ainda tiver matrícula ativa
            // (na edição, só conflita se a matrícula aberta continuar vigente, como no Salvar)
            if (alunoData.Id != _alunoIdOriginal && FicaraAtiva)
            {
                var ativa = await ObterOutraMatriculaAtivaAsync(alunoData.Id, cts.Token);
                if (ativa != null)
                {
                    await Shell.Current.DisplayAlertAsync("Matrícula Ativa", MensagemMatriculaAtiva(alunoData.Nome, ativa), "OK");
                    return;
                }
            }

            Matricula.AlunoId = alunoData.Id;
            Matricula.AlunoMatricula = alunoData;
            CpfBusca = alunoData.Cpf;
            OnPropertyChanged(nameof(Matricula));
            NotificarPropriedadesVisuais();
            await Shell.Current.DisplayAlertAsync("Aviso", "Aluno encontrado! Dados preenchidos automaticamente.", "OK");
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
    private async Task SaveMatriculaAsync()
    {
        if (IsBusy)
            return;

        // garante que o DTO está com os flags e a data final corretos
        RecomputeRestricoesFlags();
        CalcularDataFim();

        if (!await ValidateMatriculaAsync(Matricula, CpfBusca))
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Recarrega o aluno vinculado: confirma que ainda existe e atualiza a idade usada na regra do laudo
            var alunoData = await _alunoService.ObterPorIdAsync(Matricula.AlunoId, cts.Token);
            if (alunoData == null)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "O aluno informado não foi encontrado. Busque um aluno válido pelo CPF.", "OK");
                return;
            }
            Matricula.AlunoMatricula = alunoData;
            NotificarPropriedadesVisuais();

            // Regra: o aluno não pode ficar com duas matrículas ativas ao mesmo tempo
            // (no cadastro o Service também barra; na edição, só a matrícula que continuar vigente conflita)
            var outraAtiva = await ObterOutraMatriculaAtivaAsync(alunoData.Id, cts.Token);
            if (outraAtiva != null && FicaraAtiva)
            {
                await Shell.Current.DisplayAlertAsync("Regra de Negócio", MensagemMatriculaAtiva(alunoData.Nome, outraAtiva), "OK");
                return;
            }

            // Revalida caso os dados atualizados do aluno (idade) exijam laudo
            if (!await ValidateMatriculaAsync(Matricula, CpfBusca))
                return;

            // Normaliza os textos antes de persistir
            Matricula.Objetivo = Matricula.Objetivo.Trim();
            Matricula.ObservacoesRestricoes = string.IsNullOrWhiteSpace(Matricula.ObservacoesRestricoes) ? null : Matricula.ObservacoesRestricoes.Trim();

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula atualizada com sucesso!", "OK");
            }
            else
            {
                await _matriculaService.AdicionarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula criada com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação da matrícula expirou. Verifique a conexão.", "OK");
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
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Seleciona a imagem do laudo médico: galeria ou câmera, pelo mesmo fluxo da foto do cadastro
    /// (FotoHelper). Diferente da foto, o laudo só é exigido em alguns casos, por isso pode ser removido.
    /// </summary>
    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        string[] opcoes = HasLaudo
            ? [FotoHelper.Galeria, FotoHelper.Camera, RemoverLaudoOpcao]
            : [FotoHelper.Galeria, FotoHelper.Camera];

        var escolha = await Shell.Current.DisplayActionSheetAsync("Origem do Laudo Médico", "Cancelar", null, opcoes);
        if (escolha == RemoverLaudoOpcao)
        {
            RemoverLaudo();
            return;
        }

        var laudo = await FotoHelper.ObterAsync(escolha, "Selecione o laudo médico (.jpg ou .png)", "Foto do laudo médico");
        if (laudo is null)
            return;

        Matricula.LaudoMedico = laudo;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    private void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    // Uma nova matrícula sempre conta como ativa (o Service barra); na edição, só se continuar vigente
    private bool FicaraAtiva => !IsEditMode || Matricula.DataFim >= DateOnly.FromDateTime(DateTime.Today);

    // Matrícula ativa do aluno que não seja a que está aberta na tela (Id 0 no cadastro)
    private async Task<MatriculaDto?> ObterOutraMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken)
    {
        var ativas = await _matriculaService.ObterAtivasAsync(alunoId, cancellationToken);
        return ativas.FirstOrDefault(m => m.Id != Matricula.Id);
    }

    private static string MensagemMatriculaAtiva(string nomeAluno, MatriculaDto ativa)
        => $"{nomeAluno} já possui a matrícula #{ativa.Id} ativa, vigente até {ativa.DataFim:dd/MM/yyyy}. "
           + "Não é permitida uma nova matrícula enquanto houver outra ativa.";

    /*
    Validação utilizando o padrão Notification Pattern, retornando uma lista de erros para o usuário, em vez de fail-fast exceptions.
    Isso permite que o usuário veja todos os problemas de uma vez e corrija-os antes de tentar salvar novamente.
    */
    private static async Task<bool> ValidateMatriculaAsync(MatriculaDto matricula, string cpfBusca)
    {
        var errors = new List<string>();

        // Aluno: vinculado pela busca, e o CPF digitado precisa ser o do aluno vinculado
        if (matricula.AlunoId <= 0 || matricula.AlunoMatricula == null)
            errors.Add("• Aluno é obrigatório. Informe o CPF e clique em Buscar.");
        else if (PessoaValidacao.ApenasDigitos(cpfBusca) != PessoaValidacao.ApenasDigitos(matricula.AlunoMatricula.Cpf))
            errors.Add("• O CPF digitado não é o do aluno vinculado. Clique em Buscar para vincular o aluno desse CPF.");

        // Plano
        if (!Enum.IsDefined(matricula.Plano))
            errors.Add("• Plano da matrícula é inválido.");

        // Data de Início
        if (matricula.DataInicio == default)
            errors.Add("• Data de início é obrigatória.");

        // Objetivo
        if (string.IsNullOrWhiteSpace(matricula.Objetivo))
            errors.Add("• Objetivo da matrícula é obrigatório.");

        // Laudo Médico: mesmo limite de tamanho do Value Object Arquivo
        var possuiLaudo = matricula.LaudoMedico?.Conteudo is { Length: > 0 };
        if (possuiLaudo && matricula.LaudoMedico!.Conteudo.LongLength > PessoaValidacao.FotoTamanhoMaximoBytes)
            errors.Add("• O laudo médico deve ter no máximo 5 MB.");

        // Laudo Médico para menor de 16 anos
        if (matricula.AlunoMatricula != null && ExigeLaudoPorIdade(matricula.AlunoMatricula.DataNascimento) && !possuiLaudo)
            errors.Add("• Alunos menores de 16 anos devem obrigatoriamente apresentar laudo médico.");

        // Laudo Médico para restrições médicas
        if (matricula.RestricoesMedicas != AppMatriculaRestricoes.Nenhuma && !possuiLaudo)
            errors.Add("• Alunos com restrições médicas cadastradas devem obrigatoriamente apresentar laudo médico.");

        return await PessoaValidacao.ExibirErrosAsync(errors);
    }
}
