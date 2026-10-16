//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

/// <summary>
/// Abre o cadastro de Logradouro como modal, a partir das telas de Colaborador e Aluno,
/// para cadastrar um CEP que ainda não existe sem sair do formulário.
/// Reaproveita a LogradouroPage (País/UF/Cidade em listas e as mesmas validações).
/// </summary>
public sealed class LogradouroModalService(IServiceProvider services)
{
    /// <summary>
    /// Mostra o modal com o CEP já preenchido e aguarda o usuário salvar ou cancelar.
    /// Retorna o logradouro gravado (com Id) ou null se cancelado.
    /// </summary>
    public async Task<LogradouroDto?> CadastrarAsync(string? cep)
    {
        var page = services.GetRequiredService<LogradouroPage>();
        if (page.BindingContext is not LogradouroViewModel viewModel)
            return null;

        var resultado = new TaskCompletionSource<LogradouroDto?>(TaskCreationOptions.RunContinuationsAsynchronously);
        viewModel.ConfigurarModal(cep, resultado);

        // NavigationPage dá ao modal a barra de título ("Novo Logradouro")
        await Shell.Current.Navigation.PushModalAsync(new NavigationPage(page));
        return await resultado.Task;
    }
}
