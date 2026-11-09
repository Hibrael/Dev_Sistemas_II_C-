//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

/// <summary>
/// Item da seleção múltipla (CheckBoxes) de restrições médicas: um valor do enum [Flags]
/// AppMatriculaRestricoes e se ele está marcado. MatriculaViewModel junta os itens marcados
/// num único bitmask (OR) e grava em MatriculaDto.RestricoesMedicas.
/// </summary>
public class RestricaoSelectable : ObservableObject
{
    public AppMatriculaRestricoes Value { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
