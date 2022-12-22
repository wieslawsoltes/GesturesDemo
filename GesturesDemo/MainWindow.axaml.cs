using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace GesturesDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        ZoomCanvas.AddHandler(PinchToZoomGestureRecognizer.PinchToZoomEvent, (s, e) =>
        {
            Console.WriteLine($"[PinchToZoom] GestureId='{e.GestureId}', Scale='{e.Scale}', Offset='{e.Offset}', Velocity='{e.Velocity}'");
        });

        ZoomCanvas.AddHandler(PanGestureRecognizer.PanGestureEvent, (s, e) =>
        {
            Console.WriteLine($"[Pan] GestureId='{e.GestureId}', Delta='{e.Delta}', Velocity='{e.Velocity}'");
        });
    }
}
