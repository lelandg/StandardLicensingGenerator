using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Standard.Licensing;
using StandardLicensingGenerator.UiSettings;
using System.Text;
using System.Windows.Controls; // Added for StringBuilder

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
        sb.Append("<Modulus>" + Convert.ToBase64String(parameters.Modulus ?? []) + "</Modulus>");
        sb.Append("<Exponent>" + Convert.ToBase64String(parameters.Exponent ?? []) + "</Exponent>");

        if (includePrivateParameters)
        {
            sb.Append("<P>" + Convert.ToBase64String(parameters.P ?? []) + "</P>");
            sb.Append("<Q>" + Convert.ToBase64String(parameters.Q ?? []) + "</Q>");
            sb.Append("<DP>" + Convert.ToBase64String(parameters.DP ?? []) + "</DP>");
            sb.Append("<DQ>" + Convert.ToBase64String(parameters.DQ ?? []) + "</DQ>");
            sb.Append("<InverseQ>" + Convert.ToBase64String(parameters.InverseQ ?? []) + "</InverseQ>");
            sb.Append("<D>" + Convert.ToBase64String(parameters.D ?? []) + "</D>");
        }

        sb.Append("</RSAKeyValue>");
        return sb.ToString();
    }
}

public partial class MainWindow
{
    private readonly WindowSettingsManager _settingsManager;
    private string? _passPhrase;
    private bool _showPassword;

    public MainWindow()
    {
        InitializeComponent();
        LicenseTypeBox.SelectedIndex = 0;
        _settingsManager = new WindowSettingsManager(this);
        _showPassword = false;
        // Ensure the password TextBox is hidden and PasswordBox is visible on window loads
        PasswordTextBox.Visibility = Visibility.Collapsed;
        PasswordBox.Visibility = Visibility.Visible;
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

        try
        {
            string privateKeyPemString = File.ReadAllText(KeyFileBox.Text);
            string keyFormat = Path.GetExtension(KeyFileBox.Text).ToLowerInvariant();
            if (ExpirationPicker.SelectedDate == null)
            {
                MessageBox.Show("Select a valid expiration date.");
                return;
            }

            // Normalize the key using .NET functionality if needed
            if (!privateKeyPemString.Contains("BEGIN PRIVATE KEY") && privateKeyPemString.Contains("BEGIN RSA PRIVATE KEY"))
            {
                privateKeyPemString = KeyFormatUtility.NormalizePrivateKey(privateKeyPemString);
            }

            var license = License.New()  
                .WithUniqueIdentifier(Guid.NewGuid())
                .As(((ComboBoxItem)LicenseTypeBox.SelectedItem).Content.ToString() switch
                {
                    "Trial" => LicenseType.Trial,
                    "Standard" => LicenseType.Standard,
                    _ => LicenseType.Trial
                })
                .ExpiresAt((DateTime)ExpirationPicker.SelectedDate!)
                .WithMaximumUtilization(5)
                // .WithProductFeatures(new Dictionary<string, string>  
                // {  
                //     {"Sales Module", "yes"},
                //     {"Purchase Module", "yes"},  
                //     {"Maximum Transactions", "10000"}  
                // })  
                .LicensedTo(CustomerNameBox.Text, CustomerEmailBox.Text)  
                .CreateAndSignWithPrivateKey(privateKeyPemString, PasswordBox.Password);

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
                KeyFileBox.Text = keyPairWindow.InsertedPrivateKeyPath;
                PasswordBox.Password = keyPairWindow.Password ?? "";
            }
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
