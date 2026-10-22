//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MatriculaViewModel viewModel)
        {
            await viewModel.InitializeAsync();

            // garante a data final calculada também ao carregar uma matrícula existente
            CalculateFimFromPlanoAndInicio();
        }
    }

    // A data final é recalculada sempre que o plano ou a data de início mudam
    private void PlanoPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }

    private void InicioDatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }

    private void CalculateFimFromPlanoAndInicio()
    {
        if (BindingContext is MatriculaViewModel viewModel)
            viewModel.CalcularDataFim();
    }
}
