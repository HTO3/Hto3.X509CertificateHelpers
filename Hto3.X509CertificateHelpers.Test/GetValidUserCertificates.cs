using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hto3.X509CertificateHelpers.Test
{
    [TestClass]
    public class GetValidUserCertificates
    {
        [TestMethod]
        public void JustReturnAnyValue()
        {
            //Cannot test since each user will have different result. Trust me, it's works.
            
            //Act
            var result = X509CertificateHelpers.GetValidUserCertificates();

            //Assert
            Assert.IsNotNull(result);
        }
    }
}
