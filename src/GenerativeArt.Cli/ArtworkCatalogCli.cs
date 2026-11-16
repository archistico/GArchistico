using GenerativeArt.Application.Catalog;

namespace GenerativeArt.Cli;

internal static class ArtworkCatalogCli
{
    public static int Run(string[] args)
    {
        if (args.Length == 1 && args[0] == "list")
        {
            foreach (ArtworkDescriptor item in BuiltInArtworkCatalog.Create().All)
            {
                string animation = item.SupportsSeamlessLoop ? " [loop]" :
                    item.SupportsAnimation ? " [animazione]" : string.Empty;
                Console.WriteLine($"{item.Id} (v{item.AlgorithmVersion}) — {item.Title} [{item.Category}]{animation}");
            }
            return 0;
        }
        if (args.Length == 2 && args[0] == "describe")
        {
            ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
            if (!catalog.TryGet(args[1], out ArtworkDescriptor? descriptor))
            {
                Console.Error.WriteLine($"Opera sconosciuta: {args[1]}. Usa 'list'.");
                return 2;
            }
            Console.WriteLine($"{descriptor!.Title} ({descriptor.Id}, v{descriptor.AlgorithmVersion})");
            Console.WriteLine($"Categoria: {descriptor.Category}");
            Console.WriteLine($"Capacità: {descriptor.Capabilities}");
            Console.WriteLine(descriptor.Description);
            Console.WriteLine("Parametri dichiarati (editor visuale previsto in M3D):");
            foreach (ArtworkParameterSpec parameter in descriptor.Parameters)
            {
                string limits = parameter.Minimum is not null
                    ? $" [{parameter.Minimum}..{parameter.Maximum}]" : string.Empty;
                string choices = parameter.Choices.Count > 0
                    ? $" {{{string.Join(", ", parameter.Choices)}}}" : string.Empty;
                Console.WriteLine($"  {parameter.Id}: {parameter.Kind}, default={parameter.DefaultValue}{limits}{choices}");
            }
            return 0;
        }
        Console.Error.WriteLine("Uso: list | describe <artwork-id>");
        return 2;
    }
}
