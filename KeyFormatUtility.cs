using System.IO;
using System.Security.Cryptography;
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
        if (!pemKey.Contains("BEGIN RSA PRIVATE KEY"))
        {
            return pemKey;
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

        string CleanSection(string input, string header, string footer)
        {
            int start = input.IndexOf(header, StringComparison.Ordinal);
            if (start < 0) return input;

            start += header.Length;
            int end = input.IndexOf(footer, start, StringComparison.Ordinal);
            if (end < 0) return input;

            string body = input.Substring(start, end - start);
            return body.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }

        if (pemKey.Contains(pkcs8Header))
            return CleanSection(pemKey, pkcs8Header, pkcs8Footer);

        if (pemKey.Contains(rsaHeader))
            return CleanSection(pemKey, rsaHeader, rsaFooter);

        // Assume already base64
        return pemKey.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
    }
}
