using System;
using System.ComponentModel; // Required for ClosingEventArgs
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Standard.Licensing;
// using Standard.Licensing.Security.Cryptography; // This was in the original but not in the provided new code for MainWindow.xaml.cs. Assuming it's not needed for the UserSettings part.
// Add this using statement for UserSettingsManager
using StandardLicensingGenerator;

namespace StandardLicensingGenerator // Correct namespace based on previous file content
{
    public partial class MainWindow : Window
    {
        private readonly UserSettingsManager _settingsManager; // Added

        public MainWindow()
        {
            InitializeComponent();
            // Initialize UserSettingsManager for this window
            _settingsManager = new UserSettingsManager(this);

            // Load and apply settings
            LoadWindowSettings();

            LicenseTypeBox.SelectedIndex = 0; // From original constructor
            PreviewKeyDown += On_KeyDown;       // From original constructor
            this.Closing += Window_Closing;     // Register closing event handler
        }

        private void LoadWindowSettings()
        {
            // Get window state, default to Normal if not set
            string lastState = _settingsManager.GetSetting("WindowState", WindowState.Normal.ToString());
            if (Enum.TryParse(typeof(WindowState), lastState, out object? loadedState) && loadedState is WindowState)
            {
                WindowState = (WindowState)loadedState;
            }

            // Important: Only restore Top/Left/Width/Height if not maximized.
            if (WindowState == WindowState.Normal)
            {
                double lastHeight = _settingsManager.GetSetting("WindowHeight", Height);
                double lastWidth = _settingsManager.GetSetting("WindowWidth", Width);
                double lastTop = _settingsManager.GetSetting("WindowTop", Top);
                double lastLeft = _settingsManager.GetSetting("WindowLeft", Left);

                // Basic screen bounds check
                if (lastTop > SystemParameters.VirtualScreenHeight - 50) lastTop = Top;
                if (lastLeft > SystemParameters.VirtualScreenWidth - 50) lastLeft = Left;
                if (lastTop < SystemParameters.VirtualScreenTop) lastTop = SystemParameters.VirtualScreenTop;
                if (lastLeft < SystemParameters.VirtualScreenLeft) lastLeft = SystemParameters.VirtualScreenLeft;

                Height = lastHeight > 0 ? lastHeight : Height;
                Width = lastWidth > 0 ? lastWidth : Width;
                Top = lastTop;
                Left = lastLeft;
            }
        }

        // Event handler for Window Closing event
        private void Window_Closing(object? sender, CancelEventArgs? e) // Nullability fix
        {
            _settingsManager.SetSetting("WindowState", WindowState == WindowState.Minimized ? WindowState.Normal.ToString() : WindowState.ToString());

            if (WindowState == WindowState.Normal)
            {
                _settingsManager.SetSetting("WindowHeight", Height);
                _settingsManager.SetSetting("WindowWidth", Width);
                _settingsManager.SetSetting("WindowTop", Top);
                _settingsManager.SetSetting("WindowLeft", Left);
            }
            // else if you want to save RestoreBounds when maximized:
            // else if (WindowState == WindowState.Maximized)
            // {
            //     _settingsManager.SetSetting("WindowHeight", RestoreBounds.Height);
            //     _settingsManager.SetSetting("WindowWidth", RestoreBounds.Width);
            //     _settingsManager.SetSetting("WindowTop", RestoreBounds.Top);
            //     _settingsManager.SetSetting("WindowLeft", RestoreBounds.Left);
            // }

            _settingsManager.SaveSettings();
        }

        // --- Original methods from MainWindow.xaml.cs ---
        private void On_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show(
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
            }
        }

        private void BrowseKey_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "XML Key Files (*.xml)|*.xml|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                KeyFileBox.Text = dlg.FileName;
            }
        }

        private void GenerateLicense_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(KeyFileBox.Text))
            {
                MessageBox.Show("Select a valid private key file.");
                return;
            }

            var attributes = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(AttributesBox.Text))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(AttributesBox.Text);
                    if (parsed != null)
                    {
                        foreach (var kvp in parsed)
                            attributes[kvp.Key] = kvp.Value;
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid JSON in additional attributes.");
                    return;
                }
            }

            var builder = Standard.Licensing.License.New() // Ambiguity fix
                .WithUniqueIdentifier(Guid.NewGuid())
                .LicensedTo(CustomerNameBox.Text, CustomerEmailBox.Text)
                .ExpiresAt(ExpirationPicker.SelectedDate ?? DateTime.MaxValue)
                .WithProductFeatures(f =>
                {
                    f.Add("Product", ProductNameBox.Text);
                    f.Add("Version", VersionBox.Text);
                })
                .WithAdditionalAttributes(a =>
                {
                    foreach (var kvp in attributes)
                        a.Add(kvp.Key, kvp.Value);
                });

            var privateKey = File.ReadAllText(KeyFileBox.Text);
            var license = builder.CreateAndSignWithPrivateKey(privateKey, null); // Assuming null for password if not used
            ResultBox.Text = license.ToString();
        }

        private LicenseType ParseLicenseType() // This method seems unused in the original code provided, but kept for completeness if it's used by XAML or other parts.
        {
            if (LicenseTypeBox.SelectedItem is ComboBoxItem item)
            {
                var text = item.Content?.ToString();
                if (Enum.TryParse<LicenseType>(text, out var type))
                    return type;
            }
            return LicenseType.Standard;
        }

        private void SaveLicense_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "License File (*.lic)|*.lic|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, ResultBox.Text);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            var help = new HelpWindow();
            help.Owner = this;
            help.ShowDialog();
        }

        private void OpenKeyPairGenerator_Click(object sender, RoutedEventArgs e)
        {
            var keyPairWindow = new KeyPairGeneratorWindow();
            keyPairWindow.Owner = this;
            bool? ok = keyPairWindow.ShowDialog();
            if (ok == true)
            {
                if (keyPairWindow.InsertedPrivateKeyPath != null) KeyFileBox.Text = keyPairWindow.InsertedPrivateKeyPath;
            }
        }
    }
}
