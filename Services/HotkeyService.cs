using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace KanbanPopup.Services;

public class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint VkOem3 = 0xC0;

    private readonly IntPtr _handle;
    private readonly HwndSource _source;
    private readonly int _hotkeyId;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public HotkeyService(IntPtr handle)
    {
        _handle = handle;
        _hotkeyId = GetHashCode();
        _source = HwndSource.FromHwnd(handle) ?? throw new InvalidOperationException("Cannot get HwndSource");
        _source.AddHook(WndProc);
    }

    public bool Register()
    {
        _registered = RegisterHotKey(_handle, _hotkeyId, ModControl, VkOem3);
        return _registered;
    }

    public void Unregister()
    {
        if (_registered)
        {
            UnregisterHotKey(_handle, _hotkeyId);
            _registered = false;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _source.RemoveHook(WndProc);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
