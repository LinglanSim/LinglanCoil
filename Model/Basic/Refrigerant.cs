using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Model.Basic
{
    public class Refrigerant
    {
        #region refprop.dll import
        [DllImport("refprop.dll", EntryPoint = "SETMIXdll", SetLastError = true)]
        public static extern void SETMIXdll(
            char[] hfd, char[] hfm, char[] hrf,
            ref int nc, char[] hfile, double[] x,
            ref int ierr, char[] herr,
            int l1, int l2, int l3, int l4, int l5);

        [DllImport("refprop.dll", EntryPoint = "SETUPdll", SetLastError = true)]
        public static extern void SETUPdll(
            ref int nc, char[] hfld, char[] hfm, char[] hrf,
            ref int ierr, char[] herr,
            int l1, int l2, int l3, int l4);

        [DllImport("refprop.dll", EntryPoint = "SETREFdll", SetLastError = true)]
        public static extern void SETREFdll(
            char[] hrf, ref int ixflag, double[] x,
            ref double h0, ref double s0, ref double t0, ref double p0,
            ref int ierr, char[] herr,
            int l1, int l2);

        [DllImport("refprop.dll", EntryPoint = "ENTHALdll", SetLastError = true)]
        public static extern void ENTHALdll(
            ref double temperature, ref double density, double[] composition,
            ref double enthal);

        [DllImport("refprop.dll", EntryPoint = "CVCPdll", SetLastError = true)]
        public static extern void CVCPdll(
            ref double temperature, ref double density, double[] composition,
            ref double cv, ref double cp);

        [DllImport("refprop.dll", EntryPoint = "SATPdll", SetLastError = true)]
        public static extern void SATPdll(
            ref double pressure, double[] composition, ref int phase,
            ref double temperature, ref double densityL, ref double densityV,
            double[] compositionL, double[] compositionV,
            ref int ierr, char[] herr,
            int l1);
        [DllImport("refprop.dll", EntryPoint = "CRITPdll", SetLastError = true)]
        public static extern void CRITPdll(
            double[] composition, ref double temperature,
            ref double pressure, ref double density,
            ref int ierr, char[] herr,
            int l1);      
        [DllImport("refprop.dll", EntryPoint = "SATTdll", SetLastError = true)]
        public static extern void SATTdll(
            ref double temperature, double[] composition, ref int phase,
            ref double pressure, ref double densityL, ref double densityV,
            double[] compositionL, double[] compositionV,
            ref int ierr, char[] herr,
            int l1);

        [DllImport("refprop.dll", EntryPoint = "TPFLSHdll", SetLastError = true)]
        public static extern void TPFLSHdll(
            ref double t, ref double p, double[] z,
            ref double D, ref double Dl, ref double Dv,
            ref double x, ref double y,
            ref double q, ref double e,
            ref double h, ref double s,
            ref double cv, ref double cp, ref double w,
            ref int ierr, char[] herr,
            int l1);

        [DllImport("refprop.dll", EntryPoint = "PHFLSHdll", SetLastError = true)]
        public static extern void PHFLSHdll(
            ref double p, ref double h, double[] z,
            ref double t,
            ref double D, ref double Dl, ref double Dv,
            ref double x, ref double y,
            ref double q, ref double e,
            ref double s,
            ref double cv, ref double cp, ref double w,
            ref int ierr, char[] herr,
            int l1);

        [DllImport("refprop.dll", EntryPoint = "PQFLSHdll", SetLastError = true)]
        public static extern void PQFLSHdll(
            ref double p, ref double q, double[] z, ref int kq,
            ref double t,
            ref double D, ref double Dl, ref double Dv,
            ref double x, ref double y,
            ref double e,
            ref double h, ref double s,
            ref double cv, ref double cp, ref double w,
            ref int ierr, char[] herr,
            int l1);

        [DllImport("refprop.dll", EntryPoint = "TQFLSHdll", SetLastError = true)]
        public static extern void TQFLSHdll(
            ref double t, ref double q, double[] z, ref int kq, 
            ref double p,
            ref double D, ref double Dl, ref double Dv,
            ref double x, ref double y,
            ref double e,
            ref double h, ref double s,
            ref double cv, ref double cp, ref double w,
            ref int ierr, char[] herr,
            int l1);

        [DllImport("refprop.dll", EntryPoint = "TRNPRPdll", SetLastError = true)]
        public static extern void TRNPRPdll(
            ref double temperature, ref double density, double[] composition,
            ref double viscosity, ref double thermalConductivity,
            ref int ierr, char[] herr,
            int l1);

        [DllImport("refprop.dll", EntryPoint = "WMOLdll", SetLastError = true)]
        public static extern void WMOLdll(double[] x, ref double wm);

        [DllImport("refprop.dll", EntryPoint = "DHD1dll", SetLastError = true)]
        public static extern void DHD1dll(
            ref double temperature, ref double density, double[] composition,
            ref double dhdt_d, ref double dhdt_p,
            ref double dhdd_t, ref double dhdd_p,
            ref double dhdp_t, ref double dhdp_d,
            ref int ierr, char[] herr,
            int l1);
        #endregion

        #region Shared variables

        const int MAXNC = 20;
        static string _fluidsPath;
        static string _mixturesPath;
        static char[] _hfm;
        static char[] _hfld;
        static char[] _hrf;
        static char[] _hfile;
        static int _ierr;
        static char[] _herr;
        static char[] _hname;
        static int _nc;
        //static double _wm;
        static double[] _x;
        static double[] _xl;
        static double[] _xv;
        static double[] _xlkg;
        static double[] _xvkg;

        static string[] _components;

        #endregion

        #region Private methods

        static Refrigerant()
        {
            InitializeVariables();
        }

        private static void InitializeVariables()
        {
            _fluidsPath = "fluids/";
            _mixturesPath = "mixtures/";
            _hfm = (_fluidsPath + "hmx.bnc").ToArray();
            _hfld = new char[10000];
            _hrf = "DEF".ToCharArray();
            //_hrf = "OT0".ToCharArray();
            _hfile = new char[10000];
            _ierr = 0; // > 0 error; < 0 warning
            _herr = new char[255];
            _hname = new char[12];
            _nc = 0;
            //_wm = 0;
            _x = new double[MAXNC];
            _xl = new double[MAXNC];
            _xv = new double[MAXNC];
            _xlkg = new double[MAXNC];
            _xvkg = new double[MAXNC];

            //_components = new string[MAXNC];
        }

        private static void Initialize(string[] components, double[] composition)
        {
            if (components.Length > MAXNC) throw new Exception("Too many components.");

            // return if still the same fluid
            if (_components != null && _x != null
                && string.Join("", _components) == string.Join("", components)
                && string.Join("", _x) == string.Join("", composition))
            {
                return;
            }

            InitializeVariables();
            //components.CopyTo(_components, 0);
            _components = components;
            composition.CopyTo(_x, 0);

            if (_components.Length == 1 && _components[0].ToUpper().Contains(".MIX"))
            { // pre-defined mixed (.mix)
                string s = _mixturesPath + _components[0];
                s.CopyTo(0, _hfld, 0, s.Length);

                SETMIXdll(_hfld, _hfm, _hrf,
                    ref _nc, _hfile, _x, ref _ierr, _herr,
                    _hfld.Length, _hfm.Length, _hrf.Length, _hfile.Length, _herr.Length);
                if (_ierr > 0)
                {
                    throw new Exception(new string(_herr));
                }
            }
            else
            { // pure (.fld); pseudo-pure fluid (.ppf); user-defined mixed (a.fld|b.fld|c.fld|)
                _nc = _components.Length;
                if (_nc == 1)
                { // pure (.fld); pseudo-pure fluid (.ppf)
                    string s = _fluidsPath + _components[0] + "|"; // postfix "|" is needed
                    s.CopyTo(0, _hfld, 0, s.Length);
                }
                else
                { // user-defined mixed (a.fld|b.fld|c.fld|)
                    string s = _fluidsPath + string.Join("|" + _fluidsPath, _components) + "|";
                    s.CopyTo(0, _hfld, 0, s.Length);
                }

                SETUPdll(ref _nc, _hfld, _hfm, _hrf,
                    ref _ierr, _herr,
                    _hfld.Length, _hfm.Length, _hrf.Length, _herr.Length);
                if (_ierr < 0)
                {
                    int ixflag = 1;
                    double h0 = 0, s0 = 0, t0 = 0, p0 = 0;
                    SETREFdll(_hrf, ref ixflag, _x, ref h0, ref s0, ref t0, ref p0,
                        ref _ierr, _herr,
                        _hrf.Length, _herr.Length);
                    //if (_ierr == 0)
                    //{
                    //    WMOLdll(_x, ref _wm);
                    //}
                }
                else if (_ierr != 0)
                {
                    throw new Exception(new string(_herr));
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// compute the transport properties of thermal conductivity and
        /// viscosity as functions of temperature and density
        /// In: [], [], [K], [mol/L]
        /// Out: [uPa.s], [W/m.K]
        /// </summary>
        public static TRNPRPResult TRNPRP(string[] components, double[] composition, double temperature, double density)
        {
            //call TRNPRP (RefName,t,rho,eta,tcx,ierr,herr)
            Initialize(components, composition);
            double viscosity = 0;
            double thermalConductivity = 0;
            TRNPRPdll(ref temperature, ref density, _x, ref viscosity, ref thermalConductivity, ref _ierr, _herr, _herr.Length);
            return new TRNPRPResult { Viscosity = viscosity, ThermalConductivity = thermalConductivity };
        }
        public struct TRNPRPResult
        {
            public double Viscosity;
            public double ThermalConductivity;
        }


        /// <summary>
        /// In: [], [], [kPa], []
        /// Out: [K], [mol/L], [mol/L]
        /// </summary>
        public static SATPResult SATP(string[] components, double[] composition, double pressure, int phase)
        {
            Initialize(components, composition);
            double temperature = 0.0;
            double densityL = 0.0;
            double densityV = 0.0;
            int ierr = 0;
            char[] herr = new char[256];
            SATPdll(ref pressure, _x, ref phase, ref temperature, ref densityL, ref densityV, _xl, _xv, ref ierr, herr, herr.Length);
            switch (phase)
            {
                // phase: 1 liq, 2 vap, 3 liq (freezing point), 4 vap (sublimation point)
                case 1: return new SATPResult { Temperature = temperature, DensityL = densityL, DensityV = 0 };
                case 2: return new SATPResult { Temperature = temperature, DensityL = 0, DensityV = densityV };
                default: return new SATPResult();
            }
        }
        public struct SATPResult
        {
            public double Temperature;
            public double DensityL;
            public double DensityV;
        }

        public static SATT_TotalResult SATTTotal(string[] components, double[] composition, double temperature)
        {
            Initialize(components, composition);
            double pressure = 0.0;
            double densityL = 0.0;
            double densityV = 0.0;
            double enthalpyL = 0.0;
            double enthalpyV = 0.0;
            double viscosityL = 0.0;
            double viscosityV = 0.0;
            double cpL = 0.0;
            double cpV = 0.0;
            double kL = 0.0;
            double kV = 0.0;
            double wm = 0.0;
            int phase1 = 1;
            int phase2 = 2;
            //pressure is the liquid satration pressure
            pressure = SATT(components, composition, temperature, phase1).Pressure;  //kpa 
            densityL = SATT(components, composition, temperature, phase1).DensityL;  //mol/L
            densityV = SATT(components, composition, temperature, phase2).DensityV;
            //enthalpyL = TQFLSH(components, composition, temperature, 0.0).h; //J/mol
            //enthalpyV = TQFLSH(components, composition, temperature, 1.0).h;
            enthalpyL = ENTHAL(components, composition, temperature, densityL).Enthalpy;
            enthalpyV = ENTHAL(components, composition, temperature, densityV).Enthalpy;
            viscosityL = TRNPRP(components, composition, temperature, densityL).Viscosity; //uPa.s
            viscosityV = TRNPRP(components, composition, temperature, densityV).Viscosity;
            cpL = CVCP(components, composition, temperature, densityL).Cp;  //J/mol-K
            cpV = CVCP(components, composition, temperature, densityV).Cp;
            kL = TRNPRP(components, composition, temperature, densityL).ThermalConductivity; //W/m.K
            kV = TRNPRP(components, composition, temperature, densityV).ThermalConductivity;
            wm = WM(components, composition).Wm; //g/mol
            var r = new SATTTotalResult();
            r.pressure = pressure;   //kpa
            r.DensityL = densityL * wm; //kg/m3
            r.DensityV = densityV * wm;
            r.EnthalpyL = enthalpyL / wm - (components[0] == "Water" ? 0 : 140);//kJ/kg
            r.EnthalpyV = enthalpyV / wm - (components[0] == "Water" ? 0 : 140);
            r.ViscosityL = viscosityL / Math.Pow(10, 6);//kg/m-s
            r.ViscosityV = viscosityV / Math.Pow(10, 6);
            r.CpL = cpL / wm ;//kJ/kg-K
            r.CpV = cpV / wm ;
            r.KL = kL; //W/m.K
            r.KV = kV;
            r.Wm = wm;//g/mol
            return new SATT_TotalResult { SATTTotalResult = r };

        }
        public struct SATTTotalResult
        {
            public double pressure;
            public double DensityL;
            public double DensityV;
            public double EnthalpyL;
            public double EnthalpyV;
            public double ViscosityL;
            public double ViscosityV;
            public double CpL;
            public double CpV;
            public double KL;
            public double KV;
            public double Wm;

        }

        public class SATT_TotalResult
        {
            public SATTTotalResult SATTTotalResult;

        }


        /// <summary>
        /// In: [], [], [K], []
        /// Out: [kPa], [mol/L], [mol/L]
        /// </summary>
        public static SATTResult SATT(string[] components, double[] composition, double temperature, int phase)
        {
            Initialize(components, composition);
            double pressure = 0.0;
            double densityL = 0.0;
            double densityV = 0.0;
            SATTdll(ref temperature, _x, ref phase, ref pressure, ref densityL, ref densityV, _xl, _xv, ref _ierr, _herr, _herr.Length);
            switch (phase)
            {
                // phase: 1 liq, 2 vap, 3 liq (freezing point), 4 vap (sublimation point)
                //        for mixtures, 1 & 2 mean buble and dew points at same composition
                case 1: return new SATTResult { Pressure = pressure, DensityL = densityL, DensityV = 0 };
                case 2: return new SATTResult { Pressure = pressure, DensityL = 0, DensityV = densityV };
                default: return new SATTResult();
            }
        }
        public struct SATTResult
        {
            public double Pressure;
            public double DensityL;
            public double DensityV;
        }

        public static ENTHALResult ENTHAL(string[] components, double[] composition, double temperature, double density)
        {
            Initialize(components, composition);
            double enthal = 0.0;
            ENTHALdll(ref temperature, ref density, _x, ref enthal);
            return new ENTHALResult { Enthalpy = enthal };
        }
        public struct ENTHALResult
        {
            public double Enthalpy;
        }
        
        public static CVCPResult CVCP(string[] components, double[] composition, double temperature, double density)
        {
            Initialize(components, composition);
            double cv = 0.0;
            double cp = 0.0;
            CVCPdll(ref temperature, ref density, _x, ref cv, ref cp); //, ref _ierr, _herr, _herr.Length);
            return new CVCPResult { Cv = cv, Cp = cp };
        }
        public struct CVCPResult
        {
            public double Cv;
            public double Cp;
        }

        public static DHD1Result DHD1(string[] components, double[] composition, double temperature, double density)
        {
            Initialize(components, composition);
            DHD1Result r = new DHD1Result();
            DHD1dll(ref temperature, ref density, _x,
                ref r.dhdt_d, ref r.dhdt_p, ref r.dhdd_t, ref r.dhdd_p, ref r.dhdp_t, ref r.dhdp_d,
                ref _ierr, _herr, _herr.Length);
            return r;
        }
        public struct DHD1Result
        {
            public double dhdt_d;
            public double dhdt_p;
            public double dhdd_t;
            public double dhdd_p;
            public double dhdp_t;
            public double dhdp_d;
        }

        public static TPFLSHResult TPFLSH(string[] components, double[] composition, double temperature, double pressure)
        {
            Initialize(components, composition);
            TPFLSHResult r = new TPFLSHResult();
            TPFLSHdll(ref temperature, ref pressure, _x,
                ref r.D, ref r.Dl, ref r.Dv,
                ref r.x, ref r.y, ref r.q, ref r.e, ref r.h, ref r.s,
                ref r.cv, ref r.cp, ref r.w,
                ref _ierr, _herr, _herr.Length);
            return r;
        }
        public struct TPFLSHResult
        {
            public double D;
            public double Dl;
            public double Dv;
            public double x;
            public double y;
            public double q;
            public double e;
            public double h;
            public double s;
            public double cv;
            public double cp;
            public double w;
        }

        public static WMResult WM(string[] components, double[] composition)
        {
            Initialize(components, composition);
            double wm = 0;
            WMOLdll(_x, ref wm);
            return new WMResult { Wm=wm };
        }
        public struct WMResult
        {
            public double Wm;
        }       

        public struct TQFLSHResults
        {
            public double p;
            public double D;
            public double Dl;
            public double Dv;
            public double x;
            public double y;
            public double e;
            public double h;
            public double s;
            public double cv;
            public double cp;
            public double w;
        }

        public static TQFLSHResults TQFLSH(string[] components, double[] composition, double temperature, double quality)
        {
            Initialize(components,composition);
            TQFLSHResults r=new TQFLSHResults();
            int kq = 2;
            //kq_					flag specifying units for input quality
			//				kq = 1 quality on MOLAR basis [moles vapor/total moles]
			//				kq = 2 quality on MASS basis [mass vapor/total mass]
            TQFLSHdll(ref temperature, ref quality, _x,
                ref kq, ref r.p, ref r.D, ref r.Dl, ref r.Dv, 
                ref r.x, ref r.y, ref r.e, ref r.h, ref r.s, 
                ref r.cv, ref r.cp, ref r.w, ref _ierr, _herr, _herr.Length);
            return r;
        }

        public struct PQFLSHResults
        {
            public double t;
            public double D;
            public double Dl;
            public double Dv;
            public double x;
            public double y;
            public double e;
            public double h;
            public double s;
            public double cv;
            public double cp;
            public double w;
        }

        public static PQFLSHResults PQFLSH(string[] components, double[] composition, double pressure, double quality)
        {
            Initialize(components, composition);
            PQFLSHResults r = new PQFLSHResults();
            int kq = 2;
            //kq_					flag specifying units for input quality
            //				kq = 1 quality on MOLAR basis [moles vapor/total moles]
            //				kq = 2 quality on MASS basis [mass vapor/total mass]
            PQFLSHdll(ref pressure, ref quality, _x,
                ref kq, ref r.t, ref r.D, ref r.Dl, ref r.Dv,
                ref r.x, ref r.y, ref r.e, ref r.h, ref r.s,
                ref r.cv, ref r.cp, ref r.w, ref _ierr, _herr, _herr.Length);
            return r;
        }

        public struct PHFLSHResults
        {
            public double t;
            public double D;
            public double Dl;
            public double Dv;
            public double x;
            public double y;
            public double q;
            public double e;
            public double s;
            public double cv;
            public double cp;
            public double w;
        }

        public static PHFLSHResults PHFLSH(string[] components, double[] composition, double pressure, double enthalpy)
        {
            Initialize(components, composition);
            PHFLSHResults r = new PHFLSHResults();
            int kq = 2;
            //kq_					flag specifying units for input quality
            //				kq = 1 quality on MOLAR basis [moles vapor/total moles]
            //				kq = 2 quality on MASS basis [mass vapor/total mass]
            PHFLSHdll(ref pressure, ref enthalpy, _x,
                ref r.t, ref r.D, ref r.Dl, ref r.Dv,
                ref r.x, ref r.y, ref r.q, ref r.e, ref r.s,
                ref r.cv, ref r.cp, ref r.w, ref _ierr, _herr, _herr.Length);
            return r;
        }
        public struct CRITResults
        {
            public double t;
            public double p;
            public double rho;
        }

        public static CRITResults CRIT(string[] components, double[] composition)
        {
            Initialize(components, composition);
            double temperature = 0.0;
            double pressure = 0.0;
            double density = 0.0;
            CRITPdll(_x, ref temperature, ref pressure, ref density, ref _ierr, _herr, _herr.Length);
            return new CRITResults { t = temperature, p = pressure, rho = density };

        }
        #endregion Public methods
    }
}
