using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GenerativeArt.Application.Rendering;

namespace GenerativeArt.Studio;

/// <summary>
/// Superficie dell'anteprima: trascinamento con pulsante sinistro e zoom della
/// rotella ancorato al cursore. Le modifiche agiscono sulla vista, non sul renderer.
/// </summary>
public sealed class PreviewViewport : Canvas
{
    private readonly StudioViewportState _view = new();
    private readonly Image _image = new()
    {
        Stretch = Stretch.Fill,
        IsHitTestVisible = false
    };
    private bool _dragging;
    private bool _autoFit = true;
    private Point _lastPointer;

    public event Action<double>? ZoomChanged;
    public double Zoom => _view.Zoom;

    public PreviewViewport()
    {
        Background = Brushes.Transparent; // L'area vuota deve poter ricevere il mouse.
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.SizeAll);
        Children.Add(_image);

        SizeChanged += (_, _) =>
        {
            _view.Resize(Bounds.Width, Bounds.Height);
            if (_autoFit && _view.FitImage()) ZoomChanged?.Invoke(_view.Zoom);
            ArrangeImage();
        };
        PointerPressed += OnPreviewPressed;
        PointerMoved += OnPreviewMoved;
        PointerReleased += OnPreviewReleased;
        PointerCaptureLost += (_, _) => EndDrag();
        PointerWheelChanged += OnPreviewWheel;
    }

    public void SetBitmap(Bitmap? bitmap)
    {
        _image.Source = bitmap;
        if (bitmap is null)
        {
            EndDrag();
            _view.SetImageSize(0, 0);
        }
        else
        {
            _view.SetImageSize(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
            if (_autoFit && _view.FitImage()) ZoomChanged?.Invoke(_view.Zoom);
        }
        ArrangeImage();
    }

    public void SetZoom(double value)
    {
        _autoFit = false;
        if (_view.ZoomAt(value, Bounds.Width / 2, Bounds.Height / 2))
        {
            ArrangeImage();
            ZoomChanged?.Invoke(_view.Zoom);
        }
    }

    public void FitImage()
    {
        _autoFit = true;
        if (_view.FitImage())
        {
            ArrangeImage();
            ZoomChanged?.Invoke(_view.Zoom);
        }
    }

    public void CenterImage()
    {
        _view.Center();
        ArrangeImage();
    }

    private void OnPreviewPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_view.HasImage || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _dragging = true;
        _lastPointer = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void OnPreviewMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            EndDrag();
            if (e.Pointer.Captured == this) e.Pointer.Capture(null);
            return;
        }
        Point current = e.GetPosition(this);
        _view.Pan(current.X - _lastPointer.X, current.Y - _lastPointer.Y);
        _lastPointer = current;
        ArrangeImage();
        e.Handled = true;
    }

    private void OnPreviewReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        EndDrag();
        if (e.Pointer.Captured == this) e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void EndDrag() => _dragging = false;

    private void OnPreviewWheel(object? sender, PointerWheelEventArgs e)
    {
        if (!_view.HasImage || e.Delta.Y == 0) return;
        Point pointer = e.GetPosition(this);
        // Una tacca = 15%; Delta frazionario supporta rotelle e trackpad precisi.
        double nextZoom = _view.Zoom * Math.Pow(1.15, e.Delta.Y);
        if (_view.ZoomAt(nextZoom, pointer.X, pointer.Y))
        {
            _autoFit = false;
            ArrangeImage();
            ZoomChanged?.Invoke(_view.Zoom);
        }
        e.Handled = true; // Non lasciare che lo scroll raggiunga i pannelli esterni.
    }

    private void ArrangeImage()
    {
        if (!_view.HasImage)
        {
            _image.Width = 0;
            _image.Height = 0;
            return;
        }
        _image.Width = _view.ScaledWidth;
        _image.Height = _view.ScaledHeight;
        SetLeft(_image, _view.OffsetX);
        SetTop(_image, _view.OffsetY);
    }
}
