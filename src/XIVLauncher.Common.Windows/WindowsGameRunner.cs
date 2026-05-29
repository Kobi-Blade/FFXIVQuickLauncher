using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using XIVLauncher.Common.Dalamud;
using XIVLauncher.Common.Game;
using XIVLauncher.Common.PlatformAbstractions;

namespace XIVLauncher.Common.Windows;

public class WindowsGameRunner : IGameRunner
{
    private readonly DalamudLauncher dalamudLauncher;
    private readonly bool dalamudOk;

    public WindowsGameRunner(DalamudLauncher dalamudLauncher, bool dalamudOk)
    {
        this.dalamudLauncher = dalamudLauncher;
        this.dalamudOk = dalamudOk;
    }

    public Process Start(string path, string workingDirectory, string arguments, IDictionary<string, string> environment, DpiAwareness dpiAwareness)
    {
        var compat = "RunAsInvoker ";
        compat += dpiAwareness switch
        {
            DpiAwareness.Aware => "HighDPIAware",
            DpiAwareness.Unaware => "DPIUnaware",
            _ => throw new ArgumentOutOfRangeException()
        };
        environment.Add("__COMPAT_LAYER", compat);

        if (dalamudOk)
        {
            return this.dalamudLauncher.Run(new FileInfo(path), arguments, environment);
        }

        var psi = new ProcessStartInfo
        {
            FileName = path,
            WorkingDirectory = workingDirectory,
            Arguments = arguments,
            UseShellExecute = false,
        };

        foreach (var kvp in environment)
        {
            psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }

        return Process.Start(psi)!;
    }
}
