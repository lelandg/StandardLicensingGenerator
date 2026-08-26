using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Standard.Licensing;
using StandardLicensingGenerator.Models;
using StandardLicensingGenerator.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace StandardLicensingGenerator.ViewModels;

// Presentation logic for MainWindow: license form state, trial defaults,
// template handling, and license generation. UI access goes through
// IDialogService so the logic is unit-testable.
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDialogService _dialogs;
    private readonly LicenseTemplateStore _templateStore;
    private readonly List<LicenseTemplate> _templates = new();

    // Store previous form values when switching license types
    private string? _previousCustomerName;
    private string? _previousCustomerEmail;
    private string? _previousAttributes;
    private DateTime? _previousExpirationDate;

    public MainWindowViewModel(IDialogService dialogs, LicenseTemplateStore? templateStore = null)
    {
        _dialogs = dialogs;
        _templateStore = templateStore ?? new LicenseTemplateStore();
        _templates.AddRange(_templateStore.Load());
        RefreshTemplates();
    }

    public IReadOnlyList<string> LicenseTypes { get; } = new[] { "Standard", "Trial" };

    public ObservableCollection<LicenseTemplate> Templates { get; } = new();

    [ObservableProperty]
    private string productName = "";

    [ObservableProperty]
    private string version = "";

    [ObservableProperty]
    private string licenseType = "Standard";

    [ObservableProperty]
    private DateTime? expirationDate;

    [ObservableProperty]
    private string customerName = "";

    [ObservableProperty]
    private string customerEmail = "";

    [ObservableProperty]
    private string attributesJson = "";

    [ObservableProperty]
    private string keyFilePath = "";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string resultText = "";

    [ObservableProperty]
    private LicenseTemplate? selectedTemplate;

    [ObservableProperty]
    private string templateName = "";

    partial void OnLicenseTypeChanged(string value)
    {
        if (value == "Trial")
        {
            // Switching to Trial - store current values
            StoreStandardLicenseValues();
            SetTrialLicenseDefaults();
        }
        else if (value == "Standard" && _previousCustomerName != null)
        {
            // Switching back to Standard - restore previous values
            RestoreStandardLicenseValues();
        }
    }

    private void StoreStandardLicenseValues()
    {
        _previousCustomerName = CustomerName;
        _previousCustomerEmail = CustomerEmail;
        _previousAttributes = AttributesJson;
        _previousExpirationDate = ExpirationDate;
    }

    private void SetTrialLicenseDefaults()
    {
        CustomerName = "Trial User";
        CustomerEmail = "trial@example.com";
        AttributesJson = "{\"TrialMode\": \"true\", \"MaxUsers\": \"1\"}";
        ExpirationDate = DateTime.Now.AddDays(30);
    }

    private void RestoreStandardLicenseValues()
    {
        if (_previousCustomerName != null)
            CustomerName = _previousCustomerName;
        if (_previousCustomerEmail != null)
            CustomerEmail = _previousCustomerEmail;
        if (_previousAttributes != null)
            AttributesJson = _previousAttributes;
        if (_previousExpirationDate.HasValue)
            ExpirationDate = _previousExpirationDate;
    }

    partial void OnSelectedTemplateChanged(LicenseTemplate? value)
    {
        if (value != null)
            ApplyTemplate(value);
    }

    private void ApplyTemplate(LicenseTemplate template)
    {
        // Set the license type first: selecting "Trial" auto-fills the
        // customer fields, and the template values must win over that.
        if (LicenseTypes.Contains(template.LicenseType))
            LicenseType = template.LicenseType;

        ProductName = template.ProductName ?? "";
        Version = template.Version ?? "";
        CustomerName = template.CustomerName ?? "";
        CustomerEmail = template.CustomerEmail ?? "";
        AttributesJson = template.AttributesJson ?? "";
        if (!string.IsNullOrEmpty(template.KeyFilePath))
            KeyFilePath = template.KeyFilePath;
        if (template.ValidityDays is int days)
            ExpirationDate = DateTime.Today.AddDays(days);
    }

    private LicenseTemplate CaptureTemplate(string name)
    {
        int? validityDays = null;
        if (ExpirationDate is DateTime expiration)
            validityDays = Math.Max(0, (expiration.Date - DateTime.Today).Days);

        return new LicenseTemplate
        {
            Name = name,
            ProductName = ProductName,
            Version = Version,
            LicenseType = LicenseType,
            ValidityDays = validityDays,
            CustomerName = CustomerName,
            CustomerEmail = CustomerEmail,
            AttributesJson = AttributesJson,
            KeyFilePath = KeyFilePath
        };
    }

    private void RefreshTemplates(string? selectName = null)
    {
        _templates.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        Templates.Clear();
        foreach (var template in _templates)
            Templates.Add(template);
        if (selectName != null)
            SelectedTemplate = LicenseTemplateStore.FindByName(_templates, selectName);
    }

    [RelayCommand]
    private void SaveTemplate()
    {
        string name = TemplateName.Trim();
        if (name.Length == 0)
        {
            _dialogs.ShowMessage("Type a name in the Template box first.", "Missing Name", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var existing = LicenseTemplateStore.FindByName(_templates, name);
        if (existing != null)
        {
            var result = _dialogs.ShowMessage(
                $"Overwrite the existing template \"{existing.Name}\"?",
                "Overwrite Template",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
                return;
            _templates.Remove(existing);
        }

        _templates.Add(CaptureTemplate(name));
        _templateStore.Save(_templates);
        RefreshTemplates(selectName: name);
    }

    [RelayCommand]
    private void DeleteTemplate()
    {
        string name = TemplateName.Trim();
        var existing = LicenseTemplateStore.FindByName(_templates, name);
        if (existing == null)
        {
            _dialogs.ShowMessage("Select or type the name of a template to delete.", "No Template", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = _dialogs.ShowMessage(
            $"Delete the template \"{existing.Name}\"?",
            "Delete Template",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);
        if (result != MessageBoxResult.Yes)
            return;

        _templates.Remove(existing);
        _templateStore.Save(_templates);
        RefreshTemplates();
        SelectedTemplate = null;
        TemplateName = "";
    }

    [RelayCommand]
    private void BrowseKey()
    {
        var path = _dialogs.ShowOpenFileDialog("XML Key Files (*.xml)|*.xml|PEM Key Files (*.pem)|*.pem|All files (*.*)|*.*");
        if (path != null)
            KeyFilePath = path;
    }

    [RelayCommand]
    private void GenerateLicense()
    {
        if (!File.Exists(KeyFilePath))
        {
            _dialogs.ShowMessage("Select a valid private key file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var attributes = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(AttributesJson))
        {
            try
            {
                JToken token;
                // DateParseHandling.None keeps date-like strings verbatim in the signed license.
                using (var reader = new JsonTextReader(new StringReader(AttributesJson)) { DateParseHandling = DateParseHandling.None })
                {
                    token = JToken.ReadFrom(reader);
                    if (reader.Read())
                        throw new JsonReaderException("Additional text found after the JSON value.");
                }

                foreach (var kv in JsonHelper.FlattenJsonToDictionary(token))
                {
                    attributes[kv.Key] = kv.Value;
                }
            }
            catch (JsonException ex)
            {
                _dialogs.ShowMessage(
                    $"Invalid JSON in additional attributes: {ex.Message}",
                    "Invalid Format",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
        }

        try
        {
            string privateKeyPemString = File.ReadAllText(KeyFilePath);
            if (ExpirationDate == null)
            {
                _dialogs.ShowMessage("Select a valid expiration date.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Normalize the key using .NET functionality if needed
            if (!privateKeyPemString.Contains("BEGIN PRIVATE KEY") && privateKeyPemString.Contains("BEGIN RSA PRIVATE KEY"))
            {
                privateKeyPemString = KeyFormatUtility.NormalizePrivateKey(privateKeyPemString);
            }

            var license = License.New()
                .WithUniqueIdentifier(Guid.NewGuid())
                .As(LicenseType switch
                {
                    "Trial" => Standard.Licensing.LicenseType.Trial,
                    "Standard" => Standard.Licensing.LicenseType.Standard,
                    _ => Standard.Licensing.LicenseType.Trial
                })
                .ExpiresAt(ExpirationDate.Value)
                .WithMaximumUtilization(5)
                .WithAdditionalAttributes(attributes)
                .LicensedTo(CustomerName, CustomerEmail)
                .CreateAndSignWithPrivateKey(privateKeyPemString, Password);

            ResultText = license.ToString();
        }
        catch (ArgumentException argEx) when (argEx.Message.Contains("Bad sequence size"))
        {
            _dialogs.ShowMessage(
                "The selected private key appears to be encrypted with a passphrase. " +
                "This tool currently supports only unencrypted keys.",
                "Unsupported Key",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            string detailedInfo = GetDetailedExceptionInfo(argEx);
            ResultText = $"Error Details:\n{detailedInfo}";
        }
        catch (Exception ex)
        {
            string errorSummary = $"Error generating license: {ex.Message}";
            _dialogs.ShowMessage(errorSummary, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Log detailed exception info for troubleshooting
            string detailedInfo = GetDetailedExceptionInfo(ex);
            ResultText = $"Error Details:\n{detailedInfo}";
        }
    }

    [RelayCommand]
    private void SaveLicense()
    {
        var path = _dialogs.ShowSaveFileDialog("License File (*.lic)|*.lic|All files (*.*)|*.*");
        if (path != null)
        {
            File.WriteAllText(path, ResultText);
        }
    }

    private static string GetDetailedExceptionInfo(Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Exception: {ex.GetType().FullName}");
        sb.AppendLine($"Message: {ex.Message}");
        sb.AppendLine($"Stack Trace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            sb.AppendLine("\nInner Exception:");
            sb.AppendLine(GetDetailedExceptionInfo(ex.InnerException));
        }

        return sb.ToString();
    }
}
