using StandardLicensingGenerator.ViewModels;
using System.IO;
using Xunit;

namespace StandardLicensingGenerator.Tests;

public class KeyPairGeneratorViewModelTests
{
    private readonly FakeDialogService _dialogs = new();

    [Fact]
    public void GenerateKeyPair_ReportsSelectedKeySize()
    {
        var vm = new KeyPairGeneratorViewModel(_dialogs);
        vm.SelectedKeySize = "3072";

        vm.GenerateKeyPairCommand.Execute(null);

        Assert.Equal("Key pair generated with 3072 bits.", vm.ResultText);
    }

    [Fact]
    public void SavePrivateKey_WithoutKeyPair_ShowsMessage()
    {
        var vm = new KeyPairGeneratorViewModel(_dialogs);

        vm.SavePrivateKeyCommand.Execute(null);

        var message = Assert.Single(_dialogs.Messages);
        Assert.Equal("Action Required", message.Caption);
    }

    [Fact]
    public void SaveKeys_EnableInsertOnlyWhenBothSaved()
    {
        var vm = new KeyPairGeneratorViewModel(_dialogs);
        vm.GenerateKeyPairCommand.Execute(null);
        Assert.False(vm.CanInsert);

        string privatePath = TempPath();
        string publicPath = TempPath();
        try
        {
            _dialogs.SaveFileResult = privatePath;
            vm.SavePrivateKeyCommand.Execute(null);
            Assert.False(vm.CanInsert);

            _dialogs.SaveFileResult = publicPath;
            vm.SavePublicKeyCommand.Execute(null);
            Assert.True(vm.CanInsert);

            Assert.True(File.Exists(privatePath));
            Assert.True(File.Exists(publicPath));
        }
        finally
        {
            File.Delete(privatePath);
            File.Delete(publicPath);
        }
    }

    [Fact]
    public void ProcessRestoredResultText_RecoversPathAndEnablesCopy()
    {
        var vm = new KeyPairGeneratorViewModel(_dialogs);
        vm.ResultText = "Private key saved to\nC:\\keys\\fake_private_key.pem";

        vm.ProcessRestoredResultText();

        Assert.Equal("C:\\keys\\fake_private_key.pem", vm.PrivateKeyPath);
        Assert.True(vm.CanCopy);
    }

    [Fact]
    public void ProcessRestoredResultText_IgnoresOtherText()
    {
        var vm = new KeyPairGeneratorViewModel(_dialogs);
        vm.ResultText = "Key pair generated with 2048 bits.";

        vm.ProcessRestoredResultText();

        Assert.Null(vm.PrivateKeyPath);
        Assert.False(vm.CanCopy);
    }

    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), $"slg-key-{Guid.NewGuid():N}.pem");
}
