using Standard.Licensing;
using Standard.Licensing.Security.Cryptography;
using Standard.Licensing.Validation;
using StandardLicensingGenerator.Models;
using StandardLicensingGenerator.ViewModels;
using System.IO;
using Xunit;

namespace StandardLicensingGenerator.Tests;

public class MainWindowViewModelTests : IDisposable
{
    private readonly FakeDialogService _dialogs = new();
    private readonly string _templatePath =
        Path.Combine(Path.GetTempPath(), $"slg-templates-{Guid.NewGuid():N}.json");
    private readonly List<string> _tempFiles = new();

    private MainWindowViewModel CreateViewModel()
    {
        return new MainWindowViewModel(_dialogs, new LicenseTemplateStore(_templatePath));
    }

    public void Dispose()
    {
        File.Delete(_templatePath);
        foreach (var file in _tempFiles)
            File.Delete(file);
    }

    private string WriteTempFile(string content)
    {
        string path = Path.Combine(Path.GetTempPath(), $"slg-test-{Guid.NewGuid():N}.pem");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    // --- License type switching -------------------------------------------

    [Fact]
    public void SwitchingToTrial_SetsTrialDefaults()
    {
        var vm = CreateViewModel();
        vm.LicenseType = "Trial";

        Assert.Equal("Trial User", vm.CustomerName);
        Assert.Equal("trial@example.com", vm.CustomerEmail);
        Assert.Contains("TrialMode", vm.AttributesJson);
        Assert.NotNull(vm.ExpirationDate);
    }

    [Fact]
    public void SwitchingToTrialAndBack_RestoresStandardValues()
    {
        var vm = CreateViewModel();
        vm.CustomerName = "Alice";
        vm.CustomerEmail = "alice@example.com";
        vm.AttributesJson = "{\"Seats\": \"5\"}";
        var expiration = DateTime.Today.AddDays(90);
        vm.ExpirationDate = expiration;

        vm.LicenseType = "Trial";
        vm.LicenseType = "Standard";

        Assert.Equal("Alice", vm.CustomerName);
        Assert.Equal("alice@example.com", vm.CustomerEmail);
        Assert.Equal("{\"Seats\": \"5\"}", vm.AttributesJson);
        Assert.Equal(expiration, vm.ExpirationDate);
    }

    // --- Templates ---------------------------------------------------------

    [Fact]
    public void SaveTemplate_WithEmptyName_ShowsMessageAndSavesNothing()
    {
        var vm = CreateViewModel();
        vm.TemplateName = "  ";

        vm.SaveTemplateCommand.Execute(null);

        Assert.Single(_dialogs.Messages);
        Assert.Empty(vm.Templates);
        Assert.False(File.Exists(_templatePath));
    }

    [Fact]
    public void SaveTemplate_StoresRelativeValidityDays()
    {
        var vm = CreateViewModel();
        vm.ExpirationDate = DateTime.Today.AddDays(45);
        vm.TemplateName = "Acme";

        vm.SaveTemplateCommand.Execute(null);

        var template = Assert.Single(vm.Templates);
        Assert.Equal(45, template.ValidityDays);
    }

    [Fact]
    public void ApplyingTemplate_FillsFormAndComputesExpirationFromToday()
    {
        var vm = CreateViewModel();
        vm.ProductName = "Acme App";
        vm.Version = "2.1";
        vm.CustomerName = "Alice";
        vm.CustomerEmail = "alice@example.com";
        vm.AttributesJson = "{\"Seats\": \"5\"}";
        vm.KeyFilePath = @"C:\keys\fake_private_key.pem";
        vm.ExpirationDate = DateTime.Today.AddDays(30);
        vm.TemplateName = "Acme";
        vm.SaveTemplateCommand.Execute(null);

        // Change every field, then re-apply the template.
        vm.SelectedTemplate = null;
        vm.ProductName = "Other";
        vm.Version = "9.9";
        vm.CustomerName = "Bob";
        vm.CustomerEmail = "bob@example.com";
        vm.AttributesJson = "{}";
        vm.KeyFilePath = "";
        vm.ExpirationDate = DateTime.Today;

        vm.SelectedTemplate = vm.Templates[0];

        Assert.Equal("Acme App", vm.ProductName);
        Assert.Equal("2.1", vm.Version);
        Assert.Equal("Alice", vm.CustomerName);
        Assert.Equal("alice@example.com", vm.CustomerEmail);
        Assert.Equal("{\"Seats\": \"5\"}", vm.AttributesJson);
        Assert.Equal(@"C:\keys\fake_private_key.pem", vm.KeyFilePath);
        Assert.Equal(DateTime.Today.AddDays(30), vm.ExpirationDate);
    }

    [Fact]
    public void ApplyingTrialTemplate_TemplateValuesWinOverTrialDefaults()
    {
        var vm = CreateViewModel();
        vm.LicenseType = "Trial";
        vm.CustomerName = "Custom Trial Customer";
        vm.ExpirationDate = DateTime.Today.AddDays(14);
        vm.TemplateName = "Two Week Trial";
        vm.SaveTemplateCommand.Execute(null);

        vm.SelectedTemplate = null;
        vm.LicenseType = "Standard";
        vm.CustomerName = "Someone Else";

        vm.SelectedTemplate = vm.Templates[0];

        // The Trial auto-fill would set "Trial User"; the template must win.
        Assert.Equal("Trial", vm.LicenseType);
        Assert.Equal("Custom Trial Customer", vm.CustomerName);
        Assert.Equal(DateTime.Today.AddDays(14), vm.ExpirationDate);
    }

    [Fact]
    public void SaveTemplate_DoesNotReapplyTemplateToForm()
    {
        var vm = CreateViewModel();
        vm.CustomerName = "Alice";
        var expiration = DateTime.Now.AddDays(30); // carries a time of day
        vm.ExpirationDate = expiration;
        vm.TemplateName = "Acme";

        vm.SaveTemplateCommand.Execute(null);

        // Saving selects the template in the dropdown but must leave the form
        // untouched (re-applying would snap the expiration to midnight).
        Assert.Equal(vm.Templates[0], vm.SelectedTemplate);
        Assert.Equal("Alice", vm.CustomerName);
        Assert.Equal(expiration, vm.ExpirationDate);
    }

    [Fact]
    public void SaveTemplate_ExistingName_DeclinedOverwrite_KeepsOriginal()
    {
        var vm = CreateViewModel();
        vm.CustomerName = "Alice";
        vm.ExpirationDate = DateTime.Today.AddDays(10);
        vm.TemplateName = "Acme";
        vm.SaveTemplateCommand.Execute(null);

        vm.CustomerName = "Bob";
        _dialogs.NextMessageResult = System.Windows.MessageBoxResult.No;
        vm.SaveTemplateCommand.Execute(null);

        var template = Assert.Single(vm.Templates);
        Assert.Equal("Alice", template.CustomerName);
    }

    [Fact]
    public void SaveTemplate_ExistingName_AcceptedOverwrite_ReplacesTemplate()
    {
        var vm = CreateViewModel();
        vm.CustomerName = "Alice";
        vm.ExpirationDate = DateTime.Today.AddDays(10);
        vm.TemplateName = "Acme";
        vm.SaveTemplateCommand.Execute(null);

        vm.SelectedTemplate = null;
        vm.CustomerName = "Bob";
        vm.TemplateName = "ACME"; // case-insensitive match
        _dialogs.NextMessageResult = System.Windows.MessageBoxResult.Yes;
        vm.SaveTemplateCommand.Execute(null);

        var template = Assert.Single(vm.Templates);
        Assert.Equal("Bob", template.CustomerName);
    }

    [Fact]
    public void DeleteTemplate_Confirmed_RemovesAndPersists()
    {
        var vm = CreateViewModel();
        vm.ExpirationDate = DateTime.Today.AddDays(10);
        vm.TemplateName = "Acme";
        vm.SaveTemplateCommand.Execute(null);

        _dialogs.NextMessageResult = System.Windows.MessageBoxResult.Yes;
        vm.DeleteTemplateCommand.Execute(null);

        Assert.Empty(vm.Templates);
        var reloaded = new MainWindowViewModel(_dialogs, new LicenseTemplateStore(_templatePath));
        Assert.Empty(reloaded.Templates);
    }

    // --- Validity period ----------------------------------------------------

    [Fact]
    public void ValidityText_ComputesExpirationFromToday()
    {
        var vm = CreateViewModel();

        vm.ValidityText = "1 month";
        Assert.Equal(DateTime.Today.AddMonths(1), vm.ExpirationDate);

        vm.ValidityText = "5 years";
        Assert.Equal(DateTime.Today.AddYears(5), vm.ExpirationDate);
    }

    [Fact]
    public void ValidityText_InvalidInput_LeavesDateUnchanged()
    {
        var vm = CreateViewModel();
        var expiration = DateTime.Today.AddDays(10);
        vm.ExpirationDate = expiration;

        vm.ValidityText = "soon";

        Assert.Equal(expiration, vm.ExpirationDate);
    }

    [Fact]
    public void EditingExpirationDateDirectly_ClearsValidityText()
    {
        var vm = CreateViewModel();
        vm.ValidityText = "1 month";

        vm.ExpirationDate = DateTime.Today.AddDays(3); // manual edit

        Assert.Equal("", vm.ValidityText);
        Assert.Equal(DateTime.Today.AddDays(3), vm.ExpirationDate);
    }

    [Fact]
    public void ApplyingTemplate_ClearsValidityText()
    {
        var vm = CreateViewModel();
        vm.ExpirationDate = DateTime.Today.AddDays(30);
        vm.TemplateName = "Acme";
        vm.SaveTemplateCommand.Execute(null);

        vm.SelectedTemplate = null;
        vm.ValidityText = "5 years";
        vm.SelectedTemplate = vm.Templates[0];

        Assert.Equal("", vm.ValidityText);
        Assert.Equal(DateTime.Today.AddDays(30), vm.ExpirationDate);
    }

    // --- License generation ------------------------------------------------

    [Fact]
    public void GenerateLicense_MissingKeyFile_ShowsError()
    {
        var vm = CreateViewModel();
        vm.KeyFilePath = Path.Combine(Path.GetTempPath(), "does-not-exist.pem");

        vm.GenerateLicenseCommand.Execute(null);

        var message = Assert.Single(_dialogs.Messages);
        Assert.Equal("Error", message.Caption);
        Assert.Equal("", vm.ResultText);
    }

    [Fact]
    public void GenerateLicense_InvalidAttributeJson_ShowsWarningAndStops()
    {
        var vm = CreateViewModel();
        vm.KeyFilePath = WriteTempFile("not really a key");
        vm.AttributesJson = "{ broken";

        vm.GenerateLicenseCommand.Execute(null);

        var message = Assert.Single(_dialogs.Messages);
        Assert.Equal("Invalid Format", message.Caption);
        Assert.Equal("", vm.ResultText);
    }

    [Fact]
    public void GenerateLicense_NoExpirationDate_ShowsMessage()
    {
        var vm = CreateViewModel();
        vm.KeyFilePath = WriteTempFile("not really a key");
        vm.ExpirationDate = null;

        vm.GenerateLicenseCommand.Execute(null);

        var message = Assert.Single(_dialogs.Messages);
        Assert.Equal("Missing Information", message.Caption);
    }

    [Fact]
    public void GenerateLicense_ProducesVerifiableLicense_WithVerbatimAttributes()
    {
        var keyGenerator = KeyGenerator.Create();
        var keyPair = keyGenerator.GenerateKeyPair();
        const string passPhrase = "test-pass";

        var vm = CreateViewModel();
        vm.KeyFilePath = WriteTempFile(keyPair.ToEncryptedPrivateKeyString(passPhrase));
        vm.Password = passPhrase;
        vm.LicenseType = "Standard";
        vm.CustomerName = "Alice";
        vm.CustomerEmail = "alice@example.com";
        vm.ExpirationDate = DateTime.Today.AddDays(30);
        // Date-like strings and case-sensitive keys must survive verbatim.
        vm.AttributesJson = "{\"ExpiryHint\": \"2027-01-05\", \"seats\": \"5\", \"Seats\": \"10\"}";

        vm.GenerateLicenseCommand.Execute(null);

        Assert.Empty(_dialogs.Messages);
        Assert.NotEqual("", vm.ResultText);

        var license = License.Load(vm.ResultText);
        Assert.Empty(license.Validate()
            .Signature(keyPair.ToPublicKeyString())
            .AssertValidLicense());
        Assert.Equal(LicenseType.Standard, license.Type);
        Assert.Equal("Alice", license.Customer.Name);
        Assert.Equal("2027-01-05", license.AdditionalAttributes.Get("ExpiryHint"));
        Assert.Equal("5", license.AdditionalAttributes.Get("seats"));
        Assert.Equal("10", license.AdditionalAttributes.Get("Seats"));
    }
}
