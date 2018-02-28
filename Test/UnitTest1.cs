using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //var r = Main.main();
            //var r = Main.MinNout();
            //var r = Main.NinMout();
            //var rr = Main.Water_Midea5();
            //var rr = Main.Water_Midea9();
            //var rrr = Main.Water_Cool_Midea9();
            //var rr = Main.Water_Heat_Midea9();
            var r = Main.Water_Midea_cir6();
            //var r = Main.Water_Midea_cir7_parallel_test();

        }
    }
}
