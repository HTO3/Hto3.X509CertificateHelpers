using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hto3.X509CertificateHelpers;

namespace Hto3.X509CertificateHelpers.Test
{
    [TestClass]
    public class SignXML
    {
        [TestMethod]
        public void NormalUse()
        {
            //Arrange
            var CERTIFICATE = new X509Certificate2(Properties.Resources.certificate, "123456");
            var XML = "<?xml version=\"1.0\" encoding=\"utf-8\"?><dummyTag/>";

            //Act
            var signedXML = CERTIFICATE.SignXML(null, XML);

            //Assert
            Assert.IsTrue(signedXML.Contains("<Signature"));
        }
    }
}
