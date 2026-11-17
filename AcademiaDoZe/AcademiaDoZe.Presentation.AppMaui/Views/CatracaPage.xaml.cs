//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class CatracaPage : ContentPage
{
    public CatracaPage(CatracaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CatracaViewModel viewModel)
        {
            await viewModel.CarregarCommand.ExecuteAsync(null);
        }
    }
}
