using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ShutdownServiceExt;
public class ShutdownService : IHostedService
{

    private readonly IHostApplicationLifetime _appLifeTime;
    private readonly ILogger<ShutdownService> _logger;
    public static List<Task> ShutdownTaskList { get { if (_shutdownTaskList is null) _shutdownTaskList = new List<Task>(); return _shutdownTaskList; } }
    private static List<Task> _shutdownTaskList;
    public static List<Task> StartUptaskList { get { if (_startupTaskList is null) _startupTaskList = new List<Task>(); return _startupTaskList; } }
    private static List<Task> _startupTaskList;
    private bool _isStop;

    public ShutdownService(IHostApplicationLifetime appLifeTime, ILogger<ShutdownService> logger)
    {
        _logger = logger;
        _appLifeTime = appLifeTime;
        _isStop = false;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(" Shutdown Service: Start Async ....");

        var StartupTask = Task.WhenAll(ShutdownService.StartUptaskList.ToArray());
        ShutdownService.StartUptaskList.ForEach(x=>x.Start());
        try
        {
            await StartupTask;
        }
        catch (Exception ex)
        {

        }
        if (!(StartupTask.Exception is null))
        {
            _logger.LogError(StartupTask.Exception, "Error while Shutdown Servie Start Async");
        }
        _logger.LogInformation(" Shutdown Service: Start Async .... Complete");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(" ShutdownService: Stop Async ....", DateTime.Now);
        this._isStop = true;
        var ShutdownTask = Task.WhenAll(ShutdownService.ShutdownTaskList.ToArray());       
        ShutdownService.ShutdownTaskList.ForEach(x=>x.Start());
        try
        {
            _logger.LogInformation("Start await Shutdown Task");
            await ShutdownTask;
        }
        catch (Exception ex)
        {
        
        }
        if (!(ShutdownTask.Exception is null))
        {
            _logger.LogError(ShutdownTask.Exception, "Error While Shutdown Service Stop Async");
        }

        _logger.LogInformation("ShutdownService: Stop Async .... Complete");

    }


}
