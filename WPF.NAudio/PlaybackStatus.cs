﻿using System;

namespace HanumanInstitute.MediaPlayer.WPF.NAudio
{
    /// <summary>
    /// Represents the status of the player control.
    /// </summary>
    public enum PlaybackStatus
    {
        Stopped = 0,
        Loading = 1,
        Playing = 2,
        Error = 3
    }
}
