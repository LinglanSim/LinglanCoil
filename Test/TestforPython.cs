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
            CapiliaryInput capInput = new CapiliaryInput();

            //ref input
            refInput.FluidName = "R32";
            //*************if input SC or Sh, Massflowrate is the initial input***************
            refInput.Massflowrate = 0.009; //kg/s
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
            geoInput.Ntube = 12;
            geoInput.CirNum = 2;

            //cap input
            capInput.d_cap = new double[] { 0.005, 0.005};//0.006
            capInput.lenth_cap = new double[] { 0.05, 0.06 };//0.5
            
            DateTime Time1 = DateTime.Now;
            var rr = Main.main_condenser_py(refInput, airInput, geoInput, capInput);
            //var r = Main.main_evaporator_py(refInput, airInput, geoInput, capInput);
            DateTime Time2 = DateTime.Now;
            double time = (Time2 - Time1).TotalSeconds;

            //double Tsc_set = 5;
            //double Tsh_set = 5;
            //var rr = Main.main_condenser_inputSC_py(Tsc_set, refInput, airInput, geoInput);
            //var rrr = Main.main_evaporator_inputSH_py(Tsh_set, refInput, airInput, geoInput);

        }
    }
}
