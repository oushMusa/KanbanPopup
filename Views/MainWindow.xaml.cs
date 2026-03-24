using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using KanbanPopup.Models;
using KanbanPopup.Services;
using KanbanPopup.ViewModels;
using Application = System.Windows.Application;
using Forms = System.Windows.Forms;

namespace KanbanPopup.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly MouseTrackerService _mouseTracker;
    private readonly DispatcherTimer _hideTimer;
    private Forms.NotifyIcon? _notifyIcon;
    private HotkeyService? _hotkeyService;

    private bool _isBoardVisible;
    private bool _isExiting;
    private System.Windows.Point _dragStart;
    private TaskItem? _dragItem;
    private ColumnType _dragSourceType;

    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainViewModel(new StorageService());
        DataContext = _vm;

        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            HideBoardAnimated();
        };

        _mouseTracker = new MouseTrackerService();
        _mouseTracker.EdgeStateChanged += (_, atEdge) =>
        {
            if (atEdge)
            {
                _hideTimer.Stop();
                ShowBoardAnimated();
            }
            else if (_isBoardVisible && !IsMouseOver)
            {
                _hideTimer.Start();
            }
        };

        Loaded += OnLoaded;
        MouseEnter += (_, _) => _hideTimer.Stop();
        MouseLeave += (_, _) =>
        {
            if (_isBoardVisible)
            {
                _hideTimer.Start();
            }
        };
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _vm.InitializeAsync();
        PositionHidden();
        Hide();
        _mouseTracker.Start();
        SetupTray();
        SetupHotkey();
    }

    private void SetupTray()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Visible = true,
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Kanban Popup"
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => Dispatcher.Invoke(ShowBoardAnimated));
        menu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(ExitApplication));
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowBoardAnimated);
    }

    private void SetupHotkey()
    {
        var handle = new System.Windows.Interop.WindowInteropHelper(this).EnsureHandle();
        _hotkeyService = new HotkeyService(handle);
        _hotkeyService.Register();
        _hotkeyService.HotkeyPressed += (_, _) =>
        {
            if (_isBoardVisible) HideBoardAnimated();
            else ShowBoardAnimated();
        };
    }

    private void ShowBoardAnimated()
    {
        if (_isBoardVisible) return;

        PositionHidden();
        Show();
        Activate();
        _isBoardVisible = true;

        var targetTop = 10d;
        var targetLeft = SystemParameters.WorkArea.Right - Width - 10d;

        var topAnim = new DoubleAnimation(Top, targetTop, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var leftAnim = new DoubleAnimation(Left, targetLeft, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var opacityAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(160));

        BeginAnimation(TopProperty, topAnim);
        BeginAnimation(LeftProperty, leftAnim);
        BeginAnimation(OpacityProperty, opacityAnim);
    }

    private void HideBoardAnimated()
    {
        if (!_isBoardVisible) return;
        _isBoardVisible = false;

        var hiddenTop = -Height - 20d;
        var hiddenLeft = SystemParameters.WorkArea.Right - Width + 18d;

        var topAnim = new DoubleAnimation(Top, hiddenTop, TimeSpan.FromMilliseconds(140))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        var leftAnim = new DoubleAnimation(Left, hiddenLeft, TimeSpan.FromMilliseconds(140))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        var opacityAnim = new DoubleAnimation(Opacity, 0.0, TimeSpan.FromMilliseconds(120));
        opacityAnim.Completed += (_, _) => Hide();

        BeginAnimation(TopProperty, topAnim);
        BeginAnimation(LeftProperty, leftAnim);
        BeginAnimation(OpacityProperty, opacityAnim);
    }

    private void PositionHidden()
    {
        Width = 980;
        Height = 520;
        Left = SystemParameters.WorkArea.Right - Width + 18;
        Top = -Height - 20;
        Opacity = 0;
    }

    private void ExitApplication()
    {
        _isExiting = true;
        _notifyIcon?.Dispose();
        _hotkeyService?.Dispose();
        _mouseTracker.Dispose();
        Application.Current.Shutdown();
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isExiting) return;

        e.Cancel = true;
        await _vm.SaveAsync();
        HideBoardAnimated();
    }

    private void TaskCard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(null);
        if (sender is not System.Windows.Controls.ListBox listBox) return;

        _dragItem = FindTaskFromVisual(e.OriginalSource as DependencyObject);
        if (_dragItem is null) return;

        if (listBox.Tag is ColumnType sourceType)
        {
            _dragSourceType = sourceType;
        }
    }

    private async void ListBox_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (sender is not System.Windows.Controls.ListBox targetList) return;
        if (!e.Data.GetDataPresent(typeof(TaskItem))) return;
        if (e.Data.GetData(typeof(TaskItem)) is not TaskItem task) return;
        if (targetList.Tag is not ColumnType targetType) return;

        var source = _vm.Columns.First(c => c.Type == _dragSourceType);
        var target = _vm.Columns.First(c => c.Type == targetType);
        await _vm.MoveTaskAsync(task, source, target);
    }

    private void TaskCard_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragItem is null) return;

        var current = e.GetPosition(null);
        if (Math.Abs(current.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(current.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        System.Windows.DragDrop.DoDragDrop(this, new System.Windows.DataObject(typeof(TaskItem), _dragItem), System.Windows.DragDropEffects.Move);
        _dragItem = null;
    }

    private static TaskItem? FindTaskFromVisual(DependencyObject? source)
    {
        var current = source;
        while (current is not null)
        {
            if (current is FrameworkElement fe && fe.DataContext is TaskItem task)
            {
                return task;
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
