using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Files;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioRecipeDocumentsTests
{
    private readonly ArtworkRegistry _catalog = BuiltInArtworkCatalog.Create();

    private ArtworkRecipe NewRecipe() => ArtworkRecipe.Create(_catalog.Get("plasma-noise"),
        new CanvasSize(320, 240), 18446744073709551615UL);

    [Fact]
    public async Task RoundTripPreservesRecipeAndDoesNotCloseCallerStream()
    {
        var validator = new TrackingRenderer();
        var documents = new StudioRecipeDocuments(_catalog, validator);
        ArtworkRecipe original = NewRecipe();
        await using var output = new MemoryStream();
        await documents.SaveStreamAsync(output, original);
        Assert.True(output.CanWrite);
        output.Position = 0;
        ArtworkRecipe restored = await documents.LoadAsync(output);
        Assert.True(output.CanRead);
        Assert.Equal(original.ArtworkId, restored.ArtworkId);
        Assert.Equal(original.Seed, restored.Seed);
        Assert.Equal(original.Size.Width, restored.Size.Width);
        Assert.Equal(original.Size.Height, restored.Size.Height);
        Assert.Equal(original.Parameters.OrderBy(x => x.Key), restored.Parameters.OrderBy(x => x.Key));
        Assert.Same(restored, validator.LastValidated);
    }

    [Fact]
    public async Task OversizedNonSeekableDocumentIsRejectedBeforeParsing()
    {
        var validator = new TrackingRenderer();
        var documents = new StudioRecipeDocuments(_catalog, validator);
        using var source = new ChunkedReadStream(new byte[ArtworkRecipeJson.MaximumUtf8Bytes + 1]);
        await Assert.ThrowsAsync<InvalidDataException>(() => documents.LoadAsync(source));
        Assert.True(source.TotalRead <= ArtworkRecipeJson.MaximumUtf8Bytes + 1);
        Assert.Null(validator.LastValidated);
    }

    [Fact]
    public async Task CorruptOrEmptyDocumentNeverReachesRenderer()
    {
        var validator = new TrackingRenderer();
        var documents = new StudioRecipeDocuments(_catalog, validator);
        await Assert.ThrowsAnyAsync<Exception>(() => documents.LoadAsync(new MemoryStream("{"u8.ToArray())));
        await Assert.ThrowsAnyAsync<Exception>(() => documents.LoadAsync(new MemoryStream()));
        Assert.Null(validator.LastValidated);
    }

    [Fact]
    public async Task RendererValidationHappensBeforeLoadingIntoTheStudio()
    {
        var validator = new TrackingRenderer { Reject = true };
        var documents = new StudioRecipeDocuments(_catalog, validator);
        using var stream = new MemoryStream(documents.Serialize(NewRecipe()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => documents.LoadAsync(stream));
        Assert.NotNull(validator.LastValidated);
    }

    [Fact]
    public async Task CancelledReadDoesNotValidateDocument()
    {
        var validator = new TrackingRenderer();
        var documents = new StudioRecipeDocuments(_catalog, validator);
        using var source = new MemoryStream(documents.Serialize(NewRecipe()));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            documents.LoadAsync(source, cancellation.Token));
        Assert.Null(validator.LastValidated);
    }

    [Fact]
    public async Task LocalSaveIsAtomicAndCanBeLoadedFromFile()
    {
        string directory = Path.Combine(Path.GetTempPath(), "gart-recipe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "sample.json");
        try
        {
            await File.WriteAllTextAsync(path, "old-version");
            var documents = new StudioRecipeDocuments(_catalog, new TrackingRenderer());
            await documents.SaveFileAsync(path, NewRecipe());
            ArtworkRecipe loaded = await documents.LoadFileAsync(path);
            Assert.Equal(NewRecipe().Seed, loaded.Seed);
            Assert.DoesNotContain(Directory.EnumerateFiles(directory),
                file => file.EndsWith(".partial", StringComparison.Ordinal));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task CancelledLocalSavePreservesPreviousDocument()
    {
        string directory = Path.Combine(Path.GetTempPath(), "gart-recipe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "sample.json");
        try
        {
            byte[] previous = "previous"u8.ToArray();
            await File.WriteAllBytesAsync(path, previous);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var documents = new StudioRecipeDocuments(_catalog, new TrackingRenderer());
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                documents.SaveFileAsync(path, NewRecipe(), cancellation.Token));
            Assert.Equal(previous, await File.ReadAllBytesAsync(path));
            Assert.DoesNotContain(Directory.EnumerateFiles(directory),
                file => file.EndsWith(".partial", StringComparison.Ordinal));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private sealed class TrackingRenderer : IArtworkRasterRenderer
    {
        public ArtworkRecipe? LastValidated { get; private set; }
        public bool Reject { get; init; }
        public void Validate(ArtworkRecipe recipe, int? frameIndex = null)
        {
            LastValidated = recipe;
            if (Reject) throw new InvalidOperationException("Il renderer rifiuta la ricetta.");
        }
        public void Render(ArtworkRecipe recipe, Stream destination, int? frameIndex = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ChunkedReadStream : MemoryStream
    {
        public int TotalRead { get; private set; }
        public ChunkedReadStream(byte[] content) : base(content) { }
        public override bool CanSeek => false;
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            int count = await base.ReadAsync(buffer[..Math.Min(buffer.Length, 127)], cancellationToken);
            TotalRead += count;
            return count;
        }
    }
}
