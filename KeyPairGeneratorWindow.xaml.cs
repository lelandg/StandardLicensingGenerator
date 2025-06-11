using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using StandardLicensingGenerator.UiSettings;
using System.Threading.Tasks;

namespace StandardLicensingGenerator;

public partial class KeyPairGeneratorWindow
{
    private AsymmetricCipherKeyPair? _keyPair;
    private string? _privateKeyPath;
    private string? _publicKeyPath;
    private readonly WindowSettingsManager _settingsManager;
    private const string SuccessPrefix = "Private key saved to";

    public KeyPairGeneratorWindow()
    {
        InitializeComponent();
        _settingsManager = new WindowSettingsManager(this);
        _privateKeyPath = null;
        _publicKeyPath = null;
        Task.Delay(20).ContinueWith(_ => Dispatcher.Invoke(ProcessResultText));
        KeySizeBox.SelectedIndex = 0;
        Closing += On_Closing;
        PreviewKeyDown += On_KeyDown;
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
        var gen = new RsaKeyPairGenerator();
        gen.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
        _keyPair = gen.GenerateKeyPair();
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
            using var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(_keyPair.Private);
            File.WriteAllText(dlg.FileName, stringWriter.ToString());

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
            using var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(_keyPair.Public);
            File.WriteAllText(dlg.FileName, stringWriter.ToString());

            ResultBox.Text = $"Public key saved to {dlg.FileName}";
            _publicKeyPath = dlg.FileName;
            CheckEnableInsert();
        }
    }

    public string? InsertedPrivateKeyPath { get; private set; }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        InsertedPrivateKeyPath = _privateKeyPath;
        DialogResult = true;
        Close();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_privateKeyPath != null) // should never be null, but never too careful! 
        {
            Clipboard.SetText(_privateKeyPath);
        }
    }
}