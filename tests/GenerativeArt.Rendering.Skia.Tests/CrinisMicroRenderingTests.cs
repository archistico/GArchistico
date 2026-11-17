using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace GenerativeArt.Rendering.Skia.Tests;

public sealed class CrinisMicroRenderingTests
{
    private static readonly ArtworkRegistry Registry = BuiltInArtworkCatalog.Create();
    private static readonly ArtworkRenderEngine Engine = new(Registry);

    private static ArtworkRecipe Make(IReadOnlyDictionary<string, string>? values = null, ulong seed = 42,
        int width = 192, int height = 128)
    {
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["detail-style"] = "micro", ["bands"] = "9", ["samples"] = "160",
            ["bristle-pitch"] = "2.1", ["fiber-density"] = "1.2",
            ["hair-width"] = "0.65", ["body-opacity"] = "0",
            ["shadow-strength"] = "0.45", ["fiber-relief"] = "0.72",
            ["fiber-fray"] = "0.64", ["micro-detail"] = "0.8",
            ["depth-lighting"] = "0.68", ["rear-softness"] = "0.32",
            ["vortex-centers"] = "3", ["ink-flecks"] = "80"
        };
        if (values is not null)
            foreach (KeyValuePair<string, string> item in values) parameters[item.Key] = item.Value;
        return ArtworkRecipe.Create(Registry.Get("crinis-bristles"), new CanvasSize(width, height), seed, parameters);
    }

    private static byte[] Png(ArtworkRecipe recipe)
    {
        using var stream = new MemoryStream();
        Engine.Render(recipe, stream);
        return stream.ToArray();
    }

    [Theory]
    [InlineData("classic")]
    [InlineData("micro")]
    public void ZeroBodyOpacityEliminatesWholeBandShadow(string mode)
    {
        var initial = new Dictionary<string, string>
        {
            ["body-opacity"] = "0", ["shadow-strength"] = "0",
            ["detail-style"] = mode, ["ink-flecks"] = "0"
        };
        byte[] noShadow = Png(Make(initial));
        initial["shadow-strength"] = "0.65";
        Assert.Equal(noShadow, Png(Make(initial)));
    }

    [Theory]
    [InlineData("fiber-density", "2.0")]
    [InlineData("hair-width", "1.8")]
    [InlineData("fiber-coverage", "0.25")]
    [InlineData("pigment-contrast", "0")]
    [InlineData("micro-detail", "0.1")]
    [InlineData("depth-lighting", "0.1")]
    [InlineData("rear-softness", "0")]
    [InlineData("bundle-compression", "0.58")]
    [InlineData("bundle-compression", "-0.25")]
    [InlineData("bundle-volume", "0.8")]
    [InlineData("bundle-lighting", "0.9")]
    [InlineData("intra-bundle-depth", "0.85")]
    [InlineData("featured-fibers", "0.85")]
    [InlineData("cross-section-light", "0.85")]
    [InlineData("fiber-taper", "0.1")]
    [InlineData("fiber-shadow-depth", "0")]
    [InlineData("fiber-fray", "0.05")]
    [InlineData("vortex-centers", "1")]
    [InlineData("ink-flecks", "0")]
    [InlineData("palette", "crimson")]
    public void MicroControlsChangeRenderedPixels(string key, string value)
    {
        byte[] before = Png(Make());
        byte[] after = Png(Make(new Dictionary<string, string> { [key] = value }));
        Assert.False(before.AsSpan().SequenceEqual(after), $"Parametro senza effetto: {key}");
    }


    [Fact]
    public void R10FeaturedFibersHaveDistinctCaliberWithoutFillingWholeBand()
    {
        float fine = CrinisMicroRenderer.EffectiveFiberWidth(5.767f, .43f);
        float bold = CrinisMicroRenderer.FeaturedFiberWidth(fine, 5.767f, .43f, 1, 0);
        float middle = CrinisMicroRenderer.FeaturedFiberWidth(fine, 5.767f, .43f, 1, 3);
        float micro = CrinisMicroRenderer.FeaturedFiberWidth(fine, 5.767f, .43f, 1, 1);
        Assert.True(bold > middle && middle > micro);
        Assert.Equal(fine, micro);
        Assert.True(bold <= 5.767f);
        Assert.Equal(fine, CrinisMicroRenderer.FeaturedFiberWidth(fine, 5.767f, .43f, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CrinisMicroRenderer.FeaturedFiberWidth(fine, 5.767f, .43f, 1.1, 0));
    }

    [Fact]
    public void R10InnerOcclusionAffectsRasterWhenDepthEnabled()
    {
        var values = new Dictionary<string, string>
        {
            ["intra-bundle-depth"] = "0.8", ["featured-fibers"] = "0.8",
            ["cross-section-light"] = "0.9", ["internal-occlusion"] = "0",
            ["ink-flecks"] = "0"
        };
        byte[] unoccluded = Png(Make(values));
        values["internal-occlusion"] = "0.85";
        byte[] occluded = Png(Make(values));
        Assert.False(unoccluded.AsSpan().SequenceEqual(occluded));
        Assert.Equal(occluded, Png(Make(values)));
    }

    [Fact]
    public void R10OptionalControlsAreNeutralForPreviousR9Png()
    {
        var values = new Dictionary<string, string>
        {
            ["bundle-compression"] = "0.28", ["bundle-volume"] = "0.68",
            ["bundle-lighting"] = "0.82", ["intra-bundle-depth"] = "0",
            ["featured-fibers"] = "0", ["cross-section-light"] = "0",
            ["internal-occlusion"] = "0"
        };
        ArtworkRecipe baseline = Make(values);
        var json = System.Text.Json.Nodes.JsonNode.Parse(
            System.Text.Encoding.UTF8.GetString(ArtworkRecipeJson.Serialize(baseline, Registry)))!.AsObject();
        foreach (string key in new[] { "intra-bundle-depth", "featured-fibers", "cross-section-light", "internal-occlusion" })
            json["parameters"]!.AsObject().Remove(key);
        ArtworkRecipe old = ArtworkRecipeJson.Deserialize(json.ToJsonString(), Registry);
        foreach (string key in new[] { "intra-bundle-depth", "featured-fibers", "cross-section-light", "internal-occlusion" })
            Assert.Equal("0", old.Parameters[key]);
        Assert.Equal(Png(baseline), Png(old));
    }

    [Fact]
    public void R10StrataAreDeterministicAndAllThreeOccur()
    {
        var samples = Enumerable.Range(0, 512)
            .Select(i => CrinisMicroRenderer.FiberStratum(unchecked((ulong)i * 0x9E3779B97F4A7C15UL)))
            .ToArray();
        Assert.Equal(new[] { 0, 1, 2 }, samples.Distinct().Order().ToArray());
        Assert.All(samples, layer => Assert.InRange(layer, 0, 2));
    }

    [Fact]
    public void R10DepthIsSeededAndKeepsFullResolutionPng()
    {
        var values = new Dictionary<string, string>
        {
            ["bands"] = "14", ["samples"] = "220", ["hair-width"] = "5.767",
            ["bristle-pitch"] = "1.36", ["fiber-density"] = "1.907",
            ["body-opacity"] = "0", ["ink-flecks"] = "0",
            ["intra-bundle-depth"] = "0.85", ["featured-fibers"] = "0.82",
            ["cross-section-light"] = "0.88", ["internal-occlusion"] = "0.72"
        };
        byte[] png = Png(Make(values, seed: 7775761298886397793UL, width: 256, height: 144));
        Assert.Equal(png, Png(Make(values, seed: 7775761298886397793UL, width: 256, height: 144)));
        Assert.False(png.AsSpan().SequenceEqual(Png(Make(values, seed: 77, width: 256, height: 144))));
        using SKBitmap bitmap = SKBitmap.Decode(png) ?? throw new InvalidOperationException("PNG R10 non leggibile.");
        Assert.Equal(256, bitmap.Width);
        Assert.Equal(144, bitmap.Height);
    }

    [Fact]
    public void R10CancelledRenderLeavesStreamUnmodified()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var stream = new MemoryStream();
        var values = new Dictionary<string, string> { ["intra-bundle-depth"] = "1", ["featured-fibers"] = "1" };
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(values), stream,
            cancellationToken: cts.Token));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void DenseUserSettingsStillRespondToHairWidthAtHighDensity()
    {
        var values = new Dictionary<string, string>
        {
            ["bands"] = "31", ["samples"] = "420", ["bristle-pitch"] = "0.55",
            ["fiber-density"] = "2.798", ["hair-width"] = "1.2", ["body-opacity"] = "0",
            ["fiber-coverage"] = "0.97", ["fiber-relief"] = "1", ["depth-lighting"] = "0.947",
            ["rear-softness"] = "0.449", ["ink-flecks"] = "180"
        };
        byte[] slim = Png(Make(values, width: 256, height: 144));
        values["hair-width"] = "4.901";
        byte[] thick = Png(Make(values, width: 256, height: 144));
        Assert.False(slim.AsSpan().SequenceEqual(thick));
        Assert.Equal(thick, Png(Make(values, width: 256, height: 144)));
    }

    [Fact]
    public void DenseFibersRemainSeparatedEvenWithWideBrushSettings()
    {
        // Caso reale allegato: 2,629 px di spessore nominale contro
        // 1,36 / 1,836 px di spazio fra i fili nella ricetta densa.
        float pitch = 1.36f / 1.836f;
        float thin = CrinisMicroRenderer.EffectiveFiberWidth(2.629f, pitch * .64f);
        float thick = CrinisMicroRenderer.EffectiveFiberWidth(2.629f, pitch * 1.2f);
        Assert.True(thin > 0 && thin < pitch * .64f);
        Assert.True(thick > thin && thick < pitch * 1.2f);
        Assert.True(CrinisMicroRenderer.EffectiveFiberWidth(.5f, pitch * .64f) < thin);
    }

    [Theory]
    [InlineData(-0.35)]
    [InlineData(0)]
    [InlineData(0.7)]
    public void BundleCompressionKeepsEdgesAndFiberOrder(double compression)
    {
        Assert.Equal(-1, CrinisMicroRenderer.ShapeBundleLateral(-1, compression), 10);
        Assert.Equal(0, CrinisMicroRenderer.ShapeBundleLateral(0, compression), 10);
        Assert.Equal(1, CrinisMicroRenderer.ShapeBundleLateral(1, compression), 10);
        double previous = -2;
        for (int i = 0; i <= 400; i++)
        {
            double mapped = CrinisMicroRenderer.ShapeBundleLateral(-1 + i / 200d, compression);
            Assert.True(mapped > previous, $"Compressione {compression}: ordine invertito al punto {i}.");
            Assert.InRange(mapped, -1, 1);
            previous = mapped;
        }
    }

    [Fact]
    public void BundleEnvelopeIsBoundedAndNeutralAtZero()
    {
        foreach (double phase in new[] { 0d, 1.25, 4.8, Math.Tau })
            for (int sample = 0; sample <= 1200; sample += 7)
            {
                Assert.Equal(1d, CrinisMicroRenderer.BundleSectionScale(sample, phase, 0));
                Assert.InRange(CrinisMicroRenderer.BundleSectionScale(sample, phase, 1), .73, 1.27);
            }
        Assert.Throws<ArgumentOutOfRangeException>(() => CrinisMicroRenderer.ShapeBundleLateral(.5, -.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => CrinisMicroRenderer.ShapeBundleLateral(.5, .8));
        Assert.Throws<ArgumentOutOfRangeException>(() => CrinisMicroRenderer.BundleSectionScale(-1, 0, .8));
    }

    [Theory]
    [InlineData(-0.35)]
    [InlineData(0.7)]
    public void LocalDiameterRemainsBelowCompressedSpacing(double compression)
    {
        for (int i = 0; i <= 200; i++)
        {
            double u = -1 + i / 100d;
            double derivative = 1 - compression * (1 - 3 * u * u);
            double localSpacing = derivative * .73; // restringimento massimo a volume 1
            Assert.True(localSpacing >= .219 - 1e-12);
            float limit = (float)(localSpacing * 1.5); // copertura massima ammessa
            float result = CrinisMicroRenderer.EffectiveFiberWidth(8, limit);
            Assert.True(result > 0 && result < limit);
        }
    }

    [Fact]
    public void OldMicroRecipeDefaultsToNeutralGroupControlsAndSamePixels()
    {
        ArtworkRecipe baseline = Make(new Dictionary<string, string>
        {
            ["bundle-compression"] = "0", ["bundle-volume"] = "0", ["bundle-lighting"] = "0"
        });
        string json = System.Text.Encoding.UTF8.GetString(ArtworkRecipeJson.Serialize(baseline, Registry));
        var root = System.Text.Json.Nodes.JsonNode.Parse(json)!.AsObject();
        foreach (string key in new[] { "bundle-compression", "bundle-volume", "bundle-lighting" })
            root["parameters"]!.AsObject().Remove(key);
        ArtworkRecipe old = ArtworkRecipeJson.Deserialize(root.ToJsonString(), Registry);
        Assert.Equal("0", old.Parameters["bundle-compression"]);
        Assert.Equal("0", old.Parameters["bundle-volume"]);
        Assert.Equal("0", old.Parameters["bundle-lighting"]);
        Assert.Equal(Png(baseline), Png(old));
    }

    [Fact]
    public void GroupVolumeAndLightingMakeDenseBrushReadableAndRemainDeterministic()
    {
        var values = new Dictionary<string, string>
        {
            ["bands"] = "14", ["samples"] = "240", ["bristle-pitch"] = ".85",
            ["fiber-density"] = "2.7", ["hair-width"] = "4.4",
            ["fiber-coverage"] = "0.97", ["body-opacity"] = "0",
            ["ink-flecks"] = "0", ["bundle-compression"] = "0",
            ["bundle-volume"] = "0", ["bundle-lighting"] = "0"
        };
        byte[] before = Png(Make(values, width: 240, height: 160));
        values["bundle-compression"] = "0.38";
        values["bundle-volume"] = "0.85";
        values["bundle-lighting"] = "0.9";
        byte[] sculpted = Png(Make(values, width: 240, height: 160));
        Assert.False(before.AsSpan().SequenceEqual(sculpted));
        Assert.Equal(sculpted, Png(Make(values, width: 240, height: 160)));
    }

    [Fact]
    public void DenseUserSettingsRespondToCoverAndPigmentContrast()
    {
        var values = new Dictionary<string, string>
        {
            ["bands"] = "16", ["samples"] = "240", ["bristle-pitch"] = "1.36",
            ["fiber-density"] = "1.836", ["hair-width"] = "2.629",
            ["body-opacity"] = "0", ["pigment-contrast"] = "0.8",
            ["fiber-coverage"] = "0.3", ["ink-flecks"] = "30"
        };
        byte[] first = Png(Make(values, width: 256, height: 144));
        Assert.Equal(first, Png(Make(values, width: 256, height: 144)));
        values["fiber-coverage"] = "1.2";
        Assert.False(first.AsSpan().SequenceEqual(Png(Make(values, width: 256, height: 144))));
        values["pigment-contrast"] = "0";
        Assert.False(first.AsSpan().SequenceEqual(Png(Make(values, width: 256, height: 144))));
    }


    [Fact]
    public void DepthLightingAndRearSoftnessAlterLayeringWithoutBreakingDeterminism()
    {
        var baseline = new Dictionary<string, string>
        {
            ["bands"] = "16", ["samples"] = "240", ["body-opacity"] = "0",
            ["depth-lighting"] = "0", ["rear-softness"] = "0"
        };
        byte[] flat = Png(Make(baseline, width: 256, height: 144));
        baseline["depth-lighting"] = "0.9";
        baseline["rear-softness"] = "0.6";
        byte[] sculpted = Png(Make(baseline, width: 256, height: 144));
        Assert.False(flat.AsSpan().SequenceEqual(sculpted));
        Assert.Equal(sculpted, Png(Make(baseline, width: 256, height: 144)));
    }

    [Fact]
    public void MicroRenderingIsDeterministicAndDependsOnSeed()
    {
        byte[] png = Png(Make());
        Assert.Equal(png, Png(Make()));
        Assert.False(png.AsSpan().SequenceEqual(Png(Make(seed: 43))));
        using SKBitmap bitmap = SKBitmap.Decode(png) ?? throw new InvalidOperationException("PNG non leggibile.");
        Assert.Equal(192, bitmap.Width);
        Assert.Equal(128, bitmap.Height);
        Assert.True(bitmap.Pixels.Distinct().Count() > 3);
    }

    [Fact]
    public void MicroBudgetRejectsExtremeDensityBeforeWriting()
    {
        ArtworkRecipe recipe = Make(new Dictionary<string, string>
        {
            ["bands"] = "100", ["samples"] = "1200", ["band-width"] = "0.55",
            ["bristle-pitch"] = "0.55", ["fiber-density"] = "3"
        }, width: 2000, height: 2000);
        using var stream = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => Engine.Render(recipe, stream));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void MicroControlsSurviveJsonRoundTrip()
    {
        ArtworkRecipe recipe = Make(new Dictionary<string, string>
        {
            ["palette"] = "crimson", ["body-opacity"] = "0",
            ["ink-flecks"] = "150", ["fiber-density"] = "1.7"
        });
        ArtworkRecipe restored = ArtworkRecipeJson.Deserialize(ArtworkRecipeJson.Serialize(recipe, Registry), Registry);
        Assert.Equal(recipe.Parameters.OrderBy(x => x.Key), restored.Parameters.OrderBy(x => x.Key));
        Assert.Equal(Png(recipe), Png(restored));
    }

    [Fact]
    public void CancelledMicroRenderDoesNotWritePartialPng()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => Engine.Render(Make(), output,
            cancellationToken: cts.Token));
        Assert.Equal(0, output.Length);
    }
    [Fact]
    public async Task ExactMicroPreviewMatchesExport()
    {
        ArtworkRecipe recipe = Make(new Dictionary<string, string>
        {
            ["palette"] = "crimson", ["render-quality"] = "standard"
        });
        var coordinator = new StudioRenderCoordinator(Engine);
        ArtworkRecipe preview = StudioPreviewPolicy.CreateExactPreview(recipe, Registry);
        byte[] bitmap = await coordinator.RenderPngAsync(preview);
        using var output = new MemoryStream();
        await coordinator.RenderToStreamAsync(recipe, output);
        Assert.Equal(bitmap, output.ToArray());
    }

}
