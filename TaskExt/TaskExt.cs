namespace TaskExtension;
public static class TaskExt
{
    public static async Task<T[]> WhenAll<T>(params Task<T>[] tasks)
    {
        var waitTask = Task.WhenAll(tasks);
        try
        {
            await waitTask;
        }
        catch (Exception ex)
        {
            //ignore
        }

        if (waitTask.Exception != null)
        {
            throw waitTask.Exception;
        }
        return waitTask.Result;
    }
    public static async Task<T[]> WhenAll<T>(IEnumerable<Task<T>> tasks)
    {
        var waitTask = Task.WhenAll(tasks);
        try
        {
            await waitTask;
        }
        catch (Exception ex)
        {
            //ignore
        }
        if (waitTask.Exception != null)
        {
            throw waitTask.Exception;
        }
        return waitTask.Result;
    }

    public static async Task WhenAll(IEnumerable<Task> tasks)
    {
        var waitTask = Task.WhenAll(tasks);
        try
        {
            await waitTask;
        }
        catch (Exception ex)
        {

        }

        if (waitTask.Exception != null)
            throw waitTask.Exception;

    }

    public static async Task WhenAll(params Task[] tasks)
    {
        var waitTask = Task.WhenAll(tasks);
        try
        {
            await waitTask;
        }
        catch (Exception ex)
        {

        }

        if (waitTask.Exception != null)
            throw waitTask.Exception;
    }
}
