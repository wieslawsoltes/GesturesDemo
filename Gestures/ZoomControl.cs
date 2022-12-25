using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Gestures;

public class ZoomControl : Border
{
    private int _panGestureId = -1;
    private int _pinchToZoomGestureId = -1;
    private Matrix _startMatrix = Matrix.Identity;

    public ZoomControl()
    {
        BackgroundProperty.OverrideDefaultValue<ZoomControl>(Brushes.Transparent);

        AddHandler(PinchToZoomGestureRecognizer.PinchToZoomGestureEvent, OnPinchToZoom);
        AddHandler(PanGestureRecognizer.PanGestureEvent, OnPan);
    }

    private void OnPinchToZoom(object? sender, PinchToZoomGestureEventArgs e)
    {
        if (Child is null)
        {
            return;
        }

        Child.RenderTransform ??= new MatrixTransform(Matrix.Identity);

        // Console.WriteLine($"[PinchToZoom] GestureId='{e.GestureId}', Scale='{e.Scale}', Offset='{e.Offset}', Velocity='{e.Velocity}', Rotation='{e.Rotation}'");

        var scale = e.Scale;
        var offset = e.Offset;
        var rotation = e.Rotation;
        var matrix = Matrix.CreateScale(scale, scale) 
                     * Matrix.CreateTranslation(offset.X, offset.Y) 
                     * Matrix.CreateRotation(rotation);

        if (_pinchToZoomGestureId != e.GestureId)
        {
            _pinchToZoomGestureId = e.GestureId;
            _startMatrix = Child.RenderTransform.Value;
        }

        Child.RenderTransform = new MatrixTransform(_startMatrix * matrix);
    }

    private void OnPan(object? sender, PanGestureEventArgs e)
    {
        if (Child is null)
        {
            return;
        }

        Child.RenderTransform ??= new MatrixTransform(Matrix.Identity);

        // Console.WriteLine($"[Pan] GestureId='{e.GestureId}', Delta='{e.Delta}', Velocity='{e.Velocity}'");

        var matrix = Matrix.CreateTranslation(e.Delta.X, e.Delta.Y);

        if (_panGestureId != e.GestureId)
        {
            _panGestureId = e.GestureId;
            _startMatrix = Child.RenderTransform.Value;
        }

        Child.RenderTransform = new MatrixTransform(_startMatrix * matrix);
    }
}
