//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;

public class ColaboradorDto : PessoaDto
{
    public required DateOnly DataAdmissao { get; set; }
    public required AppColaboradorTipo Tipo { get; set; }
    public required AppColaboradorVinculo Vinculo { get; set; }

    /// <summary>
    /// Campo próprio deste projeto (a entidade Colaborador exige salário maior que zero).
    /// </summary>
    public required decimal Salario { get; set; }

    /// <summary>
    /// Somente leitura: a demissão é registrada pela operação de domínio Colaborador.Desligar,
    /// não por Criar — por isso os ToEntity ignoram este campo.
    /// </summary>
    public DateOnly? DataDemissao { get; set; }
}
