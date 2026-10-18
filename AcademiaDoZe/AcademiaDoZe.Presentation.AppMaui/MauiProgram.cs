using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                // fonte de ícones Material Icons
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // Configurar serviços da aplicação e repositórios
        ConfigurationHelper.ConfigureServices(builder.Services);

        // Listas de país, UF e municípios (lidas uma única vez do arquivo embutido)
        builder.Services.AddSingleton<LocalidadesService>();

        // Cadastro de logradouro em modal, aberto pelos formulários de Colaborador e Aluno
        builder.Services.AddTransient<LogradouroModalService>();

        // Registrar ViewModels
        builder.Services.AddTransient<DashboardListViewModel>();
        builder.Services.AddTransient<LogradouroListViewModel>();
        builder.Services.AddTransient<LogradouroViewModel>();
        builder.Services.AddTransient<ColaboradorListViewModel>();
        builder.Services.AddTransient<ColaboradorViewModel>();
        builder.Services.AddTransient<AlunoListViewModel>();
        builder.Services.AddTransient<AlunoViewModel>();

        // Registrar Views
        builder.Services.AddTransient<DashboardListPage>();
        builder.Services.AddTransient<LogradouroListPage>();
        builder.Services.AddTransient<LogradouroPage>();
        builder.Services.AddTransient<ColaboradorListPage>();
        builder.Services.AddTransient<ColaboradorPage>();
        builder.Services.AddTransient<AlunoListPage>();
        builder.Services.AddTransient<AlunoPage>();
        builder.Services.AddTransient<ConfigPage>();

#if WINDOWS
        RemoverBordaNativaDosCampos();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

#if WINDOWS
    // No Windows, TextBox, ComboBox e CalendarDatePicker nativos desenham fundo e borda retos próprios,
    // que aparecem como uma caixa quadrada dentro do contorno arredondado (CampoBorda).
    // Os recursos de estilo são sobrescritos em cada controle, valendo para todos os estados
    // (normal, mouse por cima, pressionado, em foco e desabilitado).
    private static void RemoverBordaNativaDosCampos()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("CampoSemBorda",
            (handler, _) => RemoverBorda(handler.PlatformView, "TextControl"));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("CampoSemBorda",
            (handler, _) => RemoverBorda(handler.PlatformView, "ComboBox"));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("CampoSemBorda", (handler, _) =>
        {
            RemoverBorda(handler.PlatformView, "CalendarDatePicker");
            // o estilo padrão alinha à esquerda; assim o seletor ocupa a largura do campo
            handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
        });
    }

    private static void RemoverBorda(Microsoft.UI.Xaml.Controls.Control controle, string prefixo)
    {
        static Microsoft.UI.Xaml.Media.SolidColorBrush Transparente() => new(Microsoft.UI.Colors.Transparent);
        var semBorda = new Microsoft.UI.Xaml.Thickness(0);

        // estado normal: vale para qualquer controle
        controle.Background = Transparente();
        controle.BorderBrush = Transparente();
        controle.BorderThickness = semBorda;

        // demais estados (mouse por cima, em foco...): o dicionário de recursos do controle nem sempre
        // aceita inserções (o CalendarDatePicker do MAUI recusa com COMException); nesse caso fica só o estado normal
        try
        {
            foreach (var estado in new[] { "", "PointerOver", "Pressed", "Focused", "Disabled" })
            {
                controle.Resources[$"{prefixo}Background{estado}"] = Transparente();
                controle.Resources[$"{prefixo}BorderBrush{estado}"] = Transparente();
            }
            controle.Resources[$"{prefixo}BorderThemeThickness"] = semBorda;
            controle.Resources[$"{prefixo}BorderThemeThicknessFocused"] = semBorda;
        }
        catch (System.Runtime.InteropServices.COMException)
        {
        }
    }
#endif
}
