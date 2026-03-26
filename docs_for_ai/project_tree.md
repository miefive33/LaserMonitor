# Project Tree（最新版）

LaserMonitor/
├── LaserMonitor.sln

├── Laser.Core/
│   ├── Models/
│   │   ├── LogEvent.cs
│   │   ├── OperationInterval.cs
│   │   ├── SheetInfo.cs
│   │   └── KpiData.cs
│   │
│   ├── Parsers/
│   │   └── LogParser.cs
│   │
│   ├── Analyzers/
│   │   ├── OperationAnalyzer.cs
│   │   ├── LossAnalyzer.cs
│   │   ├── BottleneckAnalyzer.cs
│   │   └── WeeklyAnalyzer.cs
│   │
│   ├── Builders/
│   │   └── KpiBuilder.cs
│   │
│   ├── Services/
│   │   └── SqliteService.cs
│   │
│   └── Laser.Core.csproj

├── Laser.GUI/
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   ├── HeaderView.xaml
│   │   ├── TimelineView.xaml
│   │   └── BottomPanelView.xaml
│   │
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   │
│   └── Laser.GUI.csproj

├── Laser.CLI/
│   ├── Program.cs
│   └── Laser.CLI.csproj

└── Docs/
    ├── architecture.md
    ├── module_responsibilities.md
    ├── analyzer_responsibility_map.md
    ├── sqlite_design.md
    └── ui_design_constraints.md