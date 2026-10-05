namespace AcademiaDoZe.Presentation.AppMaui.Behaviors;

/// <summary>
/// Restringe o que pode ser digitado num Entry: apenas letras (com ou sem acento) e, opcionalmente,
/// espaço, hífen e apóstrofo — o suficiente para nomes de cidades e siglas de estado de outros países.
/// No Windows o caractere inválido é recusado antes de entrar no campo (BeforeTextChanging do TextBox
/// nativo), o que preserva a posição do cursor; nas demais plataformas o texto é filtrado após a mudança.
/// </summary>
public class ApenasLetrasBehavior : Behavior<Entry>
{
    public static readonly BindableProperty PermitirEspacosProperty =
        BindableProperty.Create(nameof(PermitirEspacos), typeof(bool), typeof(ApenasLetrasBehavior), false);

    public bool PermitirEspacos
    {
        get => (bool)GetValue(PermitirEspacosProperty);
        set => SetValue(PermitirEspacosProperty, value);
    }

    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        entry.TextChanged += OnTextChanged;
        entry.HandlerChanged += OnHandlerChanged;
        OnHandlerChanged(entry, EventArgs.Empty);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged;
        entry.HandlerChanged -= OnHandlerChanged;
#if WINDOWS
        if (entry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
            textBox.BeforeTextChanging -= OnBeforeTextChanging;
#endif
        base.OnDetachingFrom(entry);
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
#if WINDOWS
        if (sender is Entry { Handler.PlatformView: Microsoft.UI.Xaml.Controls.TextBox textBox })
        {
            textBox.BeforeTextChanging -= OnBeforeTextChanging;
            textBox.BeforeTextChanging += OnBeforeTextChanging;
        }
#endif
    }

#if WINDOWS
    // recusa a alteração inteira se ela trouxer algum caractere não permitido (digitado ou colado)
    private void OnBeforeTextChanging(Microsoft.UI.Xaml.Controls.TextBox sender, Microsoft.UI.Xaml.Controls.TextBoxBeforeTextChangingEventArgs args)
    {
        if (!args.NewText.All(CaractereValido))
            args.Cancel = true;
    }
#endif

    // reserva para as outras plataformas (e para valores atribuídos por código)
    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry || string.IsNullOrEmpty(e.NewTextValue))
            return;

        var filtrado = new string([.. e.NewTextValue.Where(CaractereValido)]);
        if (filtrado == e.NewTextValue)
            return;

        entry.Text = filtrado;
        // o cursor é reposicionado depois que a plataforma termina de aplicar o texto
        entry.Dispatcher.Dispatch(() => entry.CursorPosition = entry.Text?.Length ?? 0);
    }

    private bool CaractereValido(char c) =>
        char.IsLetter(c) || (PermitirEspacos && (c == ' ' || c == '-' || c == '\''));
}
