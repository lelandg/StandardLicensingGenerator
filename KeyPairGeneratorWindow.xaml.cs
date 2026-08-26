using MahApps.Metro.Controls;
using StandardLicensingGenerator.Services;
using StandardLicensingGenerator.UiSettings;
using StandardLicensingGenerator.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StandardLicensingGenerator;

// View-only concerns for the key pair generator: window lifetime, settings
// persistence, dialog result, and clipboard. Presentation logic lives in
// KeyPairGeneratorViewModel.
public partial class KeyPairGeneratorWindow : MetroWindow
{
    private readonly WindowSettingsManager _settingsManager;
    private readonly KeyPairGeneratorViewModel _viewModel;
    private readonly Views.PasswordReveal _passwordReveal;

    public KeyPairGeneratorWindow()
    {
        InitializeComponent();
        _viewModel = new KeyPairGeneratorViewModel(new DialogService(this));
        DataContext = _viewModel;
        _settingsManager = new WindowSettingsManager(this);
        _passwordReveal = new Views.PasswordReveal(
            PasswordBox, PasswordTextBox, ShowPasswordButton,
            password => _viewModel.Password = password);

        // WindowSettingsManager restores ResultBox after Loaded; re-derive the
        // saved private-key path from the restored text shortly after startup
        // (drives the cross-session Copy-button enable).
        Task.Delay(20).ContinueWith(_ => Dispatcher.Invoke(_viewModel.ProcessRestoredResultText));
        Closing += On_Closing;
        PreviewKeyDown += On_KeyDown;
    }

    public string? InsertedPrivateKeyPath { get; private set; }

    public string? Password { get; set; }

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

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        InsertedPrivateKeyPath = _viewModel.PrivateKeyPath;
        Password = _viewModel.Password;
        DialogResult = true;
        Close();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.PrivateKeyPath != null) // should never be null, but never too careful!
        {
            Clipboard.SetText(_viewModel.PrivateKeyPath);
        }
    }
}
