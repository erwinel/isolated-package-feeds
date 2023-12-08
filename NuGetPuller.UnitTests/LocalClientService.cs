using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NuGetPuller.UnitTests;

public sealed class LocalClientService(IOptions<TestAppSettings> options, ILogger<LocalClientService> logger) : LocalClientService<TestAppSettings>(options, logger)
{
}
