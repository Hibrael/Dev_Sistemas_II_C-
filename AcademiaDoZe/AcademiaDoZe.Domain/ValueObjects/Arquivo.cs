//Hibrael Andre Cidade Xavier
using System;
using System.IO;
using System.Linq;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Arquivo
    {
        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".pdf" };
        private const long TamanhoMaximoBytes = 5 * 1024 * 1024;

        public string Nome { get; private set; }
        public string Extensao { get; private set; }
        public long TamanhoBytes { get; private set; }
        public byte[] Conteudo { get; private set; }

        public Arquivo(string nome, byte[] conteudo)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do arquivo não pode ser vazio.");

            if (conteudo is null || conteudo.Length == 0)
                throw new ArgumentException("O conteúdo do arquivo não pode ser vazio.");

            var extensao = Path.GetExtension(nome).ToLowerInvariant();

            if (!ExtensoesPermitidas.Contains(extensao))
                throw new ArgumentException($"Extensão de arquivo não permitida: {extensao}");

            if (conteudo.Length > TamanhoMaximoBytes)
                throw new ArgumentException("Arquivo excede o tamanho máximo permitido (5 MB).");

            Nome = nome;
            Extensao = extensao;
            TamanhoBytes = conteudo.Length;
            Conteudo = conteudo;
        }
    }
}
