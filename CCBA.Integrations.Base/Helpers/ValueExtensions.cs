using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CCBA.Integrations.Base.Helpers
{
    public static class ValueExtensions
    {
        /// <summary>
        /// Developer: Johan Nieuwenhuis
        /// </summary>
        /// <param name="o"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> FindNullOrEmptyValues(this object o, Type type = null)
        {
            var list = new List<string>();
            var pi = type?.GetProperties() ?? o.GetType().GetProperties();

            foreach (var p in pi)
            {
                if (p.GetValue(o, null) is IList oTheList)
                    foreach (var listItem in oTheList)
                    {
                        var genericArguments = p.PropertyType.GetGenericArguments();
                        if (genericArguments.Any())
                        {
                            var genericArgument = genericArguments[0];
                            list.AddRange(FindNullOrEmptyValues(listItem, genericArgument));
                        }
                        else
                        {
                            list.AddRange(FindNullOrEmptyValues(listItem, listItem.GetType()));
                        }
                    }

                if (pi.Count(s => string.IsNullOrEmpty(s.GetValue(o, null)?.ToString()?.Trim())) == 6)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(p.GetValue(o, null)?.ToString()?.Trim()))
                {
                    list.Add($@"{p.Name} has no value!");
                }
            }

            return list;
        }

        /// <summary>
        /// extension method to get a bool value from an object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool ToBooleanOrDefault(this object o, bool defaultValue)
        {
            var result = defaultValue;

            if (o == null) return result;
            try
            {
                switch (o.ToString()?.ToLower())
                {
                    case "yes":
                    case "true":
                    case "ok":
                    case "y":
                        result = true;
                        break;

                    case "no":
                    case "false":
                    case "n":
                        result = false;
                        break;

                    default:
                        result = bool.Parse(o.ToString());
                        break;
                }
            }
            catch
            {
                // ignore
            }

            return result;
        }
    }
}