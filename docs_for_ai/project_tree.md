# Project Tree (Latest)

## Solution

LaserMonitor.sln

---

## Core Layer

Laser.Core/

### Analyzers
- BottleneckAnalyzer.cs
- LossAnalyzer.cs
- OperationAnalyzer.cs
- SheetAnalyzer.cs
- TimeEfficiencyAnalyzer.cs

### Builders
- DailyReportBuilder.cs

### Models
- LogEvent.cs
- OperationInterval.cs
- OrderInfo.cs
- SheetInfo.cs

### Parsers
- LogParser.cs

### Services
- SqliteService.cs

### Config
- App.config
- packages.config

---

## GUI Layer

Laser.GUI/

### Command
- RelayCommand.cs

### Converters

### Dashboard

### Styles
- CardStyles.xaml
- Colors.xaml

### ViewModels
- MainViewModel.cs

### Views
- BottomPanelView.xaml
- HeaderView.xaml
- KpiPanelView.xaml
- TimelineView.xaml

### Root
- App.xaml
- AssemblyInfo.cs
- MainWindow.xaml

---

## Notes

- Debug / bin / obj は含まない
- この構造以外のファイルは基本存在しない前提