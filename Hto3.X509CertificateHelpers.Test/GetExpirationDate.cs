using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hto3.X509CertificateHelpers.Test
{
    [TestClass]
    public class GetExpirationDate
    {
        [TestMethod]
        public void NormalUse()
        {
            //Arrange
            var PFX_CERTIFICATE_STREAM = new MemoryStream(Properties.Resources.certificate);
            var VALID_PASSWORD = "123456";
            var EXPECTED_DATE = new DateTime(2029, 7, 31, 14, 20, 41, DateTimeKind.Local);
            
            //Act
            var expirationDate = X509CertificateHelpers.GetExpirationDate(PFX_CERTIFICATE_STREAM, VALID_PASSWORD);

            //Assert
            Assert.AreEqual(EXPECTED_DATE, expirationDate);
        }
    }
}
