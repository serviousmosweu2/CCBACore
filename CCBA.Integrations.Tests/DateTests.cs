using CCBA.Integrations.Base.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CCBA.Integrations.Tests
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public async Task FiscalTest()
        {
            var Feb20 = FinancialExtensions.GetFiscalYearDates(2020, EMonth.Feb);

            //445 Leap
            var t0 = FinancialExtensions.GetFiscalYearDates(2020);
            Assert.AreEqual(new DateTime(2020, 1, 1), t0[0]);
            Assert.AreEqual(new DateTime(2020, 1, 25), t0[1]);
            Assert.AreEqual(new DateTime(2020, 2, 22), t0[2]);
            Assert.AreEqual(new DateTime(2020, 3, 28), t0[3]);
            Assert.AreEqual(new DateTime(2020, 4, 25), t0[4]);
            Assert.AreEqual(new DateTime(2020, 5, 23), t0[5]);
            Assert.AreEqual(new DateTime(2020, 6, 27), t0[6]);
            Assert.AreEqual(new DateTime(2020, 7, 25), t0[7]);
            Assert.AreEqual(new DateTime(2020, 8, 22), t0[8]);
            Assert.AreEqual(new DateTime(2020, 9, 26), t0[9]);
            Assert.AreEqual(new DateTime(2020, 10, 24), t0[10]);
            Assert.AreEqual(new DateTime(2020, 11, 21), t0[11]);

            //545
            var t1 = FinancialExtensions.GetFiscalYearDates(2021);
            Assert.AreEqual(new DateTime(2021, 1, 1), t1[0]);
            Assert.AreEqual(new DateTime(2021, 1, 30), t1[1]);
            Assert.AreEqual(new DateTime(2021, 2, 27), t1[2]);
            Assert.AreEqual(new DateTime(2021, 4, 3), t1[3]);
            Assert.AreEqual(new DateTime(2021, 5, 1), t1[4]);
            Assert.AreEqual(new DateTime(2021, 5, 29), t1[5]);
            Assert.AreEqual(new DateTime(2021, 7, 3), t1[6]);
            Assert.AreEqual(new DateTime(2021, 7, 31), t1[7]);
            Assert.AreEqual(new DateTime(2021, 8, 28), t1[8]);
            Assert.AreEqual(new DateTime(2021, 10, 2), t1[9]);
            Assert.AreEqual(new DateTime(2021, 10, 30), t1[10]);
            Assert.AreEqual(new DateTime(2021, 11, 27), t1[11]);

            //445
            var t2 = FinancialExtensions.GetFiscalYearDates(2022);
            Assert.AreEqual(new DateTime(2022, 1, 1), t2[0]);
            Assert.AreEqual(new DateTime(2022, 1, 29), t2[1]);
            Assert.AreEqual(new DateTime(2022, 2, 26), t2[2]);
            Assert.AreEqual(new DateTime(2022, 4, 2), t2[3]);
            Assert.AreEqual(new DateTime(2022, 4, 30), t2[4]);
            Assert.AreEqual(new DateTime(2022, 5, 28), t2[5]);
            Assert.AreEqual(new DateTime(2022, 7, 2), t2[6]);
            Assert.AreEqual(new DateTime(2022, 7, 30), t2[7]);
            Assert.AreEqual(new DateTime(2022, 8, 27), t2[8]);
            Assert.AreEqual(new DateTime(2022, 10, 1), t2[9]);
            Assert.AreEqual(new DateTime(2022, 10, 29), t2[10]);
            Assert.AreEqual(new DateTime(2022, 11, 26), t2[11]);

            //445
            var t3 = FinancialExtensions.GetFiscalYearDates(2023);
            Assert.AreEqual(new DateTime(2023, 1, 1), t3[0]);
            Assert.AreEqual(new DateTime(2023, 1, 28), t3[1]);
            Assert.AreEqual(new DateTime(2023, 2, 25), t3[2]);
            Assert.AreEqual(new DateTime(2023, 4, 1), t3[3]);
            Assert.AreEqual(new DateTime(2023, 4, 29), t3[4]);
            Assert.AreEqual(new DateTime(2023, 5, 27), t3[5]);
            Assert.AreEqual(new DateTime(2023, 7, 1), t3[6]);
            Assert.AreEqual(new DateTime(2023, 7, 29), t3[7]);
            Assert.AreEqual(new DateTime(2023, 8, 26), t3[8]);
            Assert.AreEqual(new DateTime(2023, 9, 30), t3[9]);
            Assert.AreEqual(new DateTime(2023, 10, 28), t3[10]);
            Assert.AreEqual(new DateTime(2023, 11, 25), t3[11]);

            //445 Leap
            var t4 = FinancialExtensions.GetFiscalYearDates(2024);
            Assert.AreEqual(new DateTime(2024, 1, 1), t4[0]);
            Assert.AreEqual(new DateTime(2024, 1, 27), t4[1]);
            Assert.AreEqual(new DateTime(2024, 2, 24), t4[2]);
            Assert.AreEqual(new DateTime(2024, 3, 30), t4[3]);
            Assert.AreEqual(new DateTime(2024, 4, 27), t4[4]);
            Assert.AreEqual(new DateTime(2024, 5, 25), t4[5]);
            Assert.AreEqual(new DateTime(2024, 6, 29), t4[6]);
            Assert.AreEqual(new DateTime(2024, 7, 27), t4[7]);
            Assert.AreEqual(new DateTime(2024, 8, 24), t4[8]);
            Assert.AreEqual(new DateTime(2024, 9, 28), t4[9]);
            Assert.AreEqual(new DateTime(2024, 10, 26), t4[10]);
            Assert.AreEqual(new DateTime(2024, 11, 23), t4[11]);

            //445
            var t5 = FinancialExtensions.GetFiscalYearDates(2025);
            Assert.AreEqual(new DateTime(2025, 1, 1), t5[0]);
            Assert.AreEqual(new DateTime(2025, 1, 25), t5[1]);
            Assert.AreEqual(new DateTime(2025, 2, 22), t5[2]);
            Assert.AreEqual(new DateTime(2025, 3, 29), t5[3]);
            Assert.AreEqual(new DateTime(2025, 4, 26), t5[4]);
            Assert.AreEqual(new DateTime(2025, 5, 24), t5[5]);
            Assert.AreEqual(new DateTime(2025, 6, 28), t5[6]);
            Assert.AreEqual(new DateTime(2025, 7, 26), t5[7]);
            Assert.AreEqual(new DateTime(2025, 8, 23), t5[8]);
            Assert.AreEqual(new DateTime(2025, 9, 27), t5[9]);
            Assert.AreEqual(new DateTime(2025, 10, 25), t5[10]);
            Assert.AreEqual(new DateTime(2025, 11, 22), t5[11]);

            //545
            var t6 = FinancialExtensions.GetFiscalYearDates(2026);
            Assert.AreEqual(new DateTime(2026, 1, 1), t6[0]);
            Assert.AreEqual(new DateTime(2026, 1, 31), t6[1]);
            Assert.AreEqual(new DateTime(2026, 2, 28), t6[2]);
            Assert.AreEqual(new DateTime(2026, 4, 4), t6[3]);
            Assert.AreEqual(new DateTime(2026, 5, 2), t6[4]);
            Assert.AreEqual(new DateTime(2026, 5, 30), t6[5]);
            Assert.AreEqual(new DateTime(2026, 7, 4), t6[6]);
            Assert.AreEqual(new DateTime(2026, 8, 1), t6[7]);
            Assert.AreEqual(new DateTime(2026, 8, 29), t6[8]);
            Assert.AreEqual(new DateTime(2026, 10, 3), t6[9]);
            Assert.AreEqual(new DateTime(2026, 10, 31), t6[10]);
            Assert.AreEqual(new DateTime(2026, 11, 28), t6[11]);
        }
    }
}