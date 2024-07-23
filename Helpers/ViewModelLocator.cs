
using Goddard.Clock.ViewModels;

namespace Goddard.Clock.Helpers;
public class ViewModelLocator
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PageHeaderViewModel PageHeaderViewModel => _serviceProvider.GetRequiredService<PageHeaderViewModel>();
}