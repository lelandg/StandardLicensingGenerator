using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

namespace StandardLicensingGenerator;

public static class KeyFormatUtility
{
    /// <summary>
    /// Converts an XML format key to PEM format
    /// </summary>
    /// <param name="xmlKey">The XML formatted key content</param>
    /// <returns>PEM formatted key</returns>
    public static string ConvertXmlToPem(string xmlKey)
    {
        try
        {
            // Validate the XML key first
            // We'll keep this method for now in case users have old XML keys they want to convert.
            // However, the main generation and usage will be PEM.
            using var rsa = RSA.Create();
            rsa.FromXmlString(xmlKey);
            var parameters = rsa.ExportParameters(xmlKey.Contains("<P>"));

            AsymmetricKeyParameter keyParameter;
            if (parameters.D != null && parameters.P != null && parameters.Q != null)
            {
                // It's a private key
                keyParameter = new RsaPrivateCrtKeyParameters(
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.Modulus),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.Exponent),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.D),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.P),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.Q),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.DP),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.DQ),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.InverseQ));
            }
            else
            {
                // It's a public key
                keyParameter = new RsaKeyParameters(
                    false,
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.Modulus),
                    new Org.BouncyCastle.Math.BigInteger(1, parameters.Exponent));
            }

            using var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(keyParameter);
            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to convert XML key to PEM: {ex.Message}", nameof(xmlKey), ex);
        }
    }

    /// <summary>
    /// Ensures the provided PEM string is in PKCS#8 format.
    /// Converts RSA PRIVATE KEY blocks to PRIVATE KEY blocks if needed.
    /// </summary>
    /// <param name="pemKey">The PEM formatted private key</param>
    /// <returns>PEM string in PKCS#8 format</returns>
    public static string NormalizePrivateKey(string pemKey)
    {
        // Check if it's already in PKCS#8 format
        if (pemKey.Contains("BEGIN PRIVATE KEY"))
        {
            return pemKey;
        }

        // If not in RSA format, try to validate it first
        if (!pemKey.Contains("BEGIN RSA PRIVATE KEY"))
        {
            try {
                using var rsa = System.Security.Cryptography.RSA.Create();
                rsa.ImportFromPem(pemKey);
                // If we got here, the key is valid but in an unexpected format
                return pemKey;
            } catch {
                // Continue with normal processing
            }
        }

        using var reader = new StringReader(pemKey);
        var pemReader = new PemReader(reader);
        var obj = pemReader.ReadObject();

        AsymmetricKeyParameter keyParameter = obj switch
        {
            AsymmetricCipherKeyPair pair => pair.Private,
            AsymmetricKeyParameter param => param,
            _ => throw new ArgumentException("Unsupported RSA private key format", nameof(pemKey))
        };

        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyParameter);

        using var stringWriter = new StringWriter();
        var pemWriter = new PemWriter(stringWriter);
        pemWriter.WriteObject(privateKeyInfo);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Extracts the Base64 encoded body from a PEM formatted key string.
    /// If the input already appears to be Base64, it is returned unchanged.
    /// </summary>
    /// <param name="pemKey">PEM formatted key</param>
    /// <returns>Base64 encoded key content</returns>
    public static string GetBase64Key(string pemKey)
    {
        const string pkcs8Header = "-----BEGIN PRIVATE KEY-----";
        const string pkcs8Footer = "-----END PRIVATE KEY-----";
        const string rsaHeader = "-----BEGIN RSA PRIVATE KEY-----";
        const string rsaFooter = "-----END RSA PRIVATE KEY-----";

        string? extractedBody = null;
        string trimmedPemKey = pemKey.Trim(); // Trim upfront

        // Normalize line endings in the input PEM key to handle mixed environments
        string normalizedPemKey = trimmedPemKey.Replace("\r\n", "\n").Replace("\r", "\n");

        if (normalizedPemKey.Contains(pkcs8Header) && normalizedPemKey.Contains(pkcs8Footer))
        {
            int startIdx = normalizedPemKey.IndexOf(pkcs8Header, StringComparison.Ordinal);
            if (startIdx != -1) {
                startIdx += pkcs8Header.Length;
                int endIdx = normalizedPemKey.IndexOf(pkcs8Footer, startIdx, StringComparison.Ordinal);
                if (endIdx != -1)
                {
                    extractedBody = normalizedPemKey.Substring(startIdx, endIdx - startIdx);
                }
            }
        }
        else if (normalizedPemKey.Contains(rsaHeader) && normalizedPemKey.Contains(rsaFooter))
        {
            int startIdx = normalizedPemKey.IndexOf(rsaHeader, StringComparison.Ordinal);
            if (startIdx != -1)
            {
                startIdx += rsaHeader.Length;
                int endIdx = normalizedPemKey.IndexOf(rsaFooter, startIdx, StringComparison.Ordinal);
                if (endIdx != -1)
                {
                    extractedBody = normalizedPemKey.Substring(startIdx, endIdx - startIdx);
                }
            }
        }

        if (extractedBody != null)
        {
            // Remove all whitespace including newlines from the extracted Base64 body
            return System.Text.RegularExpressions.Regex.Replace(extractedBody, @"\s+", "");
        }
        else
        {
            // Input is not a recognized PEM format with the specified headers.
            // Check if the input itself might be a bare Base64 string.
            string cleanedPotentialBase64 = System.Text.RegularExpressions.Regex.Replace(trimmedPemKey, @"\s+", "");
            if (IsBase64String(cleanedPotentialBase64)) {
                 return cleanedPotentialBase64;
            }
            // Otherwise, it's an unrecognized format or not a key we can directly use.
            throw new ArgumentException("The provided key is not in a recognized unencrypted PEM format (PKCS#1 or PKCS#8 private key) and does not appear to be a bare Base64 string. Please ensure the key is an unencrypted private key in PEM format or a raw Base64 string.", nameof(pemKey));
        }
    }

    private static bool IsBase64String(string s)
    {
        s = s.Trim();
        if (s.Length == 0 || s.Length % 4 != 0)
            return false;

        // Check for valid Base64 characters (A-Z, a-z, 0-9, +, /, =)
        // and that padding characters (=) are only at the end.
        int paddingCount = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsLetterOrDigit(c) || c == '+' || c == '/')
            {
                if (paddingCount > 0) return false; // char after padding
            }
            else if (c == '=')
            {
                paddingCount++;
            }
            else
            {
                return false; // Invalid character
            }
        }
        // Allow 0, 1, or 2 padding characters.
        // If there's 1 padding char, the second to last char must also be padding (invalid)
        // but this is better checked by Convert.TryFromBase64String if available.
        // For this regex-like check, we just ensure padding is at the end.
        // A more robust check for complex cases:
        if (s.EndsWith("===")) return false; // Max two padding chars
        if (s.EndsWith("==") && s.Length > 1 && s[s.Length-3] == '=') return false; // e.g. "A===" invalid

        // Attempt to convert to prove it's valid base64.
        // This is the most reliable check if .NET version supports it well.
        // For older .NET Framework, this might be less performant or have quirks.
        // Using a simpler regex-like check for characters and padding is often sufficient for filtering.
        // Let's stick to char validation and padding rules for broader compatibility here,
        // as Convert.TryFromBase64String might not be available or behave consistently in older .NET Fx.

        // Re-checking padding logic more simply:
        if (paddingCount > 2) return false;
        if (paddingCount > 0 && (s[s.Length - 1] != '=' || (paddingCount == 2 && s[s.Length - 2] != '=')))
        {
            // This check ensures padding chars are indeed at the end.
            // Example: "abc=d" would be false. "ab==" is fine. "abc=" is fine.
        }


        // Fallback to a regex for a common check, though the loop above is more precise for padding.
        // This regex allows for more than 2 padding chars if not careful with its use.
        // The loop is better.
        // return System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,2}$", System.Text.RegularExpressions.RegexOptions.None);
        // The loop above is more accurate than a simple regex for padding.

        // Final check for the loop logic:
        // Ensure all non-padding chars are valid, and padding is only at the end and max 2.
        for (int i = 0; i < s.Length - paddingCount; i++)
        {
            char c = s[i];
            if (!(char.IsLetterOrDigit(c) || c == '+' || c == '/'))
                return false;
        }
        return true;
    }
    
    public static bool IsValidPrivateKey(string pemKey)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pemKey.ToCharArray());
            return true; // valid PEM key
        }
        catch (Exception)
        {
            return false; // invalid PEM key
        }
    }
}
