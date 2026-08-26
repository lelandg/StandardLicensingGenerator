using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Standard.Licensing.Security.Cryptography;
using StandardLicensingGenerator.Services;
using System.IO;
using System.Windows;

namespace StandardLicensingGenerator.ViewModels;

// Presentation logic for KeyPairGeneratorWindow: key generation and saving.
public partial class KeyPairGeneratorViewModel : ObservableObject
{
    private const string SuccessPrefix = "Private key saved to";

    private readonly IDialogService _dialogs;
    private readonly KeyGenerator? _keyGenerator;
    private KeyPair? _keyPair;
    private string? _privateKey;
    private string? _publicKey;

    public KeyPairGeneratorViewModel(IDialogService dialogs)
    {
        _dialogs = dialogs;
        _keyGenerator = KeyGenerator.Create();
    }

    public IReadOnlyList<string> KeySizes { get; } = new[] { "2048", "3072", "4096" };

    [ObservableProperty]
    private string selectedKeySize = "2048";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string resultText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanInsert))]
    private string? privateKeyPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanInsert))]
    private string? publicKeyPath;

    // Copy is enabled only for a path restored from a previous session (the
    // result text persisted by WindowSettingsManager), matching the button's
    // "works across sessions" behavior.
    [ObservableProperty]
    private bool canCopy;

    public bool CanInsert => !string.IsNullOrEmpty(PrivateKeyPath) && !string.IsNullOrEmpty(PublicKeyPath);

    // Called shortly after startup, once window settings restored ResultText.
    public void ProcessRestoredResultText()
    {
        var texts = ResultText.Split('\n');
        if (texts.Length > 1 && texts[0] == SuccessPrefix)
        {
            PrivateKeyPath = texts[1];
            CanCopy = true;
        }
    }

    [RelayCommand]
    private void GenerateKeyPair()
    {
        int keySize = int.Parse(SelectedKeySize);

        _keyPair = _keyGenerator?.GenerateKeyPair();
        _privateKey = _keyPair?.ToEncryptedPrivateKeyString(Password);
        _publicKey = _keyPair?.ToPublicKeyString();
        ResultText = $"Key pair generated with {keySize} bits.";
    }

    [RelayCommand]
    private void SavePrivateKey()
    {
        if (_keyPair == null)
        {
            _dialogs.ShowMessage("Generate a key pair first.", "Action Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var path = _dialogs.ShowSaveFileDialog("PEM Key Files (*.pem)|*.pem|All files (*.*)|*.*", "private_key.pem");
        if (path == null)
            return;
        File.WriteAllText(path, _privateKey);
        ResultText = $"{SuccessPrefix}\n{path}";
        PrivateKeyPath = path;
    }

    [RelayCommand]
    private void SavePublicKey()
    {
        if (_keyPair == null)
        {
            _dialogs.ShowMessage("Generate a key pair first.", "Action Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var path = _dialogs.ShowSaveFileDialog("PEM Key Files (*.pem)|*.pem|All files (*.*)|*.*", "public_key.pem");
        if (path == null)
            return;
        File.WriteAllText(path, _publicKey);
        ResultText = $"Public key saved to {path}";
        PublicKeyPath = path;
    }
}
