using System.Collections.Generic;
using System.Windows;

namespace StandardLicensingGenerator.UiSettings;

public class WindowSettings
{
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public WindowState WindowState { get; set; }
    public Dictionary<string, string>? ControlValues { get; set; }

    public static WindowSettings FromWindow(Window window)
    {
        return new WindowSettings
        {
            Top = window.Top,
            Left = window.Left,
            Width = window.Width,
            Height = window.Height,
            WindowState = window.WindowState
        };
    }

    public void ApplyTo(Window window)
    {
        window.Top = Top;
        window.Left = Left;
        window.Width = Width;
        window.Height = Height;
        window.WindowState = WindowState;
    }
}

