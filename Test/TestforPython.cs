using System;
using System.IO;
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
            AbstractState coolprop = AbstractState.factory("HEOS", refInput.FluidName);
            //*************if input SC or Sh, Massflowrate is the initial input***************
            refInput.Massflowrate = 0.02161311486;//0.02 //kg/s
            refInput.zh = 1.6;
            refInput.zdp = 4;
            airInput.za = 1;
            airInput.zdpa = 1.0;

            //for condenser
            refInput.tc = 45;
            refInput.tri = 80;
            //refInput.tc = 40;
            //refInput.tri = 70;

            //for evaporator
            refInput.te = 6.92842345;//12
            //refInput.P_exv = CoolProp.PropsSI("P", "T", refInput.tc + 273.15, "Q", 0, refInput.FluidName) / 1000;
            coolprop.update(input_pairs.QT_INPUTS, 0, refInput.tc + 273.15);
            refInput.P_exv = coolprop.p() / 1000;
            refInput.T_exv = refInput.tc - 8;
            refInput.H_exv = 272;

            //air input
            airInput.Volumetricflowrate = 0.23; //m3/s
            airInput.tai = 27;//24.85
            airInput.RHi = 0.47;

            //airInput.ha = 80;

            //geo input
            //mm
            geoInput.Pt = 21;
            geoInput.Pr = 13.37;
            geoInput.Di = 6.89;
            geoInput.Do = 7.35;
            geoInput.L = 653;
            geoInput.FPI = 21;
            geoInput.Fthickness = 0.095;
            geoInput.Nrow =2;
            geoInput.Ntube = 15;
            geoInput.CirNum = 2;

            //初始化湿空气数组
            double[,] HumidSourceData= Model.HumidAirSourceData.InitializeSourceTableData();
            DateTime Time1 = DateTime.Now;
            //var rr = Main.main_condenser_py(refInput, airInput, geoInput, capInput, coolprop, Model.HumidAirSourceData.SourceTableData);
            //var r = Main.main_evaporator_py(refInput, airInput, geoInput, capInput, coolprop, HumidAirSourceData.SourceTableData);
            var rr = Main.main_condenser_py(refInput, airInput, geoInput, HumidSourceData);
            var r = Main.main_evaporator_py(refInput, airInput, geoInput, HumidSourceData);

            //for (int i=0;i<5;i++)
            //r = Main.main_evaporator_py(refInput, airInput, geoInput, capInput);

            //DateTime Time2 = DateTime.Now;
            //double time01 = (Time2 - Time1).TotalSeconds;

            //using (StreamWriter wr = File.AppendText(@"D:\QQQ.txt"))
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        refInput.H_exv = 270 + i;
            //        var r = Main.main_evaporator_py(refInput, airInput, geoInput, HumidSourceData);
            //        wr.WriteLine("{0}", r.Q);
            //    }
            //}

            //using (StreamWriter wr = File.AppendText(@"D:\QQQ.txt"))
            //{
            //    for (int i = 0; i < 15; i++)
            //    {
            //        for (int j = 0; j < 2;j++ )
            //        {
            //            wr.WriteLine("{0}", r.Q_detail[i,j]);
            //        }
            //    }
            //}

            //double Tsc_set = 5;
            //double Tsh_set = 5;
            //var rr = Main.main_condenser_inputSC_py(Tsc_set, refInput, airInput, geoInput, capInput, coolprop);
            //var rrr = Main.main_evaporator_inputSH_py(Tsh_set, refInput, airInput, geoInput, capInput, coolprop);
            DateTime Time2 = DateTime.Now;
            double time01 = (Time2 - Time1).TotalSeconds;

        }
    }
}
