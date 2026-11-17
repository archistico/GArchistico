using GenerativeArt.Core.Colors;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Unità in pixel logici. Blur e softness sono sigma gaussiani, non diametri.</summary>
public readonly record struct LayerEffects(double Blur = 0, double ShadowOpacity = 0,
    double ShadowX = 0, double ShadowY = 0, double ShadowSoftness = 0,
    Color32? ShadowColor = null, double? Opacity = null)
{
    public bool IsEnabled => Blur > 0 || ShadowOpacity > 0 || Opacity is < 1;
    internal void Validate(bool production = false)
    {
        int maximumSigma = production ? 256 : 128;
        if (!double.IsFinite(Blur) || Blur < 0 || Blur > maximumSigma
            || !double.IsFinite(ShadowSoftness) || ShadowSoftness < 0 || ShadowSoftness > maximumSigma
            || !double.IsFinite(ShadowX) || Math.Abs(ShadowX) > 256
            || !double.IsFinite(ShadowY) || Math.Abs(ShadowY) > 256
            || !double.IsFinite(ShadowOpacity) || ShadowOpacity is < 0 or > 1
            || (Opacity is double opacity && (!double.IsFinite(opacity) || opacity is < 0 or > 1)))
            throw new ArgumentOutOfRangeException(nameof(Blur), "Effetti di livello fuori budget.");
    }
}
