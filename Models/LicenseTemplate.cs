namespace StandardLicensingGenerator.Models;

// A named set of license form values that can be recalled to fill the form.
// The private-key password is deliberately excluded: templates are stored as
// plaintext JSON and must never contain secrets.
public class LicenseTemplate
{
    public required string Name { get; set; }
    public string? ProductName { get; set; }
    public string? Version { get; set; }
    public string LicenseType { get; set; } = "Standard";

    // Validity is stored relative to "today" so a template never carries an
    // absolute date that is already expired when it is reused.
    public int? ValidityDays { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? AttributesJson { get; set; }
    public string? KeyFilePath { get; set; }
}
