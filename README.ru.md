# Kanban Popup (WPF, .NET 10)

Минималистичная desktop Kanban-доска для Windows 10/11 с всплывающим окном от правого края экрана.

## Возможности

- 3 колонки: `ToDo`, `InProgress`, `Done`
- Карточки задач:
  - заголовок (обязательно)
  - описание (необязательно)
- Добавление задачи через верхние поля + кнопку `+`
- Удаление задачи из любой колонки
- Drag-and-drop карточек между колонками
- Всплывающая доска:
  - по умолчанию скрыта
  - появляется, когда курсор у **правого** края экрана
  - скрывается, когда курсор уходит (задержка ~500 мс)
  - плавная slide-анимация
- Глобальная горячая клавиша: `Ctrl + ~` (показать/скрыть)
- Работа через system tray:
  - Open
  - Exit
- Хранение в JSON:
  - автосохранение
  - автозагрузка при старте

## Технологии

- C#
- .NET 10 (`net10.0-windows`)
- WPF
- MVVM
- Без тяжелых зависимостей

## Структура проекта

```text
KanbanPopup/
├─ App.xaml
├─ App.xaml.cs
├─ KanbanPopup.csproj
├─ Models/
│  ├─ TaskItem.cs
│  ├─ ColumnType.cs
│  └─ Column.cs
├─ ViewModels/
│  ├─ RelayCommand.cs
│  └─ MainViewModel.cs
├─ Services/
│  ├─ MouseTrackerService.cs
│  ├─ HotkeyService.cs
│  └─ StorageService.cs
└─ Views/
   ├─ MainWindow.xaml
   └─ MainWindow.xaml.cs
```

## Требования

- Windows 10/11
- .NET 10 SDK
- Visual Studio 2022+ (рекомендуется) или `dotnet` CLI

## Запуск (Visual Studio)

1. Откройте папку/проект в Visual Studio.
2. Установите стартовый проект: `KanbanPopup`.
3. Нажмите `F5`.

## Запуск (CLI)

```powershell
cd d:\projects\kanban2
dotnet run
```

## Сборка

```powershell
cd d:\projects\kanban2
dotnet build -c Release
```

## Публикация EXE

### Self-contained (рекомендуется для распространения)

```powershell
cd d:\projects\kanban2
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Готовый файл:

`bin\Release\net10.0-windows\win-x64\publish\KanbanPopup.exe`

### Framework-dependent (меньше размер, нужен установленный runtime)

```powershell
cd d:\projects\kanban2
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## Хранение данных

Данные доски сохраняются в:

`%AppData%\KanbanPopup\board.json`

## Использование

- Подведите курсор к **правому краю** экрана, чтобы открыть доску.
- Уведите курсор с доски, чтобы скрыть ее.
- Нажмите `Ctrl + ~`, чтобы вручную переключить видимость.
- Используйте меню в трее (`Open` / `Exit`) для фоновой работы.

## Примечания

- Приложение живет в трее и не отображается в панели задач.
- Окно без рамки, поверх остальных окон, оптимизировано для быстрого доступа.
