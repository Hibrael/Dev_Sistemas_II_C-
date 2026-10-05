using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    private readonly ILogradouroService _logradouroService;

    // seleções atuais da tela (substituem os antigos Pickers)
    private string _temaSelecionado = "system";
    private AppDatabaseType? _tipoBancoSelecionado;

    public ConfigPage(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AtualizarStatusAsync();
    }

    #region Status
    // Preenche os cartões do topo: conexão (testada de verdade), gerenciador e tema salvos
    private async Task AtualizarStatusAsync()
    {
        StatusGerenciadorLabel.Text = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString()) switch
        {
            nameof(AppDatabaseType.SqlServer) => "SQL Server",
            nameof(AppDatabaseType.MySql) => "MySQL",
            _ => "SQLite"
        };
        StatusTemaLabel.Text = NomeTema(Preferences.Get("Tema", "system"));

        StatusConexaoLabel.Text = "Verificando...";
        StatusConexaoLabel.ClearValue(Label.TextColorProperty);
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await _logradouroService.ObterTodosAsync(cts.Token);
            StatusConexaoLabel.Text = "Conectado";
            StatusConexaoLabel.TextColor = Cor("Sucesso", Colors.LimeGreen);
        }
        catch (Exception)
        {
            StatusConexaoLabel.Text = "Sem conexão";
            StatusConexaoLabel.TextColor = Color.FromArgb("#F59E0B");
        }
    }

    // lê uma cor dos recursos do app (Colors.xaml), com valor reserva caso a chave não exista
    private static Color Cor(string chave, Color reserva) =>
        Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(chave, out var valor) == true && valor is Color cor ? cor : reserva;

    private static string NomeTema(string tema) => tema switch { "light" => "Claro", "dark" => "Escuro", _ => "Sistema" };
    #endregion

    #region Tema
    private void CarregarTema()
    {
        SelecionarTema(Preferences.Get("Tema", "system"));
    }

    private void OnTemaTileTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string tema)
            SelecionarTema(tema);
    }

    // destaca o tile escolhido com a borda azul; os demais voltam ao estilo padrão
    private void SelecionarTema(string tema)
    {
        _temaSelecionado = tema is "light" or "dark" ? tema : "system";
        var destaque = Cor("AzulClaro", Colors.DodgerBlue);
        foreach (var (tile, valor) in new[] { (TemaClaroTile, "light"), (TemaEscuroTile, "dark"), (TemaSistemaTile, "system") })
        {
            if (valor == _temaSelecionado)
                tile.Stroke = destaque;
            else
                tile.ClearValue(Border.StrokeProperty);
        }
    }

    private async void OnAplicarTemaClicked(object? sender, EventArgs e)
    {
        string selectedTheme = _temaSelecionado;
        Preferences.Set("Tema", selectedTheme);

        // Disparar mensagem para uso na recarga dinâmica
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));

        await DisplayAlertAsync("Sucesso", "Tema aplicado com sucesso!", "OK");

        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    #region Banco de Dados
    private void CarregarBanco()
    {
        var bancoAtual = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        SelecionarTipoBanco(Enum.TryParse<AppDatabaseType>(bancoAtual, out var tipo) ? tipo : AppDatabaseType.Sqlite);
    }

    private void OnTipoBancoClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string valor } && Enum.TryParse<AppDatabaseType>(valor, out var tipo))
            SelecionarTipoBanco(tipo);
    }

    // marca o segmento escolhido (fundo azul) e ajusta os campos da tela
    private void SelecionarTipoBanco(AppDatabaseType tipo)
    {
        _tipoBancoSelecionado = tipo;
        var azul = Cor("AzulEscuro", Colors.MediumBlue);
        foreach (var (botao, valor) in new[] { (SegSqlite, AppDatabaseType.Sqlite), (SegSqlServer, AppDatabaseType.SqlServer), (SegMySql, AppDatabaseType.MySql) })
        {
            if (valor == tipo)
            {
                botao.BackgroundColor = azul;
                botao.TextColor = Colors.White;
            }
            else
            {
                botao.ClearValue(VisualElement.BackgroundColorProperty);
                botao.ClearValue(Button.TextColorProperty);
            }
        }
        AtualizarInterfacePorTipoBanco();
    }

    private void AtualizarInterfacePorTipoBanco()
    {
        if (_tipoBancoSelecionado is not AppDatabaseType selectedType)
        {
            return;
        }

        switch (selectedType)
        {
            case AppDatabaseType.Sqlite:
                SqliteInfoCard.IsVisible = true;
                SqliteContainer.IsVisible = true;
                ServidorBancoGrid.IsVisible = false;
                CredenciaisGrid.IsVisible = false;

                ComplementoLabel.Text = "Complemento (ex: Default Timeout=5;)";
                ComplementoEntry.Placeholder = "Default Timeout=5;";

                var defaultSqlitePath = DeviceInfo.Platform == DevicePlatform.WinUI
                    ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                    : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");

                SqliteCaminhoEntry.Text = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultSqlitePath));
                ComplementoEntry.Text = Preferences.Get("Sqlite_Complemento", "Default Timeout=5;");
                break;

            case AppDatabaseType.SqlServer:
                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;
                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;

                ServidorEntry.Placeholder = "Ex: 172.24.32.1 ou localhost";
                BancoEntry.Placeholder = "Ex: db_academia_do_ze";
                UsuarioEntry.Placeholder = "Ex: sa";
                ComplementoLabel.Text = "Complemento (SSL / Timeout / Criptografia)";
                ComplementoEntry.Placeholder = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";

                ServidorEntry.Text = Preferences.Get("SqlServer_Servidor", Preferences.Get("Servidor", "172.24.32.1"));
                BancoEntry.Text = Preferences.Get("SqlServer_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
                UsuarioEntry.Text = Preferences.Get("SqlServer_Usuario", Preferences.Get("Usuario", "sa"));
                SenhaEntry.Text = Preferences.Get("SqlServer_Senha", Preferences.Get("Senha", "abcBolinhas12345"));
                ComplementoEntry.Text = Preferences.Get("SqlServer_Complemento", "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;");
                break;

            case AppDatabaseType.MySql:
                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;
                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;

                ServidorEntry.Placeholder = "Ex: 10.30.21.16 ou localhost";
                BancoEntry.Placeholder = "Ex: db_academia_do_ze";
                UsuarioEntry.Placeholder = "Ex: root";
                ComplementoLabel.Text = "Complemento (Porta / Timeout)";
                ComplementoEntry.Placeholder = "Connection Timeout=5;Default Command Timeout=30;";

                ServidorEntry.Text = Preferences.Get("MySql_Servidor", Preferences.Get("Servidor", "10.30.21.16"));
                BancoEntry.Text = Preferences.Get("MySql_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
                UsuarioEntry.Text = Preferences.Get("MySql_Usuario", Preferences.Get("Usuario", "root"));
                SenhaEntry.Text = Preferences.Get("MySql_Senha", Preferences.Get("Senha", "abcBolinhas12345"));
                ComplementoEntry.Text = Preferences.Get("MySql_Complemento", "Connection Timeout=5;Default Command Timeout=30;");
                break;
        }
    }

    private async void OnAplicarBdClicked(object? sender, EventArgs e)
    {
        if (_tipoBancoSelecionado is not AppDatabaseType selectedType)
        {
            await DisplayAlertAsync("Aviso", "Selecione um tipo de banco de dados válido.", "OK");
            return;
        }

        if (selectedType == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o caminho do arquivo do banco SQLite.", "OK");
                return;
            }

            var caminho = SqliteCaminhoEntry.Text.Trim();
            var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;

            Preferences.Set("Sqlite_Caminho", caminho);
            Preferences.Set("Sqlite_Complemento", complemento);

            // Chaves de compatibilidade
            Preferences.Set("SqliteCaminho", caminho);
            Preferences.Set("Complemento", complemento);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ServidorEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o servidor do banco de dados.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(BancoEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o nome do banco de dados.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(UsuarioEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o usuário do banco de dados.", "OK");
                return;
            }

            var servidor = ServidorEntry.Text.Trim();
            var banco = BancoEntry.Text.Trim();
            var usuario = UsuarioEntry.Text.Trim();
            var senha = SenhaEntry.Text ?? string.Empty;
            var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;

            var prefix = selectedType == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
            Preferences.Set($"{prefix}_Servidor", servidor);
            Preferences.Set($"{prefix}_Banco", banco);
            Preferences.Set($"{prefix}_Usuario", usuario);
            Preferences.Set($"{prefix}_Senha", senha);
            Preferences.Set($"{prefix}_Complemento", complemento);

            // Chaves de compatibilidade
            Preferences.Set("Servidor", servidor);
            Preferences.Set("Banco", banco);
            Preferences.Set("Usuario", usuario);
            Preferences.Set("Senha", senha);
            Preferences.Set("Complemento", complemento);
        }

        Preferences.Set("DatabaseType", selectedType.ToString());

        // Disparar a mensagem para recarga dinâmica (processada pelo ConfigurationHelper)
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));

        await DisplayAlertAsync("Sucesso", $"Configurações do banco de dados ({selectedType}) aplicadas com sucesso!", "OK");

        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    // Ao fechar a página, chama WeakReferenceMessenger.Default.UnregisterAll(this); para evitar vazamentos de memória - memory leaks
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Desinscreve o mensageiro para evitar memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
