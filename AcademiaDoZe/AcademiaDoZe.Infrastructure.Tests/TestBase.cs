//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests
{
    public abstract class TestBase
    {
        // Alterne o SGBD alvo dos testes trocando apenas a constante abaixo:
        private const DatabaseType SelectedDatabaseType = DatabaseType.MySql;

        protected static string NomeSgbdAtual => SelectedDatabaseType switch
        {
            DatabaseType.SqlServer => "SQLServer",
            DatabaseType.MySql => "MySQL",
            DatabaseType.Sqlite => "SQLite",
            _ => "Desconhecido"
        };

        protected static string SiglaSgbdAtual => SelectedDatabaseType switch
        {
            DatabaseType.SqlServer => "SQL",
            DatabaseType.MySql => "MY",
            DatabaseType.Sqlite => "SQLITE",
            _ => "UNK"
        };

        private static readonly string DatabasePath = @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db";

        static TestBase()
        {
            var dir = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (SelectedDatabaseType == DatabaseType.Sqlite && File.Exists(DatabasePath))
                File.Delete(DatabasePath);
        }

        protected DatabaseType DatabaseType { get; } = SelectedDatabaseType;

        protected string ConnectionString { get; } = SelectedDatabaseType switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=coelho;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;",
            DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=coelho;Password=abcBolinhas12345;",
            DatabaseType.Sqlite => $"Data Source={DatabasePath};Cache=Shared;",
            _ => throw new ArgumentOutOfRangeException(nameof(SelectedDatabaseType), SelectedDatabaseType, "SGBD não suportado para testes.")
        };

        private static int counter = 10000;

        protected static string GerarCep() =>
            (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + Interlocked.Increment(ref counter)).ToString("D8")[..8];

        protected static string GerarCpf()
        {
            // Cpf.Criar valida dígito verificador de verdade (ver Cpf.ValidarCpf) — não basta gerar
            // 11 dígitos aleatórios, é preciso calcular os 2 dígitos verificadores corretamente,
            // seguindo o mesmo algoritmo usado lá.
            var baseDigitos = (100000000L + (DateTime.UtcNow.Ticks % 800000000L) + Interlocked.Increment(ref counter)).ToString("D9")[..9];
            if (baseDigitos.Distinct().Count() == 1) baseDigitos = "1" + baseDigitos[1..]; // evita CPF com todos os dígitos iguais, rejeitado por ValidarCpf

            var d1 = CalcularDigitoVerificadorCpf(baseDigitos, 9);
            var d2 = CalcularDigitoVerificadorCpf(baseDigitos + d1, 10);
            return $"{baseDigitos}{d1}{d2}";
        }

        private static int CalcularDigitoVerificadorCpf(string cpf, int tamanho)
        {
            var soma = 0;
            var peso = tamanho + 1;
            for (var i = 0; i < tamanho; i++)
            {
                soma += (cpf[i] - '0') * peso;
                peso--;
            }
            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        protected static string GerarEmail() =>
            $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";

        protected static string GerarTelefone() =>
            (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref counter)).ToString("D11")[..11];

        // Requisito da atividade: a senha usada nos testes deve conter a sigla do SGBD ativo
        // em texto puro, sem hash, na camada de testes.
        protected static string GerarSenha() => $"Senha{SiglaSgbdAtual}123";
    }
}