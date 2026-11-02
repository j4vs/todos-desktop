using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Todos.Infrastructure.Windows;

// Observe clicks without swallowing them or changing the foreground application.
public sealed class OutsideClickMonitor : IDisposable
{
    private readonly HookProc _callback;
    private nint _hook;
    private long _generation;
    private volatile bool _disposed;

    public OutsideClickMonitor(Func<System.Windows.Point, bool, Action> prepareClick)
    {
        var dispatcher = System.Windows.Application.Current.Dispatcher;
        _callback = (code, message, data) =>
        {
            if (!_disposed && code >= 0 && (message == 0x0201 || message == 0x0204 || message == 0x0207 || message == 0x020B))
            {
                var point = Marshal.PtrToStructure<MouseHookData>(data).Point;
                var pendingClick = PreparePendingClick(() => prepareClick(new System.Windows.Point(point.X, point.Y), message == 0x0201));
                dispatcher.BeginInvoke(DispatcherPriority.Background, pendingClick);
            }
            return CallNextHookEx(nint.Zero, code, message, data);
        };
        Restart();
    }

    public void InvalidatePendingClicks() => Interlocked.Increment(ref _generation);

    private Action PreparePendingClick(Func<Action> prepareClick)
    {
        var generation = Interlocked.Read(ref _generation);
        var applyClick = prepareClick();
        return () => { if (!_disposed && generation == Interlocked.Read(ref _generation)) applyClick(); };
    }

    public void Restart()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        InvalidatePendingClicks();
        var next = SetWindowsHookEx(14, _callback, GetModuleHandle(null), 0);
        if (next == nint.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        var previous = _hook;
        _hook = next;
        if (previous != nint.Zero) UnhookWindowsHookEx(previous);
    }

    public void Dispose()
    {
        _disposed = true;
        InvalidatePendingClicks();
        if (_hook != nint.Zero) UnhookWindowsHookEx(_hook);
        _hook = nint.Zero;
        GC.KeepAlive(_callback);
    }

    private delegate nint HookProc(int code, nint message, nint data);
    [StructLayout(LayoutKind.Sequential)] private struct NativePoint { public int X; public int Y; }
    [StructLayout(LayoutKind.Sequential)] private struct MouseHookData
    {
        public NativePoint Point;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }
    [DllImport("user32.dll", SetLastError = true)] private static extern nint SetWindowsHookEx(int id, HookProc callback, nint module, uint thread);
    [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook, int code, nint message, nint data);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern nint GetModuleHandle(string? name);
}
