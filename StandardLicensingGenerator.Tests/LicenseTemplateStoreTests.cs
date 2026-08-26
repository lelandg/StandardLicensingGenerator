using StandardLicensingGenerator.Models;
using System.IO;
using Xunit;

namespace StandardLicensingGenerator.Tests;

public class LicenseTemplateStoreTests
{
    private static string TempFile() =>
        Path.Combine(Path.GetTempPath(), $"slg-templates-{Guid.NewGuid():N}.json");

    [Fact]
    public void Load_MissingFile_ReturnsEmptyList()
    {
        var store = new LicenseTemplateStore(TempFile());
        Assert.Empty(store.Load());
    }

    [Fact]
    public void Load_CorruptFile_ReturnsEmptyList()
    {
        string path = TempFile();
        try
        {
            File.WriteAllText(path, "{ not json ]");
            var store = new LicenseTemplateStore(path);
            Assert.Empty(store.Load());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTripsAllFields()
    {
        string path = TempFile();
        try
        {
            var store = new LicenseTemplateStore(path);
            var template = new LicenseTemplate
            {
                Name = "Acme Standard",
                ProductName = "Acme App",
                Version = "2.1",
                LicenseType = "Trial",
                ValidityDays = 30,
                CustomerName = "Alice",
                CustomerEmail = "alice@example.com",
                AttributesJson = "{\"Seats\": \"5\"}",
                KeyFilePath = @"C:\keys\fake_private_key.pem"
            };
            store.Save(new List<LicenseTemplate> { template });

            var loaded = Assert.Single(store.Load());
            Assert.Equal(template.Name, loaded.Name);
            Assert.Equal(template.ProductName, loaded.ProductName);
            Assert.Equal(template.Version, loaded.Version);
            Assert.Equal(template.LicenseType, loaded.LicenseType);
            Assert.Equal(template.ValidityDays, loaded.ValidityDays);
            Assert.Equal(template.CustomerName, loaded.CustomerName);
            Assert.Equal(template.CustomerEmail, loaded.CustomerEmail);
            Assert.Equal(template.AttributesJson, loaded.AttributesJson);
            Assert.Equal(template.KeyFilePath, loaded.KeyFilePath);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FindByName_IsCaseInsensitive()
    {
        var templates = new List<LicenseTemplate> { new() { Name = "Acme" } };
        Assert.NotNull(LicenseTemplateStore.FindByName(templates, "acme"));
        Assert.NotNull(LicenseTemplateStore.FindByName(templates, "ACME"));
        Assert.Null(LicenseTemplateStore.FindByName(templates, "other"));
    }
}
