using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Enums
{
    internal class MatriculaRestricoes
    {
        [Flags]
        public enum MatriculaRestricoes
        {
            Nenhuma = 0,
            Diabetes = 1,
            Labirintite = 2,
            ProblemasRespiratórios = 4,
            RemedioContinuo = 8,
            ProblemasCardiacos = 16,
            ProblemasOsseos = 32,
            CirurgiaDebilitante = 64,

        }
        readonly
    }
}
