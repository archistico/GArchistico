using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioRenderCoordinatorTests
{
    private readonly ArtworkRegistry _catalog = BuiltInArtworkCatalog.Create();

    [Fact]
    public async Task PreviewUsesTheUnmodifiedRecipeAndReturnsPngBytes()
    {
        ArtworkRecipe recipe = ArtworkRecipe.Create(_catalog.Get("plasma-noise"),
            new CanvasSize(320, 240), 1841UL);
        var fake = new ControllableRenderer();
        var coordinator = new StudioRenderCoordinator(fake);

        ArtworkRecipe exact = StudioPreviewPolicy.CreateExactPreview(recipe, _catalog);
        byte[] png = await coordinator.RenderPngAsync(exact);

        Assert.Same(recipe, exact);
        Assert.Same(recipe, fake.LastRecipe);
        Assert.Equal(new byte[] { 137, 80, 78, 71 }, png);
        Assert.Equal(1, fake.Calls);
    }

    [Fact]
    public async Task SecondRenderWaitsForFirstAndNeverRunsConcurrently()
    {
        var fake = new ControllableRenderer { BlockFirstRender = true };
        var coordinator = new StudioRenderCoordinator(fake);
        ArtworkRecipe recipe = NewRecipe();
        Task<byte[]> first = coordinator.RenderPngAsync(recipe);
        Assert.True(fake.FirstStarted.Wait(TimeSpan.FromSeconds(5)), "Il primo rendering non è partito.");
        try
        {
            Task<byte[]> second = coordinator.RenderPngAsync(recipe);
            Assert.False(second.IsCompleted);
            Assert.Equal(1, fake.Calls);
            fake.ReleaseFirst.Set();
            await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(2, fake.Calls);
            Assert.False(fake.WasConcurrent);
        }
        finally { fake.ReleaseFirst.Set(); }
    }

    [Fact]
    public async Task CancelledQueuedRenderNeverReachesRenderer()
    {
        var fake = new ControllableRenderer { BlockFirstRender = true };
        var coordinator = new StudioRenderCoordinator(fake);
        ArtworkRecipe recipe = NewRecipe();
        Task<byte[]> first = coordinator.RenderPngAsync(recipe);
        Assert.True(fake.FirstStarted.Wait(TimeSpan.FromSeconds(5)));
        try
        {
            using var cancellation = new CancellationTokenSource();
            Task<byte[]> queued = coordinator.RenderPngAsync(recipe, cancellationToken: cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => { await queued; });
            Assert.Equal(1, fake.Calls);
        }
        finally { fake.ReleaseFirst.Set(); }
        await first.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CancelledAtomicExportPreservesExistingDestination()
    {
        string folder = Path.Combine(Path.GetTempPath(), "gart-coordinator-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string destination = Path.Combine(folder, "opera.png");
        byte[] old = [9, 8, 7];
        await File.WriteAllBytesAsync(destination, old);
        var fake = new ControllableRenderer { BlockFirstRender = true };
        var coordinator = new StudioRenderCoordinator(fake);
        using var cancellation = new CancellationTokenSource();
        try
        {
            Task export = coordinator.RenderToAtomicFileAsync(NewRecipe(), destination,
                cancellationToken: cancellation.Token);
            Assert.True(fake.FirstStarted.Wait(TimeSpan.FromSeconds(5)));
            cancellation.Cancel();
            fake.ReleaseFirst.Set();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await export);
            Assert.Equal(old, await File.ReadAllBytesAsync(destination));
            Assert.Empty(Directory.GetFiles(folder, "*.partial", SearchOption.TopDirectoryOnly));
        }
        finally
        {
            fake.ReleaseFirst.Set();
            Directory.Delete(folder, recursive: true);
        }
    }

    private ArtworkRecipe NewRecipe() => ArtworkRecipe.Create(_catalog.Get("plasma-noise"),
        new CanvasSize(160, 120), 42UL);

    private sealed class ControllableRenderer : IArtworkRasterRenderer
    {
        private int _calls;
        private int _active;
        private int _wasConcurrent;
        public bool BlockFirstRender { get; init; }
        public ManualResetEventSlim FirstStarted { get; } = new(false);
        public ManualResetEventSlim ReleaseFirst { get; } = new(false);
        public ArtworkRecipe? LastRecipe { get; private set; }
        public int Calls => Volatile.Read(ref _calls);
        public bool WasConcurrent => Volatile.Read(ref _wasConcurrent) != 0;

        public void Validate(ArtworkRecipe recipe, int? frameIndex = null) { }

        public void Render(ArtworkRecipe recipe, Stream destination, int? frameIndex = null,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _active) > 1)
                Interlocked.Exchange(ref _wasConcurrent, 1);
            int call = Interlocked.Increment(ref _calls);
            try
            {
                LastRecipe = recipe;
                if (BlockFirstRender && call == 1)
                {
                    FirstStarted.Set();
                    ReleaseFirst.Wait(cancellationToken);
                }
                cancellationToken.ThrowIfCancellationRequested();
                destination.Write(new byte[] { 137, 80, 78, 71 });
            }
            finally { Interlocked.Decrement(ref _active); }
        }
    }
}
