using Serilog;
using Serilog.Configuration;

namespace Serilog.ApplicationInsightsExt;
public static class LoggerExtensions
{
    public static LoggerConfiguration WithOperationId(this LoggerEnrichmentConfiguration enrichConfiguration)
    {
        if (enrichConfiguration is null) throw new ArgumentNullException(nameof(enrichConfiguration));

        return enrichConfiguration.With<OperationIdEnricher>();
    }
}