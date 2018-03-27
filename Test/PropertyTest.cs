using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Basic;

namespace Test
{
    [TestClass]
    public class PropertyTest
    {
        /*
        [TestMethod]
        public void TestMethod1()
        {
            string fluid = new string[] { "R410A.MIX" };
            double[] composition = new double[] { 1 };
            double temperature = 300;
            int phase = 2;

            var r1 = Refrigerant.SATT(fluid, composition, temperature, phase);
            var r2 = Refrigerant.CVCP(fluid, composition, temperature, r1.DensityV);
            Assert.AreEqual(1735.1590873766593, r1.Pressure, 0.1, "SATT");
        }
        [TestMethod]
        public void TestBrine()
        {
            Fluid water = new Fluid() { Name = "Water", Category = FluidCategory.Brine, Concentration = 0 };
            double temperature = 300;
            int pressure = 0;
            //SpecificVolume(Fluid fluid, double temperature, double pressure)
            var r2 = Brine.SpecificVolume(water, temperature, pressure);
            //var r1 =Refrigerant.SATT(fluid, composition, temperature, phase);
            //Assert.AreEqual(1735.1590873766593, r2.Equals, 0.1, "SATT");
            Assert.AreEqual(0.0014, r2, 0.001, "SpecificVolume");
            int curve = 1;
            var ha = AirHTC.alpha(10, 4.4, curve);
        }
        [TestMethod]
        public void TestMethod2()
        {
            string fluid = new string[] { "R410A.MIX" };
            //string fluid = new string[] { "ISOBUTAN" };
            double[] composition = new double[] { 1 };
            var mmm = Refrigerant.CRIT(fluid, composition);
        }
        */
    }
}

