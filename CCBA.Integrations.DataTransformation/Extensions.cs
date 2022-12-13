using CCBA.Integrations.DataTransformation.Interfaces;
using CCBA.Integrations.DataTransformation.Services;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace CCBA.Integrations.DataTransformation
{
    public static class Extensions
    {
        public static IServiceCollection AddDataTransformationService(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddDistributedMemoryCache();
            services.TryAddTransient<IDataTransformationService, DataTransformationService>();

            return services;
        }

        public static T MapDbEntity<T>(this SqlDataReader rd) where T : class, new()
        {
            var type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();
            var t = new T();

            for (var i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    var fieldName = rd.GetName(i);

                    if (members.Any(m => string.Equals(m.Name, fieldName, StringComparison.OrdinalIgnoreCase)))
                    {
                        accessor[t, fieldName] = Convert.ToString(rd.GetValue(i));
                    }
                }
            }

            return t;
        }
    }
}