using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace GesturesDemo;

public partial class MainWindow : Window
{
    private int _panGestureId = -1;
    private int _pinchToZoomGestureId = -1;
    private Matrix _startMatrix = Matrix.Identity;

    public MainWindow()
    {
        InitializeComponent();

        if (PanAndZoomCanvas.RenderTransform is null)
        {
            PanAndZoomCanvas.RenderTransform = new MatrixTransform(Matrix.Identity);
        }
        
        PanAndZoomPanel.AddHandler(PinchToZoomGestureRecognizer.PinchToZoomEvent, (s, e) =>
        {
            Console.WriteLine($"[PinchToZoom] GestureId='{e.GestureId}', Scale='{e.Scale}', Offset='{e.Offset}', Velocity='{e.Velocity}'");
            var scale = e.Scale;
            var offset = e.Offset;
            var matrix = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset.X, offset.Y);

            if (_pinchToZoomGestureId != e.GestureId)
            {
                _pinchToZoomGestureId = e.GestureId;
                _startMatrix = PanAndZoomCanvas.RenderTransform.Value;
            }

            PanAndZoomCanvas.RenderTransform = new MatrixTransform(_startMatrix * matrix);
        });

        PanAndZoomPanel.AddHandler(PanGestureRecognizer.PanGestureEvent, (s, e) =>
        {
            Console.WriteLine($"[Pan] GestureId='{e.GestureId}', Delta='{e.Delta}', Velocity='{e.Velocity}'");

            var matrix = Matrix.CreateTranslation(e.Delta.X, e.Delta.Y);

            if (_panGestureId != e.GestureId)
            {
                _panGestureId = e.GestureId;
                _startMatrix = PanAndZoomCanvas.RenderTransform.Value;
            }

            PanAndZoomCanvas.RenderTransform = new MatrixTransform(_startMatrix * matrix);
        });
    }
}
