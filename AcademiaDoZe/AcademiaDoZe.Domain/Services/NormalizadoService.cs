//Hibrael Andre Cidade Xavier
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.Services
{
    public static partial class NormalizadoService
    {
        public static bool TextoVazioOuNulo(string? texto) => string.IsNullOrWhiteSpace(texto);

        public static string LimparEspacos(string texto) =>
            EspacosRegex().Replace(texto.Trim(), " ");

        public static string LimparTodosEspacos(string texto) =>
            EspacosRegex().Replace(texto, string.Empty);

        public static string ApenasDigitos(string texto) =>
            NaoDigitoRegex().Replace(texto, string.Empty);

        public static string ParaMaiusculo(string texto) => texto.Trim().ToUpperInvariant();

        public static string ParaMinusculo(string texto) => texto.Trim().ToLowerInvariant();

        [GeneratedRegex(@"\s+")]
        private static partial Regex EspacosRegex();

        [GeneratedRegex(@"[^\d]")]
        private static partial Regex NaoDigitoRegex();
    }
}
