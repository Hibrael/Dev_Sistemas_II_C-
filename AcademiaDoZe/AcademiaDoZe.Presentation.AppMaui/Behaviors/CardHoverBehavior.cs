namespace AcademiaDoZe.Presentation.AppMaui.Behaviors;

/// <summary>
/// Anima a cor de fundo de um card para um azul suave quando o ponteiro do mouse passa sobre ele.
/// Ao sair, volta à cor original e limpa o valor local, devolvendo o controle ao estilo (AppThemeBinding),
/// para que a troca de tema continue funcionando. Em telas de toque (Android/iOS) não há ponteiro,
/// então o behavior simplesmente não é acionado.
/// </summary>
public class CardHoverBehavior : Behavior<View>
{
    private const string NomeAnimacao = "CardHover";
    private const uint DuracaoMs = 200;

    private View? _view;
    private PointerGestureRecognizer? _pointer;
    private Color? _corOriginal;

    protected override void OnAttachedTo(View view)
    {
        base.OnAttachedTo(view);
        _view = view;
        _pointer = new PointerGestureRecognizer();
        _pointer.PointerEntered += OnPointerEntered;
        _pointer.PointerExited += OnPointerExited;
        view.GestureRecognizers.Add(_pointer);
    }

    protected override void OnDetachingFrom(View view)
    {
        if (_pointer != null)
        {
            _pointer.PointerEntered -= OnPointerEntered;
            _pointer.PointerExited -= OnPointerExited;
            view.GestureRecognizers.Remove(_pointer);
        }
        view.AbortAnimation(NomeAnimacao);
        _pointer = null;
        _view = null;
        base.OnDetachingFrom(view);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_view == null)
            return;

        _view.AbortAnimation(NomeAnimacao);
        // guarda a cor definida pelo estilo apenas na primeira entrada (antes de qualquer valor local)
        _corOriginal ??= _view.BackgroundColor;
        Animar(_view.BackgroundColor ?? _corOriginal, ObterCorHover(), aoTerminar: null);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_view == null || _corOriginal == null)
            return;

        _view.AbortAnimation(NomeAnimacao);
        var view = _view;
        Animar(view.BackgroundColor ?? _corOriginal, _corOriginal, aoTerminar: () =>
        {
            // devolve a cor ao estilo, mantendo o AppThemeBinding ativo
            view.ClearValue(VisualElement.BackgroundColorProperty);
            _corOriginal = null;
        });
    }

    private void Animar(Color? de, Color para, Action? aoTerminar)
    {
        if (_view == null)
            return;

        var inicio = de ?? para;
        var view = _view;
        var animacao = new Animation(t => view.BackgroundColor = Interpolar(inicio, para, t));
        animacao.Commit(view, NomeAnimacao, length: DuracaoMs, easing: Easing.CubicOut,
            finished: (_, cancelado) => { if (!cancelado) aoTerminar?.Invoke(); });
    }

    private static Color Interpolar(Color de, Color para, double t) => new(
        (float)(de.Red + (para.Red - de.Red) * t),
        (float)(de.Green + (para.Green - de.Green) * t),
        (float)(de.Blue + (para.Blue - de.Blue) * t),
        (float)(de.Alpha + (para.Alpha - de.Alpha) * t));

    private static Color ObterCorHover()
    {
        var app = Microsoft.Maui.Controls.Application.Current;
        var chave = app?.RequestedTheme == AppTheme.Dark ? "CardHoverDark" : "CardHoverLight";
        return app != null && app.Resources.TryGetValue(chave, out var valor) && valor is Color cor
            ? cor
            : Colors.LightBlue;
    }
}
