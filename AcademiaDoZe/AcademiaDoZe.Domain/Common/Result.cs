//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Domain.Common
{
    /// <summary>
    /// Encapsula o resultado da criação/operação de um objeto de domínio: ou o objeto foi
    /// construído com sucesso (Success), ou a operação falhou e carrega a lista de
    /// Notification que explica por quê (Failure).
    ///
    /// Este é o mecanismo que permite que os métodos estáticos "Criar" das Entities e
    /// Value Objects nunca lancem exceção para erros de validação de entrada — exceções
    /// (DomainException) ficam reservadas para violação de invariantes em operações de
    /// domínio (ex.: cancelar uma matrícula já cancelada), não para dados de entrada inválidos.
    /// </summary>
    /// <typeparam name="T">Tipo do valor produzido quando a operação é bem-sucedida.</typeparam>
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Valor construído com sucesso. Só deve ser lido quando IsSuccess é true;
        /// nos pontos de consumo isso é expresso com o operador de perdão de nulo (Value!),
        /// já que o compilador não consegue provar a relação entre IsSuccess e Value.
        /// </summary>
        public T? Value { get; }

        public IReadOnlyCollection<Notification> Notifications { get; }

        private Result(bool isSuccess, T? value, IReadOnlyCollection<Notification> notifications)
        {
            IsSuccess = isSuccess;
            Value = value;
            Notifications = notifications;
        }

        public static Result<T> Success(T value) =>
            new(true, value, Array.Empty<Notification>());

        public static Result<T> Failure(IEnumerable<Notification> notifications) =>
            new(false, default, notifications.ToList());

        public static Result<T> Failure(Notification notification) =>
            new(false, default, new[] { notification });
    }
}
