using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace GenerativeArt.Studio;

/// <summary>
/// Applicazione Avalonia desktop: tema e gestione del ciclo di vita.
/// Usiamo il nome StudioApp per evitare collisioni con GenerativeArt.Application.
/// </summary>
public sealed class StudioApp : global::Avalonia.Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new StudioWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
