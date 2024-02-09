using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace TimeClock.Helpers
{
    public class InjectExtension : IMarkupExtension
    {
        public Type Type { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(Type);
        }
    }
}

/**
 TODO - consider 
add to startup.cs 
public void ConfigureServices(IServiceCollection services)
{
    services.AddMyServices();
}

*/