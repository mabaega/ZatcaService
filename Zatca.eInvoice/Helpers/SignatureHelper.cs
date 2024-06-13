using System.Formats.Asn1;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

internal static class SignatureHelper
{
    internal static string GetDigitalSignature(string xmlHashing, string privateKeyContent)
    {
        byte[] buffer;
        byte[] hashBytes = Convert.FromBase64String(xmlHashing);

        privateKeyContent = privateKeyContent.Replace("\n", "").Replace("\t", "").Replace("\r", "");
        if (!privateKeyContent.Contains("-----BEGIN PRIVATE KEY-----") && !privateKeyContent.Contains("-----END PRIVATE KEY-----"))
        {
            privateKeyContent = "-----BEGIN PRIVATE KEY-----\n" + privateKeyContent + "\n-----END PRIVATE KEY-----\n";
        }

        ECParameters ecParameters = LoadECPrivateKey(privateKeyContent);

        using (ECDsa ecdsa = ECDsa.Create(ecParameters))
        {
            buffer = ecdsa.SignHash(hashBytes);
        }

        return Convert.ToBase64String(buffer);
    }

    private static ECParameters LoadECPrivateKey(string privateKeyContent)
    {
        string base64Key = privateKeyContent
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim();

        byte[] privateKeyBytes = Convert.FromBase64String(base64Key);

        AsnReader reader = new AsnReader(privateKeyBytes, AsnEncodingRules.BER);
        AsnReader sequenceReader = reader.ReadSequence();
        sequenceReader.ReadInteger(); // Skip version
        byte[] privateKey = sequenceReader.ReadOctetString();

        ECParameters ecParameters = new ECParameters();
        ecParameters.Curve = ECCurve.CreateFromOid(new Oid("1.3.132.0.10")); // OID untuk secp256k1
        ecParameters.D = privateKey;

        return ecParameters;
    }

    internal static string GetSerialNumberForCertificateObject(X509Certificate2 x509Certificate2)
    {
        sbyte[] numArray = (from x in x509Certificate2.GetSerialNumber() select (sbyte)x).ToArray();
        BigInteger integer = new((byte[])(Array)numArray);
        return integer.ToString();
    }

    internal static string GetSignedPropertiesHash(string signingTime, string digestValue, string x509IssuerName, string x509SerialNumber)
    {
        string xmlString = $@"<xades:SignedProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" Id=""xadesSignedProperties"">
                                    <xades:SignedSignatureProperties>
                                        <xades:SigningTime>{signingTime}</xades:SigningTime>
                                        <xades:SigningCertificate>
                                            <xades:Cert>
                                                <xades:CertDigest>
                                                    <ds:DigestMethod xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
                                                    <ds:DigestValue xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">{digestValue}</ds:DigestValue>
                                                </xades:CertDigest>
                                                <xades:IssuerSerial>
                                                    <ds:X509IssuerName xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">{x509IssuerName}</ds:X509IssuerName>
                                                    <ds:X509SerialNumber xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">{x509SerialNumber}</ds:X509SerialNumber>
                                                </xades:IssuerSerial>
                                            </xades:Cert>
                                        </xades:SigningCertificate>
                                    </xades:SignedSignatureProperties>
                                </xades:SignedProperties>".Replace("\r\n", "\n");  // Normalize line endings to LF only

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(xmlString.Trim()));
        string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(hashHex));
    }

}


