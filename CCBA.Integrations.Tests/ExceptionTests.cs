using CCBA.Integrations.Base.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CCBA.Integrations.Tests
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void ExceptionExtensions_EnsureIsNotNull()
        {
            object obj = null;

            try
            {
                obj.EnsureNotNull(nameof(obj));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                e.Message.Should().Be($"{nameof(obj)} is null");
            }
        }
    }
}