using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // rota de cadastro/edição, acessada via Shell.Current.GoToAsync("logradouro")
        Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
    }
}
