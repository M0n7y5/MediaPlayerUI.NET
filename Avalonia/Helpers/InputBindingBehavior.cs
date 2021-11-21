﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace HanumanInstitute.MediaPlayer.Avalonia.Helpers
{
    public class InputBindingBehavior : AvaloniaObject
    {
        public InputBindingBehavior()
        {
            PropagateInputBindingsToWindowProperty.Changed.Subscribe(OnPropagateInputBindingsToWindowChanged);
        }

        public static readonly AttachedProperty<bool> PropagateInputBindingsToWindowProperty =
            AvaloniaProperty.RegisterAttached<InputBindingBehavior, Control, bool>("PropagateInputBindingsToWindow",
                false, false, BindingMode.OneTime);

        public static bool GetPropagateInputBindingsToWindow(AvaloniaObject d) =>
            d.CheckNotNull(nameof(d)).GetValue(PropagateInputBindingsToWindowProperty);

        public static void SetPropagateInputBindingsToWindow(AvaloniaObject d, bool value) =>
            d.CheckNotNull(nameof(d)).SetValue(PropagateInputBindingsToWindowProperty, value);

        private static void OnPropagateInputBindingsToWindowChanged(AvaloniaPropertyChangedEventArgs<bool> e)
        {
            if (!e.OldValue.GetValueOrDefault() && e.NewValue.GetValueOrDefault())
            {
                if (e.Sender is StyledElement elem)
                {
                    elem.Initialized += FrameworkElement_Initialized;
                }
            }
        }

        private static void FrameworkElement_Initialized(object? sender, EventArgs e)
        {
            var frameworkElement = (Control)sender!;
            frameworkElement.Initialized -= FrameworkElement_Initialized;

            var window = GetParentWindow(frameworkElement);
            if (window != null)
            {
                // Move input bindings from the FrameworkElement to the window.
                TransferBindingsToWindow(frameworkElement, window, true);
            }
        }

        private static Window? GetParentWindow(Interactive obj)
        {
            IStyledElement i = obj;
            while (i is not Window && i != null)
            {
                i = i.Parent;
            }

            return i as Window;
        }

        public static void TransferBindingsToWindow(IInputElement src, IInputElement dst, bool remove)
        {
            src.CheckNotNull(nameof(src));
            dst.CheckNotNull(nameof(dst));

            for (var i = src.KeyBindings.Count - 1; i >= 0; i--)
            {
                var key = src.KeyBindings[i];
                dst.KeyBindings.Add(key);
                if (remove)
                {
                    src.KeyBindings.Remove(key);
                }
            }
        }
    }
}
