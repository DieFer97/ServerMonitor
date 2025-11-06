using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Helpers;

public static class ServiceHelper
{
    public static T? GetService<T>() where T : class
    {
        if (Application.Current?.GetType().GetProperty("Services")?.GetValue(Application.Current) is IServiceProvider services)
        {
            return services.GetService(typeof(T)) as T;
        }
        return null;
    }
}

