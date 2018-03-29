using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.Basic;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var geoInput = new GeometryInput();
            var refInput = new RefStateInput();
            var airInput = new AirStateInput();

            //ref input
            refInput.FluidName = "R410A";
            refInput.Massflowrate = 0.01;
            refInput.tc = 45;
            refInput.tri = 80;

            refInput.te = 7.2;
            refInput.P_exv = CoolProp.PropsSI("P", "T", refInput.tc + 273.15, "Q", 0, refInput.FluidName) / 1000;
            refInput.T_exv = refInput.tc - 8;

            //air input
            airInput.Volumetricflowrate = 0.28317; //m3/h
            airInput.tai = 26.67;
            airInput.RHi = 0.469;
            airInput.ha = 50;


            //geo input
            geoInput.Pt = 25.4;
            geoInput.Pr = 19.05;
            geoInput.Di = 8.4074;
            geoInput.Do = 10.0584;
            geoInput.L = 914.4;
            geoInput.FPI = 15;
            geoInput.Fthickness = 0.095;
            geoInput.Nrow = 2;
            geoInput.Ntube = 6;
            geoInput.CirNum = 2;
            var rr = Main.main_condenser_py(refInput, airInput, geoInput);
            var r = Main.main_evaporator_py(refInput, airInput, geoInput);
            //var r = Main.main_evaporator();
            //var r1 = Main.main_condenser();

            //var r = Main.Water_Midea5();
            //var rr = Main.Water_Midea9();
            //var rrr = Main.Water_Cool_Midea9();
            //var rr = Main.Water_Heat_Midea9();
            //var rr = Main.Water_Heat_Jiayong6();
            //var rr = Main.Water_Cool_Jiayong6();
            //var rr = Main.Water_Heat_Jiayong2();
            //var rr = Main.Water_Cool_Jiayong2();
            //var rr = Main.Water_Heat_Jiayong6_reverse();
            //var rr = Main.Water_Heat_Jiayong2_reverse();
            //var rr = Main.Water_Midea9_reverse();
            //var rr = Main.Water_Heat_Zhongyang1_reverse();
            //var rr = Main.Water_Heat_Jiayong6_autosplitCir_New();
            //var rr = Main.Water_Heat_Jiayong2_autosplitCir();
            //var rr = Main.Water_Heat_Zhongyang1_autosplitCir();
            //var rr = Main.Water_Heat_Zhongyang1();
            //var rr = Main.Water_Cool_Zhongyang1();
            //var rr = Main.Water_Heat_Zhongyang2();
            //var rr = Main.Water_Cool_Zhongyang2();
            //var r = Main.MinNout();
            //var r = Main.NinMout();
			//var r = Main.Water_Midea_cir6();
        }
    }
}
