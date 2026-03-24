using System.Collections.ObjectModel;

namespace KanbanPopup.Models;

public class Column
{
    public ColumnType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<TaskItem> Tasks { get; set; } = [];
}
