using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.ApplicationInsightsExt;
public class OperationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null) return;
        string operationId = activity.Context.TraceId.ToString();
        string parentId = activity.Context.SpanId.ToString();
        //var activityIdArray = activity?.Id?.Split('-');
        //if (!(activityIdArray is null) || activityIdArray.Length > 1) operationId = activityIdArray[1];

        logEvent.AddPropertyIfAbsent(new LogEventProperty("Operation Id", new ScalarValue(operationId)));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("Parent Id", new ScalarValue(parentId)));

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("OperationId", new ScalarValue(operationId)));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ParentId", new ScalarValue(parentId)));
    }
}