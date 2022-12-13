using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Inject models and repositories.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void AddAddSingletonService<T>(this IFunctionsHostBuilder builder) where T : class
        {
            builder.Services.AddSingleton<T>();
        }

        /// <summary>
        /// Inject models and repositories.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void AddAddTransientService<T>(this IFunctionsHostBuilder builder) where T : class
        {
            builder.Services.AddTransient<T>();
        }

        /// <summary>
        /// Inject configuration settings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void AddSettings<T>(this IFunctionsHostBuilder builder) where T : class
        {
            builder.Services.AddOptions<T>().Configure<IConfiguration>((setting, configuration) =>
            {
                configuration.GetSection(typeof(T).Name).Bind(setting);
            });
        }
    }
}