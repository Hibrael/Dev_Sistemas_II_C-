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

    // Redução das fotos de câmera: lado maior de até 1024 px, JPEG com qualidade 80
    private const int TamanhoMaximoPx = 1024;
    private const int QualidadeJpeg = 80;

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

#if WINDOWS
                // No Windows, o redimensionamento do MediaPicker (MAUI 10) devolve o arquivo com o stream
                // já fechado ("Cannot access a closed Stream"): captura sem opções e reduz depois, aqui
                result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Foto do cadastro" });
#else
                // Fotos de câmera costumam passar de 5 MB: o próprio MediaPicker do MAUI 10
                // redimensiona e recomprime em JPEG antes de devolver o arquivo.
                result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Foto do cadastro",
                    MaximumWidth = TamanhoMaximoPx,
                    MaximumHeight = TamanhoMaximoPx,
                    CompressionQuality = QualidadeJpeg
                });
#endif
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

            var conteudo = await LerConteudoAsync(result);

#if WINDOWS
            if (escolha == Camera && conteudo.Length > 0)
            {
                conteudo = await ReduzirFotoWindowsAsync(conteudo);
                nome = Path.ChangeExtension(nome, ".jpg");
            }
#endif

            if (conteudo.Length == 0)
            {
                await Shell.Current.DisplayAlertAsync("Foto inválida", "O arquivo selecionado está vazio.", "OK");
                return null;
            }

            if (conteudo.LongLength > PessoaValidacao.FotoTamanhoMaximoBytes)
            {
                var tamanhoMb = conteudo.LongLength / 1024d / 1024d;
                await Shell.Current.DisplayAlertAsync("Foto muito grande",
                    $"A imagem tem {tamanhoMb:N1} MB e o limite é 5 MB. Escolha uma imagem menor.", "OK");
                return null;
            }

            return new ArquivoDto { Conteudo = conteudo, Nome = nome };
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

    /// <summary>
    /// Lê o arquivo escolhido. Se o stream devolvido pelo MAUI já vier fechado,
    /// lê direto do caminho do arquivo, quando ele existir.
    /// </summary>
    private static async Task<byte[]> LerConteudoAsync(FileResult result)
    {
        try
        {
            await using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (ObjectDisposedException) when (File.Exists(result.FullPath))
        {
            return await File.ReadAllBytesAsync(result.FullPath);
        }
    }

#if WINDOWS
    /// <summary>
    /// Reduz a foto da câmera com o codificador de imagens do Windows: lado maior de até
    /// TamanhoMaximoPx (mantendo a proporção e a orientação EXIF) e JPEG com QualidadeJpeg.
    /// </summary>
    private static async Task<byte[]> ReduzirFotoWindowsAsync(byte[] original)
    {
        using var entrada = new MemoryStream(original).AsRandomAccessStream();
        var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(entrada);

        var escala = Math.Min(1.0, (double)TamanhoMaximoPx / Math.Max(decoder.PixelWidth, decoder.PixelHeight));
        var transformacao = new Windows.Graphics.Imaging.BitmapTransform
        {
            ScaledWidth = (uint)Math.Max(1, Math.Round(decoder.PixelWidth * escala)),
            ScaledHeight = (uint)Math.Max(1, Math.Round(decoder.PixelHeight * escala)),
            InterpolationMode = Windows.Graphics.Imaging.BitmapInterpolationMode.Fant
        };

        using var bitmap = await decoder.GetSoftwareBitmapAsync(
            Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
            Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied,
            transformacao,
            Windows.Graphics.Imaging.ExifOrientationMode.RespectExifOrientation,
            Windows.Graphics.Imaging.ColorManagementMode.DoNotColorManage);

        using var saida = new Windows.Storage.Streams.InMemoryRandomAccessStream();
        var opcoes = new Windows.Graphics.Imaging.BitmapPropertySet
        {
            { "ImageQuality", new Windows.Graphics.Imaging.BitmapTypedValue(QualidadeJpeg / 100f, Windows.Foundation.PropertyType.Single) }
        };
        var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(
            Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, saida, opcoes);
        encoder.SetSoftwareBitmap(bitmap);
        await encoder.FlushAsync();

        saida.Seek(0);
        using var ms = new MemoryStream();
        await saida.AsStreamForRead().CopyToAsync(ms);
        return ms.ToArray();
    }
#endif
}
