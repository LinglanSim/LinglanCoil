using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.Basic;

namespace Test
{
    [TestClass]
    public class TestforRef
    {
        [TestMethod]
        public void TestMethod1(double[,] SourceTableData)
        {
            //var r = Main.main_evaporator();
            //var r1 = Main.main_condenser();
            RefStateInput refInput=new RefStateInput();
            CapiliaryInput cap_inlet = new CapiliaryInput();
            CapiliaryInput cap_outlet = new CapiliaryInput();
            refInput.FluidName = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", refInput.FluidName);
            refInput.Massflowrate = 0.005;
            refInput.tc = 45;
            refInput.tri = 75;
            AirStateInput airInput=new AirStateInput();
            airInput.RHi = 0.5;
            airInput.tai = 35;
            airInput.Volumetricflowrate = 1000;
            GeometryInput geoInput=new GeometryInput();
            geoInput.Pt = 21;
            geoInput.Pr = 22;
            geoInput.Di = 6.9;
            geoInput.Do = 7.4;
            geoInput.L = 600;
            geoInput.FPI = 1.3;
            geoInput.Fthickness = 0.095;
            geoInput.Nrow = 2;
            geoInput.Ntube = 14;
            geoInput.CirNum = 3;
            // cap input
            cap_inlet.d_cap = new double[6];// { 0, 0 };//0.006
            cap_inlet.lenth_cap = new double[6];// { 0, 0 };//0.5
            cap_outlet.d_cap = new double[6];// { 0, 0 };//0.006
            cap_outlet.lenth_cap = new double[6];// { 0, 0 };//0.5

            string flowtype = "逆流";
            string fin_type="平片";
            string tube_type="光管";
            string hex_type="冷凝器";
            var r1 = Main.main_condenser_Slab(refInput,airInput,geoInput,flowtype,fin_type,tube_type,hex_type,cap_inlet, cap_outlet, SourceTableData);
            //var r = Main.MinNout();
            //var r = Main.NinMout();
        }
    }
}

