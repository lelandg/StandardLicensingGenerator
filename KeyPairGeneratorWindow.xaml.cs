using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using Standard.Licensing.Security.Cryptography;
using StandardLicensingGenerator.UiSettings;

namespace StandardLicensingGenerator;

public partial class KeyPairGeneratorWindow
{
    private string? _privateKeyPath;
    private string? _publicKeyPath;
    private readonly WindowSettingsManager _settingsManager;
    private const string SuccessPrefix = "Private key saved to";
    private readonly KeyGenerator? _keyGenerator;
    KeyPair? _keyPair;
    private string? _privateKey;
    private string? _publicKey;
    private string? _passPhrase;
    private bool _showPassword;

    public KeyPairGeneratorWindow()
    {
        InitializeComponent();
        _keyGenerator = KeyGenerator.Create(); 
        _settingsManager = new WindowSettingsManager(this);
        
        _privateKeyPath = null;
        _publicKeyPath = null;
        _showPassword = false;
        Task.Delay(20).ContinueWith(_ => Dispatcher.Invoke(ProcessResultText));
        KeySizeBox.SelectedIndex = 0;
        Closing += On_Closing;
        PreviewKeyDown += On_KeyDown;

        // Ensure password TextBox is hidden and PasswordBox is visible on window load
        PasswordTextBox.Visibility = Visibility.Collapsed;
        PasswordBox.Visibility = Visibility.Visible;
    }

    private void ProcessResultText()
    {
        var texts = ResultBox.Text.Split('\n');
        if (texts.Length > 1 && texts[0] == SuccessPrefix)
        {
            _privateKeyPath = texts[1];
            CopyButton.IsEnabled = true;
        }
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


    private void GenerateKeyPair_Click(object sender, RoutedEventArgs e)
    {
        int keySize = int.Parse(((System.Windows.Controls.ComboBoxItem)KeySizeBox.SelectedItem).Content.ToString()!);
        
        _keyPair = _keyGenerator?.GenerateKeyPair();
        _passPhrase = PasswordBox.Password;
        _privateKey = _keyPair?.ToEncryptedPrivateKeyString(_passPhrase);  
        _publicKey = _keyPair?.ToPublicKeyString();
        ResultBox.Text = $"Key pair generated with {keySize} bits.";
    }

    private void CheckEnableInsert()
    {
        InsertButton.IsEnabled = !string.IsNullOrEmpty(_privateKeyPath) && !string.IsNullOrEmpty(_publicKeyPath);
    }

    private void SavePrivateKey_Click(object sender, RoutedEventArgs e)
    {
        if (_keyPair == null)
        {
            MessageBox.Show("Generate a key pair first.");
            return;
        }
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PEM Key Files (*.pem)|*.pem|All files (*.*)|*.*", // Changed filter to PEM
            FileName = "private_key.pem" // Changed default filename to .pem
        };
        if (dlg.ShowDialog() == true)
        {
            File.WriteAllText(dlg.FileName, _privateKey);
            ResultBox.Text = $"{SuccessPrefix}\n{dlg.FileName}";
            _privateKeyPath = dlg.FileName;
            CheckEnableInsert();
        }
    }

    private void SavePublicKey_Click(object sender, RoutedEventArgs e)
    {
        if (_keyPair == null)
        {
            MessageBox.Show("Generate a key pair first.");
            return;
        }
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PEM Key Files (*.pem)|*.pem|All files (*.*)|*.*", // Changed filter to PEM
            FileName = "public_key.pem" // Changed default filename to .pem
        };
        if (dlg.ShowDialog() == true)
        {
            File.WriteAllText(dlg.FileName, _publicKey);

            ResultBox.Text = $"Public key saved to {dlg.FileName}";
            _publicKeyPath = dlg.FileName;
            CheckEnableInsert();
        }
    }

    public string? InsertedPrivateKeyPath { get; private set; }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        InsertedPrivateKeyPath = _privateKeyPath;
        Password = _passPhrase;
        DialogResult = true;
        Close();
    }

    public string? Password { get; set; }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_privateKeyPath != null) // should never be null, but never too careful! 
        {
            Clipboard.SetText(_privateKeyPath);
        }
    }

    private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _showPassword = !_showPassword;

        if (_showPassword)
        {
            // Make sure text box has the current password value before showing it
            if (PasswordTextBox.Text != PasswordBox.Password)
            {
                PasswordTextBox.Text = PasswordBox.Password;
            }

            // Show text box, hide password box
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordTextBox.Focus();
            PasswordTextBox.SelectionStart = PasswordTextBox.Text.Length; // Position cursor at end
        }
        else
        {
            // Make sure password box has the current text value before showing it
            if (PasswordBox.Password != PasswordTextBox.Text)
            {
                PasswordBox.Password = PasswordTextBox.Text;
            }

            // Show password box, hide text box
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Focus();
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Always keep the text box in sync with the password box
        // This ensures both fields have the value when toggling visibility
        if (PasswordTextBox.Text != PasswordBox.Password)
        {
            PasswordTextBox.Text = PasswordBox.Password;
        }

        // Update the passphrase when the password changes
        _passPhrase = PasswordBox.Password;
    }

    private void PasswordTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Always keep the password box in sync with the text box
        // This ensures both fields have the value when toggling visibility
        if (PasswordBox.Password != PasswordTextBox.Text)
        {
            PasswordBox.Password = PasswordTextBox.Text;
        }

        // Update the passphrase when the text changes
        _passPhrase = PasswordTextBox.Text;
    }
}