using System.Text.Json;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

/// <summary>
/// Listas fixas usadas nos campos de endereço, para o usuário escolher em vez de digitar.
/// As cidades vêm do arquivo Resources/Raw/municipios.json (municípios do IBGE agrupados por UF),
/// que é empacotado com o app — nenhuma API é chamada durante o uso.
/// </summary>
public class LocalidadesService
{
    public const string Brasil = "Brasil";

    public IReadOnlyList<string> Paises { get; } =
    [
        "Argentina", "Bolívia", "Brasil", "Chile", "Colômbia", "Costa Rica", "Cuba", "El Salvador",
        "Equador", "Guatemala", "Haiti", "Honduras", "México", "Nicarágua", "Panamá", "Paraguai",
        "Peru", "República Dominicana", "Uruguai", "Venezuela"
    ];

    public IReadOnlyList<string> Ufs { get; } =
    [
        "AC", "AL", "AM", "AP", "BA", "CE", "DF", "ES", "GO", "MA", "MG", "MS", "MT", "PA",
        "PB", "PE", "PI", "PR", "RJ", "RN", "RO", "RR", "RS", "SC", "SE", "SP", "TO"
    ];

    private Dictionary<string, List<string>>? _cidadesPorUf;
    private readonly SemaphoreSlim _carregamento = new(1, 1);

    /// <summary>Cidades da UF informada, em ordem alfabética. Lê o arquivo apenas na primeira chamada.</summary>
    public async Task<IReadOnlyList<string>> ObterCidadesAsync(string? uf)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return [];

        await CarregarAsync();
        return _cidadesPorUf!.TryGetValue(uf.Trim().ToUpperInvariant(), out var cidades) ? cidades : [];
    }

    public bool UfValida(string? uf) => !string.IsNullOrWhiteSpace(uf) && Ufs.Contains(uf.Trim().ToUpperInvariant());

    private async Task CarregarAsync()
    {
        if (_cidadesPorUf != null)
            return;

        await _carregamento.WaitAsync();
        try
        {
            if (_cidadesPorUf != null)
                return;

            await using var arquivo = await FileSystem.OpenAppPackageFileAsync("municipios.json");
            _cidadesPorUf = await JsonSerializer.DeserializeAsync<Dictionary<string, List<string>>>(arquivo) ?? [];
        }
        finally
        {
            _carregamento.Release();
        }
    }
}
