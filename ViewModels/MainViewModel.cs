using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KanbanPopup.Models;
using KanbanPopup.Services;

namespace KanbanPopup.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly StorageService _storageService;
    private string _newTaskTitle = string.Empty;
    private string _newTaskDescription = string.Empty;

    public ObservableCollection<Column> Columns { get; } = [];
    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            if (_newTaskTitle == value) return;
            _newTaskTitle = value;
            OnPropertyChanged();
        }
    }

    public string NewTaskDescription
    {
        get => _newTaskDescription;
        set
        {
            if (_newTaskDescription == value) return;
            _newTaskDescription = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel(StorageService storageService)
    {
        _storageService = storageService;
        AddTaskCommand = new RelayCommand(_ => AddTask(), _ => !string.IsNullOrWhiteSpace(NewTaskTitle));
        DeleteTaskCommand = new RelayCommand(DeleteTask);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task InitializeAsync()
    {
        var loaded = await _storageService.LoadAsync();
        Columns.Clear();
        foreach (var column in loaded)
        {
            AttachCollection(column.Tasks);
            Columns.Add(column);
        }
        Columns.CollectionChanged += OnColumnsChanged;
    }

    public async Task SaveAsync() => await _storageService.SaveAsync(Columns);

    public async Task MoveTaskAsync(TaskItem task, Column source, Column target)
    {
        if (source == target) return;
        if (!source.Tasks.Remove(task)) return;
        target.Tasks.Add(task);
        await SaveAsync();
    }

    private async void AddTask()
    {
        var todo = Columns.FirstOrDefault(c => c.Type == ColumnType.ToDo);
        if (todo is null || string.IsNullOrWhiteSpace(NewTaskTitle)) return;

        todo.Tasks.Add(new TaskItem
        {
            Title = NewTaskTitle.Trim(),
            Description = string.IsNullOrWhiteSpace(NewTaskDescription) ? null : NewTaskDescription.Trim()
        });

        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        await SaveAsync();
    }

    private async void DeleteTask(object? param)
    {
        if (param is not TaskItem task) return;

        foreach (var column in Columns)
        {
            if (!column.Tasks.Contains(task)) continue;
            column.Tasks.Remove(task);
            await SaveAsync();
            return;
        }
    }

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<Column>())
            {
                AttachCollection(item.Tasks);
            }
        }
    }

    private void AttachCollection(ObservableCollection<TaskItem> tasks)
    {
        tasks.CollectionChanged -= OnTasksChanged;
        tasks.CollectionChanged += OnTasksChanged;
    }

    private async void OnTasksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await SaveAsync();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}
