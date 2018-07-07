﻿using EmergenceGuardian.MediaPlayerUI.Mvvm;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EmergenceGuardian.MediaPlayerUI {
    [TemplatePart(Name = PlayerUI.PART_SeekBar, Type = typeof(Slider))]
    public class PlayerUI : PlayerUIBase {
        public const string PART_SeekBar = "PART_SeekBar";
        public Slider SeekBar => GetTemplateChild(PART_SeekBar) as Slider;

        static PlayerUI() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayerUI), new FrameworkPropertyMetadata(typeof(PlayerUI)));
            FocusableProperty.OverrideMetadata(typeof(PlayerUI), new FrameworkPropertyMetadata(false));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            // ApplyTemplate can get called several times (switching full screen), attach to events only once.
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            Slider Bar = SeekBar;
            if (Bar != null) {
                MouseDown += UserControl_MouseDown;
                Bar.AddHandler(Slider.PreviewMouseDownEvent, new MouseButtonEventHandler(base.SeekBar_PreviewMouseLeftButtonDown), true);
                // Thumb doesn't yet exist.
                Bar.Loaded += (s, e) => {
                    Thumb SeekBarThumb = GetSliderThumb(Bar);
                    if (SeekBarThumb != null) {
                        SeekBarThumb.DragStarted += SeekBar_DragStarted;
                        SeekBarThumb.DragCompleted += SeekBar_DragCompleted;
                    }
                };
            }
        }

        /// <summary>
        /// Prevents the Host from receiving mouse events when clicking on controls bar.
        /// </summary>
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
        }

        protected override void OnPlayerHostChanged(DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null) {
                ((Control)e.OldValue).MouseDown -= Host_MouseDown;
                ((Control)e.OldValue).MouseWheel -= Host_MouseWheel;
                ((Control)e.OldValue).MouseDoubleClick -= Host_MouseDoubleClick;
            }
            if (e.NewValue != null) {
                ((Control)e.NewValue).MouseDown += Host_MouseDown;
                ((Control)e.NewValue).MouseWheel += Host_MouseWheel;
                ((Control)e.NewValue).MouseDoubleClick += Host_MouseDoubleClick;
            }
        }

        private void Host_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (ChangeVolumeOnMouseWheel) {
                if (e.Delta > 0)
                    PlayerHost.Volume += 5;
                else if (e.Delta < 0)
                    PlayerHost.Volume -= 5;
            }
        }

        private void Host_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            // ** Double clicks aren't working properly yet.
            HandleMouseAction(sender, e, 2);
        }

        private void Host_MouseDown(object sender, MouseButtonEventArgs e) {
            HandleMouseAction(sender, e, 1);
        }

        /// <summary>
        /// Handles mouse click events for both Host and Fullscreen.
        /// </summary>
        private void HandleMouseAction(object sender, MouseButtonEventArgs e, int clickCount) {
            bool IsFullScreen = sender is FullScreenUI;
            if (IsActionFullScreen(e, clickCount)) {
                FullScreen = !IsFullScreen; // using !FullScreen can return wrong value when exiting fullscreen
                e.Handled = true;
            } else if (IsActionPause(e, clickCount)) {
                if (PlayPauseCommand.CanExecute(null))
                    PlayPauseCommand.Execute(null);
                e.Handled = true;
            }
        }

        private bool IsActionFullScreen(MouseButtonEventArgs e, int clickCount) => IsMouseAction(MouseFullscreen, e, clickCount);
        private bool IsActionPause(MouseButtonEventArgs e, int clickCount) => IsMouseAction(MousePause, e, clickCount);

        private bool IsMouseAction(MouseTrigger a, MouseButtonEventArgs e, int clickCount) {
            if (clickCount != TriggerClickCount(a))
                return false;
            if (a == MouseTrigger.LeftClick && e.ChangedButton == MouseButton.Left)
                return true;
            if (a == MouseTrigger.MiddleClick && e.ChangedButton == MouseButton.Middle)
                return true;
            if (a == MouseTrigger.RightClick && e.ChangedButton == MouseButton.Right)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns the amount of clicks represented by specified mouse trigger.
        /// </summary>
        private int TriggerClickCount(MouseTrigger a) {
            if (a == MouseTrigger.None)
                return 0;
            else if (a == MouseTrigger.LeftClick || a == MouseTrigger.MiddleClick || a == MouseTrigger.RightClick)
                return 1;
            else
                return 2;
        }

        private Panel parentCache;
        /// <summary>
        /// Returns the container of this control the first time it is called and maintain reference to that container.
        /// </summary>
        private Panel ParentCache {
            get {
                if (parentCache == null)
                    parentCache = Parent as Panel;
                if (parentCache == null)
                    throw new ArgumentNullException("ParentCache returned null.");
                return parentCache;
            }
        }

        #region Properties

        // TitleProperty
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(bool), typeof(PlayerUI));
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        // MouseFullscreen
        public static readonly DependencyProperty MouseFullscreenProperty = DependencyProperty.Register("MouseFullscreen", typeof(MouseTrigger), typeof(PlayerUI),
            new PropertyMetadata(MouseTrigger.MiddleClick));
        public MouseTrigger MouseFullscreen { get => (MouseTrigger)GetValue(MouseFullscreenProperty); set => SetValue(MouseFullscreenProperty, value); }

        // MousePause
        public static readonly DependencyProperty MousePauseProperty = DependencyProperty.Register("MousePause", typeof(MouseTrigger), typeof(PlayerUI),
            new PropertyMetadata(MouseTrigger.LeftClick));
        public MouseTrigger MousePause { get => (MouseTrigger)GetValue(MousePauseProperty); set => SetValue(MousePauseProperty, value); }

        // ChangeVolumeOnMouseWheel
        public static readonly DependencyProperty ChangeVolumeOnMouseWheelProperty = DependencyProperty.Register("ChangeVolumeOnMouseWheel", typeof(bool), typeof(PlayerUI),
            new PropertyMetadata(true));
        public bool ChangeVolumeOnMouseWheel { get => (bool)GetValue(ChangeVolumeOnMouseWheelProperty); set => SetValue(ChangeVolumeOnMouseWheelProperty, value); }

        // IsPlayPauseVisible
        public static readonly DependencyProperty IsPlayPauseVisibleProperty = DependencyProperty.Register("IsPlayPauseVisible", typeof(bool), typeof(PlayerUI),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsPlayPauseVisible { get => (bool)GetValue(IsPlayPauseVisibleProperty); set => SetValue(IsPlayPauseVisibleProperty, value); }

        // IsStopVisible
        public static readonly DependencyProperty IsStopVisibleProperty = DependencyProperty.Register("IsStopVisible", typeof(bool), typeof(PlayerUI),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsStopVisible { get => (bool)GetValue(IsStopVisibleProperty); set => SetValue(IsStopVisibleProperty, value); }

        // IsLoopVisible
        public static readonly DependencyProperty IsLoopVisibleProperty = DependencyProperty.Register("IsLoopVisible", typeof(bool), typeof(PlayerUI),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsLoopVisible { get => (bool)GetValue(IsLoopVisibleProperty); set => SetValue(IsLoopVisibleProperty, value); }

        // IsVolumeVisible
        public static readonly DependencyProperty IsVolumeVisibleProperty = DependencyProperty.Register("IsVolumeVisible", typeof(bool), typeof(PlayerUI),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsVolumeVisible { get => (bool)GetValue(IsVolumeVisibleProperty); set => SetValue(IsVolumeVisibleProperty, value); }

        // IsSpeedVisible
        public static readonly DependencyProperty IsSpeedVisibleProperty = DependencyProperty.Register("IsSpeedVisible", typeof(bool), typeof(PlayerUI),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsSpeedVisible { get => (bool)GetValue(IsSpeedVisibleProperty); set => SetValue(IsSpeedVisibleProperty, value); }

        // IsSeekBarVisible
        public static readonly DependencyProperty IsSeekBarVisibleProperty = DependencyProperty.Register("IsSeekBarVisible", typeof(bool), typeof(PlayerUI),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsSeekBarVisible { get => (bool)GetValue(IsSeekBarVisibleProperty); set => SetValue(IsSeekBarVisibleProperty, value); }

        #endregion


        #region Fullscreen

        public FullScreenUI FullScreenUI { get; private set; }

        private ICommand toggleFullScreenCommand;
        public ICommand ToggleFullScreenCommand => CommandHelper.InitCommand(ref toggleFullScreenCommand, ToggleFullScreen, CanToggleFullScreen);

        private bool CanToggleFullScreen() => PlayerHost != null;
        private void ToggleFullScreen() {
            FullScreen = !FullScreen;
        }

        public bool FullScreen {
            get => FullScreenUI != null;
            set {
                if (value != FullScreen) {
                    if (PlayerHost == null)
                        throw new ArgumentException("PlayerHost must be set to use FullScreen");
                    if (value) {
                        // Create full screen.
                        FullScreenUI = new FullScreenUI();
                        FullScreenUI.Closed += FullScreenUI_Closed;
                        FullScreenUI.MouseDown += Host_MouseDown;
                        // Transfer key bindings.
                        InputBindingBehavior.TransferBindingsToWindow(Window.GetWindow(this), FullScreenUI, false);
                        // Transfer player.
                        TransferElement(PlayerHost.InnerControlParentCache, FullScreenUI.ContentGrid, PlayerHost.InnerControl);
                        TransferElement(ParentCache, FullScreenUI.AirspaceGrid, this);
                        FullScreenUI.Airspace.VerticalOffset = -this.ActualHeight;
                        // Show.
                        FullScreenUI.ShowDialog();
                    } else if (FullScreenUI != null) {
                        // Transfer player back.
                        TransferElement(FullScreenUI.ContentGrid, PlayerHost.InnerControlParentCache, PlayerHost.InnerControl);
                        TransferElement(FullScreenUI.AirspaceGrid, ParentCache, this);
                        // Close.
                        var F = FullScreenUI;
                        FullScreenUI = null;
                        F.CloseOnce();
                        // Activate.
                        Window.GetWindow(this).Activate();
                        this.Focus();
                    }
                }
            }
        }

        public void TransferElement(Panel src, Panel dst, FrameworkElement element) {
            src.Children.Remove(element);
            dst.Children.Add(element);
        }

        private void FullScreenUI_Closed(object sender, EventArgs e) {
            FullScreen = false;
        }

        #endregion
    }
}
