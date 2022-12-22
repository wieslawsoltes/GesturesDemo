using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace GesturesDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        PanAndZoomTransformControl.AddHandler(PinchToZoomGestureRecognizer.PinchToZoomEvent, (s, e) =>
        {
            Console.WriteLine($"[PinchToZoom] GestureId='{e.GestureId}', Scale='{e.Scale}', Offset='{e.Offset}', Velocity='{e.Velocity}'");
            var scale = e.Scale;
            var offset = e.Offset;
            var matrix = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset.X, offset.Y);
            PanAndZoomTransformControl.LayoutTransform = new MatrixTransform(matrix);
        });

        PanAndZoomTransformControl.AddHandler(PanGestureRecognizer.PanGestureEvent, (s, e) =>
        {
            Console.WriteLine($"[Pan] GestureId='{e.GestureId}', Delta='{e.Delta}', Velocity='{e.Velocity}'");
            var matrix = Matrix.CreateTranslation(e.Delta.X, e.Delta.Y);
            PanAndZoomTransformControl.LayoutTransform = new MatrixTransform(matrix);
        });
    }
}
