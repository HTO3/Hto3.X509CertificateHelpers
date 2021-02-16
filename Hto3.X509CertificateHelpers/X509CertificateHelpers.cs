using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Hto3.X509CertificateHelpers
{
    /// <summary>
    /// Class containing helpers for X.509 certificates manipulation.
    /// </summary>
    public static class X509CertificateHelpers
    {
        private static X509Certificate2 LoadCertificate(Stream pfxCertificateStream, String password)
        {
            using (var memoryStream = new MemoryStream())
            {
                pfxCertificateStream.CopyTo(memoryStream, 2048);
                return new X509Certificate2(memoryStream.ToArray(), password);
            }
        }

        /// <summary>
        /// Verifies if the digital certificate password is correct.
        /// </summary>
        /// <param name="pfxCertificateStream">PFX certificate stream</param>
        /// <param name="password">PFX password</param>
        /// <returns></returns>
        public static Boolean VerifyPassword(Stream pfxCertificateStream, String password)
        {
            if (pfxCertificateStream == null)
                throw new ArgumentNullException(nameof(pfxCertificateStream));
            if (!pfxCertificateStream.CanRead)
                throw new IOException($"Cannot read the '{nameof(pfxCertificateStream)}' parameter.");
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            try
            {
                X509CertificateHelpers.LoadCertificate(pfxCertificateStream, password);
                return true;
            }
            catch (CryptographicException ex)
            {
                if (ex.Message.Contains("senha") || ex.Message.Contains("pass"))
                    return false;
                else
                    throw;
            }
        }
        /// <summary>
        /// Get the expiration date of a certificate.
        /// </summary>
        /// <param name="pfxCertificateStream">PFX certificate stream</param>
        /// <param name="password">PFX password</param>
        /// <returns></returns>
        public static DateTime GetExpirationDate(Stream pfxCertificateStream, String password)
        {
            if (pfxCertificateStream == null)
                throw new ArgumentNullException(nameof(pfxCertificateStream));
            if (!pfxCertificateStream.CanRead)
                throw new IOException($"Cannot read the '{nameof(pfxCertificateStream)}' parameter.");
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var certificate = X509CertificateHelpers.LoadCertificate(pfxCertificateStream, password);
            return certificate.NotAfter;
        }

        /// <summary>
        /// Gets the collection of valid certificates from the currently authenticated user's computer.
        /// </summary>
        /// <remarks>
        /// See more about X509Certificate2Collection at https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2collection.
        /// </remarks>
        /// <returns></returns>
        public static X509Certificate2Collection GetValidUserCertificates()
        {
            var store = new X509Store("MY", StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            return store.Certificates
                .Find(X509FindType.FindByTimeValid, DateTime.Now, false)
                .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);
        }
        /// <summary>
        /// If necessary, calls the Windows dialog box for the user to enter the digital certificate PIN code. Widely used in A3 digital certificates.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        public static void AskForPIN(this X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            X509CertificateHelpers.SignXML(certificate, null, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dummyTag/>");
        }
        /// <summary>
        /// Get the certificate owner name.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <returns></returns>
        public static String GetCertificateOwnerName(this X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            var dnsName = certificate.GetNameInfo(X509NameType.DnsName, false);

            if (dnsName.Contains(":"))
                return dnsName.Substring(0, dnsName.LastIndexOf(":"));
            else
                return dnsName;
        }
        /// <summary>
        /// Tries to get the CPF or CNPJ linked to the digital certificate, if it is not found, null will be returned.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <returns></returns>
        public static String GetCertificateCPFCNPJ(this X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            //First attempt
            {
                var match = Regex.Match(certificate.GetNameInfo(X509NameType.DnsName, false), @"(:((\d\d\d\d\d\d\d\d\d\d\d\d\d\d)|(\d\d\d\d\d\d\d\d\d\d\d)))$");

                if (match.Success)
                    return match.Value.Substring(1);
            }

            //Second attempt
            {
                var alternativeNameExtension = certificate.Extensions.Cast<X509Extension>().SingleOrDefault(c => c.Oid.FriendlyName == "Nome Alternativo Para o Requerente" || c.Oid.FriendlyName == "Subject Alternative Name");

                if (alternativeNameExtension != null)
                {
                    var match = Regex.Match(Encoding.ASCII.GetString(alternativeNameExtension.RawData), @"\?[^\?\d]+((\d\d\d\d\d\d\d\d\d\d\d\d\d\d)|(\d\d\d\d\d\d\d\d\d\d\d))\?");

                    if (match.Success)
                        return Regex.Replace(match.Value, @"[^\d]", String.Empty);
                }
            }

            return null;
        }
        /// <summary>
        /// Sign a XML with a digital certificate.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <param name="elementNameRef">XML element to sign</param>
        /// <param name="xml">XML to sign</param>
        /// <param name="attributeRef">(Optional) Reference to use in the sign process</param>
        /// <returns></returns>
        public static String SignXML(this X509Certificate2 certificate, String elementNameRef, String xml, String attributeRef = "Id")
        {
            var docXML = new XmlDocument() { PreserveWhitespace = true };
            using (XmlTextReader xtr = new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(xml))))
            {
                docXML.Load(xtr);
            }

            return X509CertificateHelpers.SignXML(certificate, elementNameRef, docXML, attributeRef);
        }
        /// <summary>
        /// Sign a XML with a digital certificate.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <param name="elementNameRef">XML element to sign</param>
        /// <param name="xmlDocument">XML document to sign</param>
        /// <param name="attributeRef">(Optional) Reference to use in the sign process</param>
        /// <returns></returns>
        public static String SignXML(this X509Certificate2 certificate, String elementNameRef, XmlDocument xmlDocument, String attributeRef = "Id")
        {
            var reference = new Reference();
            if (String.IsNullOrEmpty(elementNameRef))
                reference.Uri = String.Empty;
            else
            {
                var elementRef = xmlDocument.GetElementsByTagName(elementNameRef).Item(0);
                if (elementRef == null)
                    throw new InvalidOperationException($"Cannot sign the XML. Element '{elementNameRef}' not found.");
                reference.Uri = String.Format("#{0}", elementRef.Attributes[attributeRef].Value);
            }
            reference.DigestMethod = SignedXml.XmlDsigSHA1Url;
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform(false));
            reference.AddTransform(new XmlDsigC14NTransform(false));

            var signedXml = new SignedXml(xmlDocument);
            signedXml.SigningKey = certificate.PrivateKey;
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
            signedXml.AddReference(reference);
            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(certificate));

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));

            return xmlDocument.OuterXml;
        }
    }
}
