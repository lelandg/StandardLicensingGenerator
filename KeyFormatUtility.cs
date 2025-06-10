using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

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
}
