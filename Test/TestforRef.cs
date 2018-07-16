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
        public void TestMethod1()
        {
            //var r = Main.main_evaporator();
            //var r1 = Main.main_condenser();
            RefStateInput refInput=new RefStateInput();
            refInput.FluidName = "R32";
            refInput.Massflowrate = 0.02;
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
            geoInput.Ntube = 12;
            geoInput.CirNum = 3;
            string flowtype="Counter";
            string fin_type="Plain";
            string tube_type="smooth";
            string hex_type="Condenser";
            var r1 = Main.main_condenser_Slab(refInput,airInput,geoInput,flowtype,fin_type,tube_type,hex_type);
            //var r = Main.MinNout();
            //var r = Main.NinMout();
        }
    }
}

