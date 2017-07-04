using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LionFire.Applications.Hosting;

namespace LionFire.Trading.Tests
{
    [TestClass]
    public class NotificationTests
    {
        [TestMethod]
        public void TestMethod1()
        {

            var app = new AppHost()
                .AddTrading(accountModesAllowed: AccountMode.Test)
                .AddJsonAssetProvider()

                .Run()
                ;


        }
    }
}
