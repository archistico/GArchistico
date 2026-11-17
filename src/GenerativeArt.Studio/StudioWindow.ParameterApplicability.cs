using GenerativeArt.Application.Rendering;

namespace GenerativeArt.Studio;

/// <summary>
/// Applica le dipendenze dei parametri senza modificarne i valori.
/// Il testo esplicativo rimane visibile anche se Avalonia non mostra il tooltip
/// sui controlli disabilitati.
/// </summary>
public sealed partial class StudioWindow
{
    private void RefreshParameterApplicability()
    {
        if (_selected is null) return;
        foreach (var entry in _applicabilityEditors)
        {
            string? reason = StudioParameterApplicability.InactiveReason(
                _selected.Id, entry.Key, _parameters);
            var (editor, note) = entry.Value;
            bool enabled = reason is null;
            if (!enabled && _invalidNumericInputs.Contains(entry.Key) &&
                _resetEditorOnDisable.TryGetValue(entry.Key, out Action? restore))
                restore();
            editor.IsEnabled = enabled;
            note.IsVisible = !enabled;
            note.Text = reason ?? "";
        }
    }
}
