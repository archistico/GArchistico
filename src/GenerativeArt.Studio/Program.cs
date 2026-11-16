using Avalonia;

namespace GenerativeArt.Studio;

internal static class Program
{
    // Entry point standard per applicazioni desktop Avalonia 12.
    // La finestra viene registrata da StudioApp nel lifetime desktop.
    [STAThread]
    public static int Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<StudioApp>().UsePlatformDetect();
}
