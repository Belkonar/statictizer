using Microsoft.Extensions.DependencyInjection;
using shared.Services;

namespace service_deps.Services;

public class StorageFactory
{
    private readonly IServiceProvider _provider;

    public StorageFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ILocationStorage GetStorage(string type)
    {
        if (string.IsNullOrWhiteSpace(type) || type == "mongo")
        {
            return _provider.GetRequiredService<MongoLocationStorage>();
        }
        else
        {
            return _provider.GetRequiredService<S3LocationStorage>();
        }
    }
}
