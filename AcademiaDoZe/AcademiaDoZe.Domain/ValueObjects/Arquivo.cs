//Hibrael Andre Cidade Xavier
using System;
using System.IO;
using System.Linq;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public class Arquivo
    {
        public byte[] Conteudo { get; private set; }

        public Arquivo(string nome, byte[] conteudo)
        {
            if (conteudo is null || conteudo.Length == 0)
            {
                throw new ArgumentException("O conteúdo do arquivo não pode ser vazio.");

                Nome = nome;
                Extensao = extensao;
                TamanhoBytes = conteudo.Length;
                Conteudo = conteudo;
            }

        }
    }
}
