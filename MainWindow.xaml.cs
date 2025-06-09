using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Standard.Licensing;
using Standard.Licensing.Security.Cryptography;

namespace StandardLicensingGenerator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LicenseTypeBox.SelectedIndex = 0;
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

        var builder = License.New()
            .WithUniqueIdentifier(Guid.NewGuid())
            .LicensedTo(CustomerNameBox.Text, CustomerEmailBox.Text)
            .WithType(ParseLicenseType())
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
        var license = builder.CreateAndSignWithPrivateKey(privateKey, null);
        ResultBox.Text = license.ToString();
    }

    private LicenseType ParseLicenseType()
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
}
