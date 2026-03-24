using System.Windows.Forms;
using System.Windows.Threading;

namespace KanbanPopup.Services;

public class MouseTrackerService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private int _screenWidth;
    private int _screenHeight;

    public event EventHandler<bool>? EdgeStateChanged;

    public MouseTrackerService()
    {
        RefreshScreenBounds();

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _timer.Tick += OnTick;
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    public void RefreshScreenBounds()
    {
        _screenWidth = Screen.PrimaryScreen?.Bounds.Width ?? 1920;
        _screenHeight = Screen.PrimaryScreen?.Bounds.Height ?? 1080;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var p = Cursor.Position;
        var isNearTop = p.Y <= 1;
        var isNearRight = p.X >= _screenWidth - 1 && p.Y >= 0 && p.Y <= _screenHeight;
        EdgeStateChanged?.Invoke(this, isNearTop || isNearRight);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }
}
