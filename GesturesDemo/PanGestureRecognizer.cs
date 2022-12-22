using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;

namespace Avalonia.Input
{
    public class PanGestureEventArgs : RoutedEventArgs
    {
        private static int _nextFreeId;

        public static int GetNextFreeId() => Interlocked.Increment(ref _nextFreeId);
        
        public PanGestureEventArgs(int gestureId, Vector delta, Vector velocity)
            : base(PanGestureRecognizer.PanGestureEvent)
        {
            GestureId = gestureId;
            Delta = delta;
            Velocity = velocity;
        }

        public int GestureId { get; }

        public Vector Delta { get; }

        public Vector Velocity { get; }
        
    }

    public class PanGestureRecognizer : StyledElement, IGestureRecognizer
    {
        private IInputElement? _target;
        private IGestureRecognizerActionsDispatcher? _actions;
        private Point _initialPosition;
        private IPointer? _tracking;
        private Point _lastPosition;
        private ulong? _lastMoveTimestamp;
        private int _gestureId;
        private int _requiredPointers = 3;
        private readonly HashSet<IPointer> _trackingPointers = new HashSet<IPointer>();

        public static readonly RoutedEvent<PanGestureEventArgs> PanGestureEvent =
            RoutedEvent.Register<PanGestureEventArgs>(
                "PanGestureEvent", RoutingStrategies.Bubble, typeof(PanGestureEventArgs));

        public static readonly DirectProperty<PanGestureRecognizer, int> RequiredPointersProperty =
            AvaloniaProperty.RegisterDirect<PanGestureRecognizer, int>(
                nameof(RequiredPointers),
                o => o.RequiredPointers,
                (o, v) => o.RequiredPointers = v);

        public int RequiredPointers
        {
            get => _requiredPointers;
            set => SetAndRaise(RequiredPointersProperty, ref _requiredPointers, value);
        }
        
        public void Initialize(IInputElement target, IGestureRecognizerActionsDispatcher actions)
        {
            _target = target;
            _actions = actions;
        }

        public void PointerPressed(PointerPressedEventArgs e)
        {
            if (_target != null && e.Pointer.IsPrimary &&
                (e.Pointer.Type == PointerType.Touch || e.Pointer.Type == PointerType.Pen))
            {
                if (_trackingPointers.Count < RequiredPointers)
                {
                    _trackingPointers.Add(e.Pointer);
                }

                if (_trackingPointers.Count == RequiredPointers)
                {
                    _gestureId = PanGestureEventArgs.GetNextFreeId();
                    _initialPosition = e.GetPosition((Visual?)_target);
                    _lastPosition = _initialPosition;
                    _lastMoveTimestamp = e.Timestamp;
                }
            }
        }

        public void PointerMoved(PointerEventArgs e)
        {
            if (_trackingPointers.Count == RequiredPointers && _target is Visual visual)
            {
                var currentPosition = e.GetPosition(visual);
                var elapsedTime = e.Timestamp - (_lastMoveTimestamp ?? 0);
                if (elapsedTime > 0)
                {
                    var velocity = (currentPosition - _lastPosition) / elapsedTime;
                    _lastPosition = currentPosition;
                    _lastMoveTimestamp = e.Timestamp;

                    var delta = currentPosition - _initialPosition;
                    _actions!.Capture(e.Pointer, this);
                    _target.RaiseEvent(new PanGestureEventArgs(_gestureId, delta, velocity));
                }
            }
        }

        public void PointerReleased(PointerReleasedEventArgs e)
        {
            if (_trackingPointers.Contains(e.Pointer))
            {
                _trackingPointers.Remove(e.Pointer);

                if (_trackingPointers.Count < RequiredPointers)
                {
                    EndGesture();
                }
            }
        }

        public void PointerCaptureLost(IPointer pointer)
        {
            if (_tracking == pointer)
            {
                EndGesture();
            }
        }

        private void EndGesture()
        {
            _tracking = null;
            _initialPosition = default;
            _lastPosition = default;
            _lastMoveTimestamp = null;
        }
    }
}
