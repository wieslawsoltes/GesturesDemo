using System;
using System.Threading;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;

namespace GesturesDemo;

public class PinchToZoomGestureEventArgs : RoutedEventArgs
{
    private static int _nextFreeId;

    public static int GetNextFreeId() => Interlocked.Increment(ref _nextFreeId);

    public PinchToZoomGestureEventArgs(int gestureId, double scale, Point offset, Vector velocity)
        : base(PinchToZoomGestureRecognizer.PinchToZoomGestureEvent)
    {
        GestureId = gestureId;
        Scale = scale;
        Offset = offset;
        Velocity = velocity;
    }

    public int GestureId { get; }
    public double Scale { get; }
    public Point Offset { get; }
    public Vector Velocity { get; }
}

public class PinchToZoomGestureRecognizer : StyledElement, IGestureRecognizer
{
    private IInputElement? _target;
    private IGestureRecognizerActionsDispatcher? _actions;
    private IPointer? _primaryPointer;
    private IPointer? _secondaryPointer;
    private ulong _lastPrimaryTimestamp;
    private ulong _lastSecondaryTimestamp;
    private int _gestureId;
    private double _initialDistance;
    private Point _initialPrimaryPosition;
    private Point _initialSecondaryPosition;
    private Point _initialCenter;
    private double _initialScale;
    private Point _initialOffset;
    private bool _isZoomable;

    public static readonly RoutedEvent<PinchToZoomGestureEventArgs> PinchToZoomGestureEvent =
        RoutedEvent.Register<PinchToZoomGestureEventArgs>(
            "PinchToZoomGesture", RoutingStrategies.Bubble, typeof(PinchToZoomGestureRecognizer));

    /// <summary>
    /// Defines the <see cref="IsZoomable"/> property.
    /// </summary>
    public static readonly DirectProperty<PinchToZoomGestureRecognizer, bool> IsZoomableProperty =
        AvaloniaProperty.RegisterDirect<PinchToZoomGestureRecognizer, bool>(
            nameof(IsZoomable),
            o => o.IsZoomable,
            (o, v) => o.IsZoomable = v);

    public bool IsZoomable
    {
        get => _isZoomable;
        set => SetAndRaise(IsZoomableProperty, ref _isZoomable, value);
    }

    public void Initialize(IInputElement target, IGestureRecognizerActionsDispatcher actions)
    {
        _target = target;
        _actions = actions;
    }

    public void PointerPressed(PointerPressedEventArgs e)
    {
        if (e.Pointer == _primaryPointer || e.Pointer == _secondaryPointer)
        {
            return;
        }
        if (_primaryPointer == null)
        {
            _primaryPointer = e.Pointer;
            _lastPrimaryTimestamp = e.Timestamp;
            _initialPrimaryPosition = e.GetPosition((Visual?)_target);
        }
        else if (_secondaryPointer == null)
        {
            _gestureId = PanGestureEventArgs.GetNextFreeId();
            _secondaryPointer = e.Pointer;
            _lastSecondaryTimestamp = e.Timestamp;
            _initialSecondaryPosition = e.GetPosition((Visual?)_target);
        }
        else // More than two pointers already pressed, end gesture
        {
            EndGesture();
        }
    }
        
    public void PointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Pointer == _primaryPointer)
        {
            _primaryPointer = _secondaryPointer;
            _secondaryPointer = null;
            _lastPrimaryTimestamp = _lastSecondaryTimestamp;
            _initialPrimaryPosition = _initialSecondaryPosition;
        }
        else if (e.Pointer == _secondaryPointer)
        {
            _secondaryPointer = null;
        }

        if (_primaryPointer == null)
        {
            EndGesture();
        }
    }

    public void PointerMoved(PointerEventArgs e)
    {
        if (e.Pointer == _primaryPointer)
        {
            _lastPrimaryTimestamp = e.Timestamp;
            _initialPrimaryPosition = e.GetPosition((Visual?)_target);
        }
        else if (e.Pointer == _secondaryPointer)
        {
            _lastSecondaryTimestamp = e.Timestamp;
            _initialSecondaryPosition = e.GetPosition((Visual?)_target);
        }

        if (_primaryPointer != null && _secondaryPointer != null)
        {
            if (!IsZoomable)
            {
                return;
            }

            if (_initialDistance == 0)
            {
                _initialDistance = GetDistance(_initialPrimaryPosition, _initialSecondaryPosition);
                _initialCenter = GetCenter(_initialPrimaryPosition, _initialSecondaryPosition);
                _initialScale = 1;
                _initialOffset = default;
                _actions!.Capture(_primaryPointer, this);
                _actions!.Capture(_secondaryPointer, this);
            }

            var currentPrimaryPosition = _initialPrimaryPosition;
            var currentSecondaryPosition = _initialSecondaryPosition;

            if (e.Pointer == _primaryPointer)
            {
                currentPrimaryPosition = e.GetPosition((Visual?)_target);
            }
            else if (e.Pointer == _secondaryPointer)
            {
                currentSecondaryPosition = e.GetPosition((Visual?)_target);
            }

            var currentDistance = GetDistance(currentPrimaryPosition, currentSecondaryPosition);
            var currentCenter = GetCenter(currentPrimaryPosition, currentSecondaryPosition);
            var currentScale = currentDistance / _initialDistance;
            var currentOffset = currentCenter - _initialCenter;

            var primaryTimeDelta = e.Timestamp - _lastPrimaryTimestamp;
            var secondaryTimeDelta = e.Timestamp - _lastSecondaryTimestamp;
            var primaryTimeDeltaSeconds = (double)primaryTimeDelta / TimeSpan.TicksPerSecond;
            var secondaryTimeDeltaSeconds = (double)secondaryTimeDelta / TimeSpan.TicksPerSecond;
            var primaryVelocity = (currentPrimaryPosition - _initialPrimaryPosition) / primaryTimeDeltaSeconds;
            var secondaryVelocity =
                (currentSecondaryPosition - _initialSecondaryPosition) / secondaryTimeDeltaSeconds;
            var averageVelocity = (primaryVelocity + secondaryVelocity) / 2;
            var args = new PinchToZoomGestureEventArgs(_gestureId, currentScale, currentOffset, averageVelocity);
            _target?.RaiseEvent(args);
        }
    }

    public void PointerCaptureLost(IPointer pointer)
    {
        if (pointer == _primaryPointer)
        {
            _primaryPointer = _secondaryPointer;
            _secondaryPointer = null;
            _lastPrimaryTimestamp = _lastSecondaryTimestamp;
            _initialPrimaryPosition = _initialSecondaryPosition;
        }
        else if (pointer == _secondaryPointer)
        {
            _secondaryPointer = null;
        }

        if (_primaryPointer == null)
        {
            EndGesture();
        }
    }

    private void EndGesture()
    {
        _primaryPointer = null;
        _secondaryPointer = null;
        _lastPrimaryTimestamp = 0;
        _lastSecondaryTimestamp = 0;
        _initialDistance = 0;
        _initialPrimaryPosition = default;
        _initialSecondaryPosition = default;
        _initialCenter = default;
        _initialScale = 0;
        _initialOffset = default;
    }

    private double GetDistance(Point p1, Point p2)
    {
        var dx = p1.X - p2.X;
        var dy = p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private Point GetCenter(Point p1, Point p2)
    {
        return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
    }
}
