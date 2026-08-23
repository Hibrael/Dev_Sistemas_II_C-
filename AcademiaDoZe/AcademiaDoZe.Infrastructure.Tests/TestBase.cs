//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests
{
    public abstract class TestBase
    {
        private const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;
        private static readonly string DatabasePath = Path.Combine(Path.GetTempPath(), "academia_do_ze.db");

        static TestBase()
        {
            if (File.Exists(DatabasePath)) File.Delete(DatabasePath);
        }

        protected DatabaseType DatabaseType { get; } = SelectedDatabaseType;
        protected string ConnectionString { get; } = $"Data Source={DatabasePath};Cache=Shared;";

        private static int counter = 10000;
        protected static string GerarCep() => (80000000 + Interlocked.Increment(ref counter)).ToString("D8");
    }
}