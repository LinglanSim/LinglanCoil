using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.Basic;
using System.IO;

namespace Test
{
    [TestClass]
    public class TestMultiRun
    {
        [TestMethod]
        public void TestMethod1()
        {
            var geoInput = new GeometryInput();
            var refInput = new RefStateInput();
            var airInput = new AirStateInput();

            //ref input
            refInput.FluidName = "R32";
            refInput.Massflowrate = 0.01870031119662376; //kg/s

            for (int i = 0; i < 20; i++)
            {
                //for condenser
                refInput.tc = 45;
                refInput.tri = 80;
                //for evaporator
                //refInput.te = 280 - 273.15;
                //refInput.P_exv = CoolProp.PropsSI("P", "T", refInput.tc + 273.15, "Q", 0, refInput.FluidName) / 1000;
                //refInput.T_exv = refInput.tc - 8;
                //refInput.H_exv = 250;

                //air input
                airInput.Volumetricflowrate = 0.01 + i * 0.025789474; //m3/s
                airInput.tai = 25;
                airInput.RHi = 0.5;
                airInput.ha = 80;

                //geo input
                //mm
                geoInput.Pt = 21;
                geoInput.Pr = 22;
                geoInput.Di = 6.9;
                geoInput.Do = 7.4;
                geoInput.L = 842;
                geoInput.FPI = 21;
                geoInput.Fthickness = 0.095;
                geoInput.Nrow = 2;
                geoInput.Ntube = 24;
                geoInput.CirNum = 2;
                var rr = Main.main_condenser_py(refInput, airInput, geoInput);
                using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\smoothness.txt"))
                {
                    wr.WriteLine("v, {0}, href, {1}, PressD, {2}, Q, {3}, Xo, {4}, Tro, {5}, Hro, {6}", airInput.Volumetricflowrate, rr.href, rr.DP, rr.Q, rr.x_o, rr.Tro, rr.hro);
                }
            }

            //var r = Main.main_evaporator_py(refInput, airInput, geoInput);

        }
    }
}
