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
            //var r = Main.MinNout();
            //var r = Main.NinMout();
            double mu_r;
            double Tdp;
            DateTime Time1 = DateTime.Now;
            for(int i=0;i<10000;i++)
            {
                mu_r = CoolProp.HAPropsSI("W", "T", 293.15, "P", 101325,"R",0.5);
            }          
            DateTime Time2 = DateTime.Now;
            for(int i=0;i<10000;i++)
            {
                Tdp = CoolProp.HAPropsSI("D", "T", 20 + 273.15, "P", 101325, "R", 0.5) ;
            }
            DateTime Time3=DateTime.Now;
            double T1=(Time2-Time1).TotalSeconds;
            double T2=(Time3-Time2).TotalSeconds;

        }
    }
}

