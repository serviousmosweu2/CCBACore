using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public static class FinancialExtensions
    {
        public static List<DateTime> GetFiscalYearDates(int year)
        {
            if (year == 2026)
            {
                //Todo: Does not follow any identifiable pattern
                return FiveFourFive(year);
            }
            return DateTime.IsLeapYear(year - 1) && (year - 1) / 4 % 5 == 0 ? FiveFourFive(year) : FourFourFive(year);
        }

        public static DateTime GetFiscalYearDates(int year, EMonth month)
        {
            return GetFiscalYearDates(year)[(int)month];
        }

        public static decimal StringToDecimal(this string input)
        {
            var formatted = input.Trim().Replace(" ", "").Replace(",", "").Replace(".", ",");
            var isNegative = formatted.Contains("-");
            var sValue = formatted.Trim().Replace("-", "");
            var cultureInfo = CultureInfo.InvariantCulture;
            // if the first regex matches, the number string is in us culture
            if (Regex.IsMatch(sValue, @"^(:?[\d,]+\.)*\d+$"))
            {
                cultureInfo = new CultureInfo("en-US");
            }
            // if the second regex matches, the number string is in de culture
            else if (Regex.IsMatch(sValue, @"^(:?[\d.]+,)*\d+$"))
            {
                cultureInfo = new CultureInfo("de-DE");
            }
            var styles = NumberStyles.Number;
            if (!decimal.TryParse(sValue, styles, cultureInfo, out var dValue))
            {
                throw new Exception("Invalid number value");
            }

            if (isNegative)
            {
                dValue *= -1;
            }

            return dValue;
        }

        private static List<DateTime> FiveFourFive(int year)
        {
            var dateList = new List<DateTime>();
            var tempDate = new DateTime(year, 1, 1);
            dateList.Add(tempDate);

            var isLeapYear = DateTime.IsLeapYear(year);

            if (!isLeapYear)
            {
                var pattern = new List<int> { 5, 4, 5 };
                for (var j = 1; j <= 12; j++)
                {
                    while (true)
                    {
                        tempDate = tempDate.AddDays(1);
                        if (tempDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            var index = pattern.FindIndex(s => s != 0);
                            pattern[index]--;
                            if (pattern[index] == 0)
                            {
                                break;
                            }
                        }

                        if (pattern.All(s => s == 0))
                        {
                            pattern = new List<int> { 4, 4, 5 };
                        }
                    }
                    dateList.Add(tempDate);
                }
            }
            else
            {
                var pattern = new List<int> { 4, 4, 5 };
                for (var j = 1; j <= 12; j++)
                {
                    while (true)
                    {
                        tempDate = tempDate.AddDays(1);
                        if (tempDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            var index = pattern.FindIndex(s => s != 0);
                            pattern[index]--;
                            if (pattern[index] == 0)
                            {
                                break;
                            }
                        }

                        if (pattern.All(s => s == 0))
                        {
                            pattern = new List<int> { 4, 4, 5 };
                        }
                    }
                    dateList.Add(tempDate);
                }
            }

            return dateList;
        }

        private static List<DateTime> FourFourFive(int year)
        {
            var dateList = new List<DateTime>();
            var tempDate = new DateTime(year, 1, 1);
            dateList.Add(tempDate);

            var isLeapYear = DateTime.IsLeapYear(year);

            if (!isLeapYear)
            {
                var pattern = new List<int> { 4, 4, 5 };
                for (var j = 1; j <= 12; j++)
                {
                    while (true)
                    {
                        tempDate = tempDate.AddDays(1);
                        if (tempDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            var index = pattern.FindIndex(s => s != 0);
                            pattern[index]--;
                            if (pattern[index] == 0)
                            {
                                break;
                            }
                        }

                        if (pattern.All(s => s == 0))
                        {
                            pattern = new List<int> { 4, 4, 5 };
                        }
                    }
                    dateList.Add(tempDate);
                }
            }
            else
            {
                var pattern = new List<int> { 4, 4, 5 };
                for (var j = 1; j <= 12; j++)
                {
                    while (true)
                    {
                        tempDate = tempDate.AddDays(1);
                        if (tempDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            var index = pattern.FindIndex(s => s != 0);
                            pattern[index]--;
                            if (pattern[index] == 0)
                            {
                                break;
                            }
                        }

                        if (pattern.All(s => s == 0))
                        {
                            pattern = new List<int> { 4, 4, 5 };
                        }
                    }
                    dateList.Add(tempDate);
                }
            }

            return dateList;
        }
    }
}