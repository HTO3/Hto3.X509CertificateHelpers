using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hto3.X509CertificateHelpers.Test
{
    [TestClass]
    public class GetCertificateCPFCNPJ
    {
        [TestMethod]
        public void NormalUse()
        {
            //Arrange
            var CERTIFICATE = new X509Certificate2(Properties.Resources.certificate, "123456");
            var EXPECTED_CPF_CNPJ = "12992664000163";

            //Act
            var cpf_cnpj = CERTIFICATE.GetCertificateCPFCNPJ();

            //Assert
            Assert.AreEqual(EXPECTED_CPF_CNPJ, cpf_cnpj);
        }
    }
}
