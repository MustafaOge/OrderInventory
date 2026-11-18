using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OrderInventory.Application.AutoMapper;

namespace OrderInventory.Tests.Services;

internal static class TestMapper
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(
            expression => expression.AddProfile<MapperProfile>(),
            NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }
}
