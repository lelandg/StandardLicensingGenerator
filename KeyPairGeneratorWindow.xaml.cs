using System;
using System.IO;
using System.Windows;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

public partial class KeyPairGeneratorWindow : Window
{
    private AsymmetricCipherKeyPair? _keyPair;
    private string? _privateKeyPath;
    private string? _publicKeyPath;

    public KeyPairGeneratorWindow()
    {
        InitializeComponent();
        KeySizeBox.SelectedIndex = 0;
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
            Filter = "Private Key (*.pem)|*.pem|All files (*.*)|*.*",
            FileName = "private_key.pem"
        };
        if (dlg.ShowDialog() == true)
        {
            using var sw = new StreamWriter(dlg.FileName);
            var pemWriter = new PemWriter(sw);
            pemWriter.WriteObject(_keyPair.Private);
            ResultBox.Text = $"Private key saved to {dlg.FileName}";
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
            Filter = "Public Key (*.pem)|*.pem|All files (*.*)|*.*",
            FileName = "public_key.pem"
        };
        if (dlg.ShowDialog() == true)
        {
            using var sw = new StreamWriter(dlg.FileName);
            var pemWriter = new PemWriter(sw);
            pemWriter.WriteObject(_keyPair.Public);
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
}
