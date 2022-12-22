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
            Console.WriteLine(e);
        });
    }
}
