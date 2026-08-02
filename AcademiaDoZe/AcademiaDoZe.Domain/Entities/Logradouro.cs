using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public class Logradouro: Entity
    {
        public string NomeLogradouro { get; private set; }
        public string Cep { get; private set; }
        public string Pais { get; private set; }
        public string Cidade { get; private set; }
        public string Bairro { get; private set; }
        public string Rua { get; private set; }

    }
}
