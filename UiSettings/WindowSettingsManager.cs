using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StandardLicensingGenerator.UiSettings;

public class WindowSettingsManager
{
    private readonly Window _window;
    private readonly string _filePath;
    private readonly Dictionary<string, FrameworkElement> _controls = new();

    public WindowSettingsManager(Window window, string? fileName = null)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));

        string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        Directory.CreateDirectory(folder);

        string name = fileName ?? $"{_window.GetType().Name}.json";
        _filePath = Path.Combine(folder, name);

        ScanControls();
    }

    private void ScanControls()
    {
        _controls.Clear();
        foreach (var fe in FindNamedControls(_window))
        {
            if (!_controls.ContainsKey(fe.Name))
                _controls.Add(fe.Name, fe);
        }
    }

    private static IEnumerable<FrameworkElement> FindNamedControls(DependencyObject parent)
    {
        if (parent == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement fe)
            {
                if (!string.IsNullOrEmpty(fe.Name))
                    yield return fe;
            }
            foreach (var grand in FindNamedControls(child))
                yield return grand;
        }
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
            return;
        try
        {
            string json = File.ReadAllText(_filePath);
            var settings = JsonSerializer.Deserialize<WindowSettings>(json);
            if (settings != null)
            {
                settings.ApplyTo(_window);
                if (settings.ControlValues != null)
                    ApplyControlValues(settings.ControlValues);
            }
        }
        catch
        {
            // ignore invalid files
        }
    }

    public void Save()
    {
        var settings = new WindowSettings
        {
            Top = _window.Top,
            Left = _window.Left,
            Width = _window.Width,
            Height = _window.Height,
            WindowState = _window.WindowState,
            ControlValues = GetControlValues()
        };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private Dictionary<string, string> GetControlValues()
    {
        var values = new Dictionary<string, string>();
        foreach (var kvp in _controls)
        {
            switch (kvp.Value)
            {
                case TextBox tb:
                    values[kvp.Key] = tb.Text;
                    break;
                case ComboBox cb:
                    values[kvp.Key] = cb.SelectedIndex.ToString();
                    break;
                case CheckBox chk:
                    values[kvp.Key] = chk.IsChecked?.ToString() ?? string.Empty;
                    break;
                case DatePicker dp:
                    values[kvp.Key] = dp.SelectedDate?.ToString("o") ?? string.Empty;
                    break;
            }
        }
        return values;
    }

    private void ApplyControlValues(Dictionary<string, string> values)
    {
        foreach (var kvp in values)
        {
            if (!_controls.TryGetValue(kvp.Key, out var control))
                continue;

            switch (control)
            {
                case TextBox tb:
                    tb.Text = kvp.Value;
                    break;
                case ComboBox cb:
                    if (int.TryParse(kvp.Value, out int index))
                        cb.SelectedIndex = index;
                    else
                        cb.Text = kvp.Value;
                    break;
                case CheckBox chk:
                    if (bool.TryParse(kvp.Value, out bool b))
                        chk.IsChecked = b;
                    break;
                case DatePicker dp:
                    if (DateTime.TryParse(kvp.Value, out DateTime dt))
                        dp.SelectedDate = dt;
                    break;
            }
        }
    }
}

