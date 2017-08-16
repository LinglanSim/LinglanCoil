using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace Model.Basic
{
    public class Brine
    {
        #region BrineProp.dll import

        [DllImport("BrineProp.dll",
        EntryPoint = "BrineSpecificVolumeDll",
        CallingConvention = CallingConvention.StdCall)]
        private extern static double BrineSpecificVolumeDll(string name, ref double conc, ref double temp, ref double press, int ln);//m3/kg

        [DllImport("BrineProp.dll",
        EntryPoint = "BrineSpecificHeatDll",
        CallingConvention = CallingConvention.StdCall)]
        private extern static double BrineSpecificHeatDll(string name, ref double conc, ref double press, ref double temp, int ln);

        [DllImport("BrineProp.dll",
            EntryPoint = "BrineViscosityDll",
            CallingConvention = CallingConvention.StdCall)]
        private extern static double BrineViscosityDll(string name, ref double conc, ref double press, ref double temp, int ln);

        [DllImport("BrineProp.dll",
            EntryPoint = "BrineConductivityDll",
            CallingConvention = CallingConvention.StdCall)]
        private extern static double BrineConductivityDll(string name, ref double conc, ref double press, ref double temp, int ln);

        #endregion

        private static ConcurrentDictionary<string, double> _specificVolumeCache = new ConcurrentDictionary<string, double>();
        public static double SpecificVolume(Fluid fluid, double temperature, double pressure)
        {

            string name = fluid.Name;
            double c = fluid.Concentration;
            double t = temperature;
            double p = pressure;

            string key = string.Format("{0}{1}{2}{3}", name, c.ToString(), t.ToString(), p.ToString());
            if (_specificVolumeCache.ContainsKey(key))
            {
                return _specificVolumeCache[key];
            }
            else
            {
                double result = BrineSpecificVolumeDll(name, ref c, ref t, ref p, name.Length);
                _specificVolumeCache.AddOrUpdate(key, result, (k, v) => result);
                return result;
            }
        }

        private static ConcurrentDictionary<string, double> _specificHeatCache = new ConcurrentDictionary<string, double>();
        public static double SpecificHeat(Fluid fluid, double temperature, double pressure)
        {
            string name = fluid.Name;
            double c = fluid.Concentration;
            double t = temperature;
            double p = pressure;

            string key = string.Format("{0}{1}{2}{3}", name, c.ToString(), t.ToString(), p.ToString());
            if (_specificHeatCache.ContainsKey(key))
            {
                return _specificHeatCache[key];
            }
            else
            {
                double result = BrineSpecificHeatDll(name, ref c, ref p, ref t, name.Length);
                _specificHeatCache.AddOrUpdate(key, result, (k, v) => result);
                return result;
            }
        }

        private static ConcurrentDictionary<string, double> _viscosityCache = new ConcurrentDictionary<string, double>();
        public static double Viscosity(Fluid fluid, double temperature, double pressure)
        {
            string name = fluid.Name;
            double c = fluid.Concentration;
            double t = temperature;
            double p = pressure;

            string key = string.Format("{0}{1}{2}{3}", name, c.ToString(), t.ToString(), p.ToString());
            if (_viscosityCache.ContainsKey(key))
            {
                return _viscosityCache[key];
            }
            else
            {
                double result = BrineViscosityDll(name, ref c, ref p, ref t, name.Length);
                _viscosityCache.AddOrUpdate(key, result, (k, v) => result);
                return result;
            }
        }

        private static ConcurrentDictionary<string, double> _conductivityCache = new ConcurrentDictionary<string, double>();
        public static double Conductivity(Fluid fluid, double temperature, double pressure)
        {
            string name = fluid.Name;
            double c = fluid.Concentration;
            double t = temperature;
            double p = pressure;

            string key = string.Format("{0}{1}{2}{3}", name, c.ToString(), t.ToString(), p.ToString());
            if (_conductivityCache.ContainsKey(key))
            {
                return _conductivityCache[key];
            }
            else
            {
                double result = BrineConductivityDll(name, ref c, ref p, ref t, name.Length);
                _conductivityCache.AddOrUpdate(key, result, (k, v) => result);
                return result;
            }
        }

        /// <summary>
        /// rhhg
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns>J/kg</returns>
        public static double SteamLatentHeat(double temperature)
        {
            double t = temperature;
            double hfg = -0.0035 * Math.Pow(t, 2) - 2.0806 * t + 2498.8; // kJ/kg
            hfg = hfg * 1e3d;
            return hfg;
        }
    }
}
