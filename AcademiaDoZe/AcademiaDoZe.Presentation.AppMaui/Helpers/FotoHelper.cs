//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

/// <summary>
/// Seleção da foto de Colaborador/Aluno: galeria (FilePicker) ou câmera (MediaPicker).
/// O resultado já sai convertido para ArquivoDto (array de bytes + nome com extensão),
/// pronto para ser persistido.
///
/// Como o domínio exige foto, não há opção de remover: apenas trocar.
/// </summary>
public static class FotoHelper
{
    private const string Galeria = "Galeria";
    private const string Camera = "Câmera";

    // Restringe o seletor às extensões aceitas pelo VO Arquivo (.jpg, .jpeg, .png)
    private static readonly FilePickerFileType TiposPermitidos = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.Android, ["image/jpeg", "image/png"] },
        { DevicePlatform.iOS, ["public.jpeg", "public.png"] },
        { DevicePlatform.MacCatalyst, ["public.jpeg", "public.png"] },
        { DevicePlatform.WinUI, [".jpg", ".jpeg", ".png"] }
    });

    /// <summary>
    /// Pergunta a origem da foto e devolve o arquivo escolhido, ou null se o usuário cancelar
    /// ou se a imagem não atender às regras (o motivo é exibido num alerta).
    /// </summary>
    public static async Task<ArquivoDto?> SelecionarAsync()
    {
        try
        {
            var escolha = await Shell.Current.DisplayActionSheetAsync("Origem da foto", "Cancelar", null, Galeria, Camera);

            FileResult? result = null;
            if (escolha == Galeria)
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione uma foto (.jpg ou .png)",
                    FileTypes = TiposPermitidos
                });
            }
            else if (escolha == Camera)
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlertAsync("Câmera", "Captura de foto não suportada neste dispositivo.", "OK");
                    return null;
                }

                // Fotos de câmera costumam passar de 5 MB: o próprio MediaPicker do MAUI 10
                // redimensiona e recomprime em JPEG antes de devolver o arquivo.
                result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Foto do cadastro",
                    MaximumWidth = 1024,
                    MaximumHeight = 1024,
                    CompressionQuality = 80
                });
            }

            if (result is null)
                return null;

            var nome = result.FileName;
            if (!PessoaValidacao.FotoExtensaoValida(nome))
            {
                if (escolha == Camera)
                {
                    // a câmera já devolve JPEG; só o nome pode vir sem extensão conhecida
                    nome = Path.ChangeExtension(string.IsNullOrWhiteSpace(nome) ? "foto_camera" : nome, ".jpg");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Foto inválida", "Escolha uma imagem .jpg, .jpeg ou .png.", "OK");
                    return null;
                }
            }

            await using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            if (ms.Length == 0)
            {
                await Shell.Current.DisplayAlertAsync("Foto inválida", "O arquivo selecionado está vazio.", "OK");
                return null;
            }

            if (ms.Length > PessoaValidacao.FotoTamanhoMaximoBytes)
            {
                var tamanhoMb = ms.Length / 1024d / 1024d;
                await Shell.Current.DisplayAlertAsync("Foto muito grande",
                    $"A imagem tem {tamanhoMb:N1} MB e o limite é 5 MB. Escolha uma imagem menor.", "OK");
                return null;
            }

            return new ArquivoDto { Conteudo = ms.ToArray(), Nome = nome };
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlertAsync("Permissão negada", "Libere o acesso à câmera nas configurações do dispositivo para tirar a foto.", "OK");
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlertAsync("Câmera", "Captura de foto não suportada neste dispositivo.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }

        return null;
    }
}
