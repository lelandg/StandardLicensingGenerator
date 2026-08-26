using MahApps.Metro.Controls;
using StandardLicensingGenerator.Services;
using StandardLicensingGenerator.UiSettings;
using StandardLicensingGenerator.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace StandardLicensingGenerator;

// View-only concerns for the main window: window lifetime, settings
// persistence, child windows, and clipboard. Presentation logic lives in
// MainWindowViewModel.
public partial class MainWindow : MetroWindow
{
    private readonly WindowSettingsManager _settingsManager;
    private readonly MainWindowViewModel _viewModel;
    private readonly Views.PasswordReveal _passwordReveal;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel(new DialogService(this));
        DataContext = _viewModel;
        _settingsManager = new WindowSettingsManager(this);
        _passwordReveal = new Views.PasswordReveal(
            PasswordBox, PasswordTextBox, ShowPasswordButton,
            password => _viewModel.Password = password);
        ShowPasswordButton.Content = "S_how";
        PreviewKeyDown += On_KeyDown;
        Closing += On_Closing;
    }

    private void On_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _settingsManager.Save();
    }

    private void On_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                var result = Views.CustomMessageBox.Show(
                    this,
                    "Do you want to exit the application?",
                    "Exit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes
                );

                if (result == MessageBoxResult.Yes)
                {
                    Close();
                }
                break;
            case Key.F1:
                ShowHelp_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowHelp_Click(object sender, RoutedEventArgs e)
    {
        var help = new HelpWindow { Owner = this };
        help.ShowDialog();
    }

    private void ShowAbout_Click(object sender, RoutedEventArgs e)
    {
        var about = new AboutWindow { Owner = this };
        about.ShowDialog();
    }

    private void OpenKeyPairGenerator_Click(object sender, RoutedEventArgs e)
    {
        var keyPairWindow = new KeyPairGeneratorWindow { Owner = this };
        bool? ok = keyPairWindow.ShowDialog();
        if (ok == true)
        {
            if (keyPairWindow.InsertedPrivateKeyPath != null)
            {
                _viewModel.KeyFilePath = keyPairWindow.InsertedPrivateKeyPath;
                // Setting the PasswordBox raises PasswordChanged, which syncs
                // the mirror TextBox and the ViewModel.
                PasswordBox.Password = keyPairWindow.Password ?? "";
            }
        }
    }

    private void CopyResultToClipboard_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_viewModel.ResultText);
    }

    private void LaunchProjectOnGitHub(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/lelandg/StandardLicensingGenerator",
            UseShellExecute = true
        });
    }
}
