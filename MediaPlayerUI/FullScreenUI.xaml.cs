﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace EmergenceGuardian.MediaPlayerUI {
    /// <summary>
    /// Interaction logic for FullScreenUI.xaml
    /// </summary>
    public partial class FullScreenUI : Window {
        public bool IsClosing { get; private set; }

        public FullScreenUI() {
            InitializeComponent();
        }

        public Grid ContentGrid => this.MainGrid;

        public void CloseOnce() {
            if (!IsClosing) {
                IsClosing = true;
                this.Close();
            }
        }

        private void UI_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            IsClosing = true;
        }

        private void Window_Deactivated(object sender, EventArgs e) {
            if (IsLoaded)
                CloseOnce();
        }
    }
}
