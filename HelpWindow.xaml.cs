using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using StandardLicensingGenerator.UiSettings;

namespace StandardLicensingGenerator;

public partial class HelpWindow : MetroWindow
{
    private readonly WindowSettingsManager _settingsManager;
    public HelpWindow()
    {
        _settingsManager = new WindowSettingsManager(this);
        InitializeComponent();
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
}
