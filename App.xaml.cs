using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace KanbanPopup;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "DispatcherUnhandledException");
        // Let default handler run so debugger can catch it when attached
        e.Handled = false;
    }

    private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex, "CurrentDomain_UnhandledException");
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "TaskScheduler_UnobservedTaskException");
        // Do not mark observed so the process behavior is unchanged
    }

    private static void LogException(Exception ex, string source)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KanbanPopup");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "error.log");
            var text = $"[{DateTime.UtcNow:O}] {source}\n{ex}\n----\n";
            File.AppendAllText(path, text);
        }
        catch
        {
            // Swallow logging exceptions to avoid secondary failures
        }
    }
}
