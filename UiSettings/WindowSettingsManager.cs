using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace StandardLicensingGenerator.UiSettings;

public class WindowSettingsManager
{
    private readonly Window _window;
    private readonly string _filePath;

    public WindowSettingsManager(Window window, string? fileName = null)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));

        string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        Directory.CreateDirectory(folder);

        string name = fileName ?? $"{_window.GetType().Name}.json";
        _filePath = Path.Combine(folder, name);
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
            return;
        try
        {
            string json = File.ReadAllText(_filePath);
            var settings = JsonSerializer.Deserialize<WindowSettings>(json);
            settings?.ApplyTo(_window);
        }
        catch
        {
            // ignore invalid files
        }
    }

    public void Save()
    {
        var settings = WindowSettings.FromWindow(_window);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}

