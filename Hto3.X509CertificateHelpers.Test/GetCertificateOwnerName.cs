using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hto3.X509CertificateHelpers.Test
{
    [TestClass]
    public class GetCertificateOwnerName
    {
        [TestMethod]
        public void NormalUse()
        {
            //Arrange
            var CERTIFICATE = new X509Certificate2(Properties.Resources.certificate, "123456");
            var EXPECTED_NAME = "My Full Name";

            //Act
            var name = CERTIFICATE.GetCertificateOwnerName();

            //Assert
            Assert.AreEqual(EXPECTED_NAME, name);
        }
    }
}
