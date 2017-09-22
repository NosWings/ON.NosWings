using System;
using OpenNos.Core;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper : Singleton<MateHelper>
    {
        #region Instantiation

        #endregion

        #region Members

        public MateHelper()
        {
            LoadXpData();
            LoadPrimaryMpData();
            LoadSecondaryMpData();
            LoadHpData();
        }

        #endregion

        #region Properties

        public double[] HpData { get; private set; }

        // Race == 0
        public double[] PrimaryMpData { get; private set; }

        // Race == 2
        public double[] SecondaryMpData { get; private set; }

        public double[] XpData { get; private set; }

        #endregion

        #region Methods

        private void LoadPrimaryMpData()
        {
            PrimaryMpData = new double[256];
            PrimaryMpData[0] = 10;
            PrimaryMpData[1] = 10;
            PrimaryMpData[2] = 15;

            int basup = 5;
            byte count = 0;
            bool isStable = true;
            bool isDouble = false;

            for (int i = 3; i < PrimaryMpData.Length; i++)
            {
                if (i % 10 == 1)
                {
                    PrimaryMpData[i] += PrimaryMpData[i - 1] + basup * 2;
                    continue;
                }
                if (!isStable)
                {
                    basup++;
                    count++;

                    if (count == 2)
                    {
                        if (isDouble)
                        { isDouble = false; }
                        else
                        { isStable = true; isDouble = true; count = 0; }
                    }

                    if (count == 4)
                    { isStable = true; count = 0; }
                }
                else
                {
                    count++;
                    if (count == 2)
                    { isStable = false; count = 0; }
                }
                PrimaryMpData[i] = PrimaryMpData[i - (i % 10 == 2 ? 2 : 1)] + basup;
            }
        }

        private void LoadSecondaryMpData()
        {
            SecondaryMpData = new double[256];
            SecondaryMpData[0] = 60;
            SecondaryMpData[1] = 60;
            SecondaryMpData[2] = 78;

            int basup = 18;
            bool boostup = false;

            for (int i = 3; i < SecondaryMpData.Length; i++)
            {
                if (i % 10 == 1)
                {
                    SecondaryMpData[i] += SecondaryMpData[i - 1] + i + 10;
                    continue;
                }

                if(boostup)
                { basup += 3; boostup = false; }
                else
                { basup++; boostup = true; }

                SecondaryMpData[i] = SecondaryMpData[i - (i % 10 == 2 ? 2 : 1)] + basup;
            }
        }

        private void LoadHpData()
        {
            HpData = new double[256];
            int baseHp = 138;
            int HPbasup = 18;
            for (int i = 0; i < HpData.Length; i++)
            {
                HpData[i] = baseHp;
                HPbasup++;
                baseHp += HPbasup;

                if (i == 37)
                {
                    baseHp = 1765;
                    HPbasup = 65;
                }
                if (i < 41)
                {
                    continue;
                }
                if (((99 - i) % 8) == 0)
                {
                    HPbasup++;
                }
            }
        }

        private void LoadXpData()
        {
            // Load XpData
            XpData = new double[256];
            double[] v = new double[256];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            XpData[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }
            for (int i = 1; i < XpData.Length; i++)
            {
                if (i < 79)
                {
                    switch (i)
                    {
                        case 14:
                            var = 6 / 3d;
                            break;
                        case 39:
                            var = 19 / 3d;
                            break;
                        case 59:
                            var = 70 / 3d;
                            break;
                    }
                    XpData[i] = Convert.ToInt64(XpData[i - 1] + var * v[i - 1]);
                }
                if (i < 79)
                {
                    continue;
                }
                switch (i)
                {
                    case 79:
                        var = 5000;
                        break;
                    case 82:
                        var = 9000;
                        break;
                    case 84:
                        var = 13000;
                        break;
                }
                XpData[i] = Convert.ToInt64(XpData[i - 1] + var * (i + 2) * (i + 2));
            }
        }

        #endregion

        #region Singleton

        private static MateHelper _instance;

        public static MateHelper Instance
        {
            get { return _instance ?? (_instance = new MateHelper()); }
        }

        #endregion
    }
}