using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.Basic;

namespace Test
{
    [TestClass]
    public class TestforPython
    {
        [TestMethod]
        public void TestMethod1()
        {
            var geoInput = new GeometryInput();
            var refInput = new RefStateInput();
            var airInput = new AirStateInput();

            //ref input
            refInput.FluidName = "R32";
            refInput.Massflowrate = 0.02; //kg/s
            refInput.zh = 3;
            refInput.zdp = 3;
            airInput.za = 1;
            airInput.zdpa = 1;

            //for condenser
            refInput.tc = 45; 
            refInput.tri = 80; 
            //for evaporator
            refInput.te = 285.15 - 273.15; 
            refInput.P_exv = CoolProp.PropsSI("P", "T", refInput.tc + 273.15, "Q", 0, refInput.FluidName) / 1000;
            refInput.T_exv = refInput.tc - 8;
            refInput.H_exv = 260;

            //air input
            airInput.Volumetricflowrate = 0.12; //m3/s
            airInput.tai = 298 - 273.15;
            airInput.RHi = 0.9;

            //airInput.ha = 80;

            //geo input
            //mm
            geoInput.Pt = 21;
            geoInput.Pr = 13.37;
            geoInput.Di = 6.9;
            geoInput.Do = 7.4;
            geoInput.L = 605;
            geoInput.FPI = 20;
            geoInput.Fthickness = 0.095;
            geoInput.Nrow = 2;
            geoInput.Ntube = 13;
            geoInput.CirNum = 2;
            //var rr = Main.main_condenser_py(refInput, airInput, geoInput);
            var r = Main.main_evaporator_py(refInput, airInput, geoInput);

        }
    }
}
