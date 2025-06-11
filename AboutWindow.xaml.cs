using System.Reflection;
using System.Windows;
using System.Windows.Input;
using StandardLicensingGenerator.UiSettings;

namespace StandardLicensingGenerator;

public partial class AboutWindow
{
    private readonly WindowSettingsManager _settingsManager;

    public AboutWindow()
    {
        _settingsManager = new WindowSettingsManager(this);
        InitializeComponent();

        // Get version info from the assembly
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        // Update version text fields
        if (version != null)
        {
            // Display version in format: major.minor.patch
            VersionText.Text = $"{version.Major}.{version.Minor}.{version.Build}";

            // Display build in format: major.minor.patch.revision
            BuildText.Text = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        // Add event handlers
        Closing += On_Closing;
        PreviewKeyDown += On_KeyDown;
    }

    private void On_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void On_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _settingsManager.Save();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
