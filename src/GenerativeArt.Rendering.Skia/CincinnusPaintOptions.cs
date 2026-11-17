using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Parametri C3 per pennellate pittoriche; non ancora esposti nel catalogo Studio.</summary>
public sealed record CincinnusPaintOptions
{
    public float BristlePitch { get; init; } = 4.6f;
    public SKColor? BackgroundColor { get; init; }
    public float HairWidthRatio { get; init; } = .65f;
    public float BodyShade { get; init; } = .38f;
    public float LightX { get; init; } = -.21f;
    public float LightY { get; init; } = -.31f;
    public float WideShadowBlur { get; init; } = 60f;
    public float TightShadowBlur { get; init; } = 10f;
    public float WideShadowOpacity { get; init; } = .75f;
    public float TightShadowOpacity { get; init; } = .82f;
    public int DotCount { get; init; } = 4_000;
    public float TopToBottomTone { get; init; } = .8f;
    public float DepthTone { get; init; } = .25f;
    public float ToneBandExponent { get; init; } = 3f;

    /// <summary>Palette a otto colori, sostituibile integralmente o colore per colore.</summary>
    public IReadOnlyList<SKColor> Palette { get; init; } = Array.AsReadOnly(new[]
    {
        new SKColor(0x1A, 0x03, 0x08), new SKColor(0x3D, 0x07, 0x13),
        new SKColor(0x6E, 0x0F, 0x22), new SKColor(0xA3, 0x14, 0x2F),
        new SKColor(0xCB, 0x1B, 0x45), new SKColor(0xE0, 0x48, 0x5A),
        new SKColor(0xF0, 0x8A, 0x80), new SKColor(0xF9, 0xC9, 0xB6)
    });

    internal void Validate()
    {
        if (!float.IsFinite(BristlePitch) || BristlePitch < .2f || BristlePitch > 100)
            throw new ArgumentOutOfRangeException(nameof(BristlePitch));
        if (!float.IsFinite(HairWidthRatio) || HairWidthRatio <= 0 || HairWidthRatio > 2)
            throw new ArgumentOutOfRangeException(nameof(HairWidthRatio));
        if (!float.IsFinite(BodyShade) || BodyShade < 0 || BodyShade > 1)
            throw new ArgumentOutOfRangeException(nameof(BodyShade));
        if (!float.IsFinite(LightX) || !float.IsFinite(LightY) ||
            (double)LightX * LightX + (double)LightY * LightY > 1)
            throw new ArgumentOutOfRangeException(nameof(LightX));
        if (!float.IsFinite(WideShadowBlur) || WideShadowBlur < 0 || WideShadowBlur > 300 ||
            !float.IsFinite(TightShadowBlur) || TightShadowBlur < 0 || TightShadowBlur > 300)
            throw new ArgumentOutOfRangeException(nameof(WideShadowBlur));
        if (!float.IsFinite(WideShadowOpacity) || WideShadowOpacity is < 0 or > 1 ||
            !float.IsFinite(TightShadowOpacity) || TightShadowOpacity is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(WideShadowOpacity));
        if (DotCount is < 0 or > 20_000) throw new ArgumentOutOfRangeException(nameof(DotCount));
        if (!float.IsFinite(TopToBottomTone) || TopToBottomTone is < 0 or > 2 ||
            !float.IsFinite(DepthTone) || DepthTone is < 0 or > 2 ||
            !float.IsFinite(ToneBandExponent) || ToneBandExponent is < .1f or > 12f)
            throw new ArgumentOutOfRangeException(nameof(TopToBottomTone));
        if (Palette is null || Palette.Count is < 2 or > 32)
            throw new ArgumentException("Occorrono da 2 a 32 colori.", nameof(Palette));
    }
}
