using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SourceBackend.Web.Extensions;

public static class HandlerRegistrationExtensions
{
    public static int AddFeatureHandlersFrom(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0) return 0;

        var count = 0;

        foreach (var asm in assemblies.Distinct())
        {
            IEnumerable<Type> types;
            try
            {
                // GetTypes puede lanzar ReflectionTypeLoadException
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Usa sólo los tipos que cargaron OK
                types = ex.Types.Where(t => t is not null)!;
            }

            var handlerTypes = types.Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name == "Handler" &&
                t.Namespace is not null &&
                t.Namespace.Contains(".Features.", StringComparison.Ordinal));

            foreach (var type in handlerTypes)
            {
                services.AddScoped(type); // AsSelf
                count++;
            }
        }

        return count;
    }
}