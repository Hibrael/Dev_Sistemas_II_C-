//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Application.DTOs
{
    /// <summary>
    /// Resposta da catraca a uma tentativa de entrada ou saída, de aluno ou de colaborador.
    ///
    /// Um acesso negado (sem matrícula ativa, jornada estourada, entrada em duplicidade...) não é
    /// um erro do sistema e por isso não vira exceção: volta aqui com Permitido = false e o motivo
    /// em Mensagem, para a apresentação exibir "Acesso negado" sem tratar como falha.
    ///
    /// Todos os horários deste DTO já estão no fuso local, prontos para exibição.
    /// </summary>
    public sealed class RegistroAcessoResultadoDto
    {
        public required bool Permitido { get; init; }

        /// <summary>Ex.: "Entrada liberada", "Saída registrada", "Acesso negado".</summary>
        public required string Titulo { get; init; }

        /// <summary>Explicação para quem está na catraca (motivo da negação ou resumo do acesso).</summary>
        public required string Mensagem { get; init; }

        public int PessoaId { get; init; }
        public string NomePessoa { get; init; } = string.Empty;

        /// <summary>Id do registro em tb_acesso (0 quando o acesso foi negado).</summary>
        public int AcessoId { get; init; }

        public DateTime? Entrada { get; init; }
        public DateTime? Saida { get; init; }

        /// <summary>Tempo desta ida à academia (só na saída).</summary>
        public TimeSpan? Permanencia { get; init; }

        // ---- Aluno ----

        /// <summary>Último dia do plano da matrícula ativa (aluno).</summary>
        public DateOnly? FimPlano { get; init; }

        /// <summary>Dias que ainda restam de plano, contando de hoje até FimPlano (aluno).</summary>
        public int? DiasRestantesPlano { get; init; }

        // ---- Colaborador ----

        /// <summary>Soma da permanência de todos os acessos do dia (colaborador).</summary>
        public TimeSpan? PermanenciaNoDia { get; init; }

        /// <summary>Limite diário do vínculo: 8 h (CLT/PJ) ou 6 h (estágio) (colaborador).</summary>
        public TimeSpan? JornadaDiariaMaxima { get; init; }

        /// <summary>Quanto ainda pode ficar hoje sem estourar a jornada (colaborador).</summary>
        public TimeSpan? JornadaRestante { get; init; }

        /// <summary>Verdadeiro quando, na saída, a soma do dia passou do limite (colaborador).</summary>
        public bool UltrapassouJornada { get; init; }

        /// <summary>Formata uma duração como "2 h 05 min" (ou "45 min" / "menos de 1 min").</summary>
        public static string FormatarDuracao(TimeSpan duracao)
        {
            if (duracao < TimeSpan.Zero) duracao = TimeSpan.Zero;

            var horas = (int)duracao.TotalHours;
            var minutos = duracao.Minutes;

            if (horas == 0 && minutos == 0) return "menos de 1 min";
            if (horas == 0) return $"{minutos} min";
            return $"{horas} h {minutos:00} min";
        }
    }
}
