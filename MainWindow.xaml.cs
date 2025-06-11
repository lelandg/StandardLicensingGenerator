using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Standard.Licensing;
using StandardLicensingGenerator.UiSettings;
using System.Text; // Added for StringBuilder

namespace StandardLicensingGenerator;

// Extension methods for XML key format compatibility
public static class RsaExtensions
{
    public static void FromXmlString(this System.Security.Cryptography.RSA rsa, string xmlString)
    {
        var parameters = new System.Security.Cryptography.RSAParameters();
        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.LoadXml(xmlString);

        if (xmlDoc.DocumentElement == null) throw new ArgumentException("Invalid XML format", nameof(xmlString));

        if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
        {
            foreach (System.Xml.XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                    case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                    case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
                    case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
                    case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
                    case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
                    case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                    case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
                }
            }
            rsa.ImportParameters(parameters);
        }
        else
        {
            throw new ArgumentException("Invalid XML format", nameof(xmlString));
        }
    }

    public static string ToXmlString(this System.Security.Cryptography.RSA rsa, bool includePrivateParameters)
    {
        var parameters = rsa.ExportParameters(includePrivateParameters);

        var sb = new StringBuilder();
        sb.Append("<RSAKeyValue>");
        sb.Append("<Modulus>" + Convert.ToBase64String(parameters.Modulus ?? Array.Empty<byte>()) + "</Modulus>");
        sb.Append("<Exponent>" + Convert.ToBase64String(parameters.Exponent ?? Array.Empty<byte>()) + "</Exponent>");

        if (includePrivateParameters)
        {
            sb.Append("<P>" + Convert.ToBase64String(parameters.P ?? Array.Empty<byte>()) + "</P>");
            sb.Append("<Q>" + Convert.ToBase64String(parameters.Q ?? Array.Empty<byte>()) + "</Q>");
            sb.Append("<DP>" + Convert.ToBase64String(parameters.DP ?? Array.Empty<byte>()) + "</DP>");
            sb.Append("<DQ>" + Convert.ToBase64String(parameters.DQ ?? Array.Empty<byte>()) + "</DQ>");
            sb.Append("<InverseQ>" + Convert.ToBase64String(parameters.InverseQ ?? Array.Empty<byte>()) + "</InverseQ>");
            sb.Append("<D>" + Convert.ToBase64String(parameters.D ?? Array.Empty<byte>()) + "</D>");
        }

        sb.Append("</RSAKeyValue>");
        return sb.ToString();
    }
}

public partial class MainWindow
{
    private readonly WindowSettingsManager _settingsManager; 
    public MainWindow()
    {
        InitializeComponent();
        LicenseTypeBox.SelectedIndex = 0;
        _settingsManager = new WindowSettingsManager(this);
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
                break;
            case Key.F1:
                ShowHelp_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private void BrowseKey_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "XML Key Files (*.xml)|*.xml|PEM Key Files (*.pem)|*.pem|All files (*.*)|*.*"
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

        var builder = License.New()
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

        try
        {
            string privateKeyPemString = File.ReadAllText(KeyFileBox.Text);
            string keyFormat = Path.GetExtension(KeyFileBox.Text).ToLowerInvariant();

            if (keyFormat == ".xml")
            {
                try
                {
                    privateKeyPemString = KeyFormatUtility.ConvertXmlToPem(privateKeyPemString);
                }
                catch (Exception xmlEx)
                {
                    MessageBox.Show($"Error processing XML key file: {xmlEx.Message}\n\nPlease ensure your XML key file is correctly formatted. You can also use a PEM key directly.",
                        "Key Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (keyFormat != ".pem")
            {
                MessageBox.Show("Unsupported key file format. Please use a .pem or .xml file.",
                    "Invalid Key Format", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            privateKeyPemString = KeyFormatUtility.NormalizePrivateKey(privateKeyPemString);

            // Use ImportFromPem to validate the key format first
            try {
                using var rsa = System.Security.Cryptography.RSA.Create();
                rsa.ImportFromPem(privateKeyPemString);
            } catch (Exception keyEx) {
                MessageBox.Show($"Private key format error: {keyEx.Message}", "Invalid Key Format", MessageBoxButton.OK, MessageBoxImage.Error);
                ResultBox.Text = $"Key Format Error:\n{privateKeyPemString}\n\nError: {keyEx.Message}";
                return;
            }

            var base64Key = KeyFormatUtility.GetBase64Key(privateKeyPemString);

            var license = builder.CreateAndSignWithPrivateKey(base64Key, null);
            ResultBox.Text = license.ToString();
        }
        catch (ArgumentException argEx) when (argEx.Message.Contains("Bad sequence size"))
        {
            MessageBox.Show(
                "The selected private key appears to be encrypted with a passphrase. " +
                "This tool currently supports only unencrypted keys.",
                "Unsupported Key",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            string detailedInfo = GetDetailedExceptionInfo(argEx);
            ResultBox.Text = $"Error Details:\n{detailedInfo}";
        }
        catch (Exception ex)
        {
            string errorSummary = $"Error generating license: {ex.Message}";
            MessageBox.Show(errorSummary, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Log detailed exception info for troubleshooting
            string detailedInfo = GetDetailedExceptionInfo(ex);
            ResultBox.Text = $"Error Details:\n{detailedInfo}";
        }
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

    private string GetDetailedExceptionInfo(Exception ex)
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

    private void CopyResultToClipboard_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(ResultBox.Text);
    }
}
