using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using System.IO;
using Model.Basic;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var r = Main.main();
            //var r = Main.MinNout();
            //var r = Main.NinMout();
            //var rr = Main.Water_Midea5();
            //var rr = Main.Water_Midea9();
            //var rrr = Main.Water_Cool_Midea9();
            //var rr = Main.Water_Heat_Midea9();
            using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Nlement.txt"))
            {
                wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", r.Q, r.DP, r.href, r.Ra_ratio, r.Tao, r.Tro);
                wr.WriteLine("\n");
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < 6; j++)
                        wr.WriteLine("Tao:", r.Tao_Detail[i, j]);

            }
        }
    }
}
