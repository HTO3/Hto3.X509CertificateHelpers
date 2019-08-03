using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hto3.X509CertificateHelpers.Test
{
    [TestClass]
    public class VerifyPassword
    {
        [TestMethod]
        public void ValidPassword()
        {
            //Arrange
            var PFX_CERTIFICATE_STREAM = new MemoryStream(Properties.Resources.certificate);
            var VALID_PASSWORD = "123456";

            //Act
            var isValid = X509CertificateHelpers.VerifyPassword(PFX_CERTIFICATE_STREAM, VALID_PASSWORD);

            //Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void InvalidPassword()
        {
            //Arrange
            var PFX_CERTIFICATE_STREAM = new MemoryStream(Properties.Resources.certificate);
            var INVALID_PASSWORD = "----";

            //Act
            var isValid = X509CertificateHelpers.VerifyPassword(PFX_CERTIFICATE_STREAM, INVALID_PASSWORD);

            //Assert
            Assert.IsFalse(isValid);
        }
    }
}
