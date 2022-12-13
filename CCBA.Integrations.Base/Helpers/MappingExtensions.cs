using System.Collections.Generic;
using System.Linq;

namespace CCBA.Integrations.Base.Helpers
{
    public static class MappingExtensions
    {
        public static IEnumerable<T> MapList<T>(IEnumerable<object> input) where T : class, new()
        {
            var output = new List<T>();
            foreach (var u in input)
            {
                var obj = new T();
                var inputProps = u.GetType().GetProperties().ToList();
                foreach (var t in inputProps)
                {
                    obj.GetType().GetProperty(t.Name)?.SetValue(obj, t.GetValue(u));
                }
                output.Add(obj);
            }
            return output;
        }
    }
}