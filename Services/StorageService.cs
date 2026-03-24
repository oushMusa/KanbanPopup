using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using KanbanPopup.Models;

namespace KanbanPopup.Services;

public class StorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public StorageService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KanbanPopup");

        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "board.json");
    }

    public async Task<ObservableCollection<Column>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return CreateDefaultColumns();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var data = JsonSerializer.Deserialize<ObservableCollection<Column>>(json, JsonOptions);
            if (data is null || data.Count == 0)
            {
                return CreateDefaultColumns();
            }

            EnsureAllColumns(data);
            return data;
        }
        catch
        {
            return CreateDefaultColumns();
        }
    }

    public async Task SaveAsync(ObservableCollection<Column> columns)
    {
        var json = JsonSerializer.Serialize(columns, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    private static ObservableCollection<Column> CreateDefaultColumns()
    {
        return
        [
            new Column { Type = ColumnType.ToDo, Title = "ToDo" },
            new Column { Type = ColumnType.InProgress, Title = "InProgress" },
            new Column { Type = ColumnType.Done, Title = "Done" }
        ];
    }

    private static void EnsureAllColumns(ObservableCollection<Column> columns)
    {
        var map = columns.ToDictionary(c => c.Type, c => c);
        if (!map.ContainsKey(ColumnType.ToDo))
        {
            columns.Insert(0, new Column { Type = ColumnType.ToDo, Title = "ToDo" });
        }
        if (!map.ContainsKey(ColumnType.InProgress))
        {
            columns.Add(new Column { Type = ColumnType.InProgress, Title = "InProgress" });
        }
        if (!map.ContainsKey(ColumnType.Done))
        {
            columns.Add(new Column { Type = ColumnType.Done, Title = "Done" });
        }

        foreach (var column in columns)
        {
            column.Tasks ??= [];
            column.Title = column.Type switch
            {
                ColumnType.ToDo => "ToDo",
                ColumnType.InProgress => "InProgress",
                _ => "Done"
            };
        }
    }
}
