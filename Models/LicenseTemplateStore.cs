using System.IO;
using System.Reflection;
using System.Text.Json;

namespace StandardLicensingGenerator.Models;

// Loads and saves the list of license templates as JSON. Default location is
// %APPDATA%\StandardLicensingGenerator\Templates.json, next to the window
// settings files.
public class LicenseTemplateStore
{
    private readonly string _filePath;

    public LicenseTemplateStore(string? filePath = null)
    {
        if (filePath != null)
        {
            _filePath = filePath;
        }
        else
        {
            string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "Templates.json");
        }
    }

    public List<LicenseTemplate> Load()
    {
        if (!File.Exists(_filePath))
            return new List<LicenseTemplate>();
        try
        {
            var templates = JsonSerializer.Deserialize<List<LicenseTemplate>>(File.ReadAllText(_filePath));
            return templates ?? new List<LicenseTemplate>();
        }
        catch
        {
            // ignore invalid files, same as WindowSettingsManager.Load
            return new List<LicenseTemplate>();
        }
    }

    public void Save(List<LicenseTemplate> templates)
    {
        var json = JsonSerializer.Serialize(templates, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    // Template names are matched case-insensitively so "Acme" and "acme"
    // cannot coexist as separate templates.
    public static LicenseTemplate? FindByName(IEnumerable<LicenseTemplate> templates, string name)
    {
        return templates.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
