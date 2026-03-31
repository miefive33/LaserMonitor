# Project Tree

LaserMonitor/
├── LaserMonitor.sln

├── Laser.Core/
│   ├── Models/
│   │   ├── DailySummary.cs
│   │   ├── ErrorData.cs
│   │   ├── LogEvent.cs
│   │   ├── LossData.cs
│   │   ├── OperationInterval.cs
│   │   ├── OrderInfo.cs
│   │   ├── SheetInfo.cs
│   │   └── TimeEfficiencyResult.cs
│   │
│   ├── Parsers/
│   │   └── LogParser.cs
│   │
│   ├── Analyzers/
│   │   ├── OperationAnalyzer.cs
│   │   ├── ScheduleSplitter.cs
│   │   ├── LossAnalyzer.cs
│   │   ├── ErrorAnalyzer.cs
│   │   ├── BottleneckAnalyzer.cs
│   │   ├── SheetAnalyzer.cs
│   │   └── TimeEfficiencyAnalyzer.cs
│   │
│   ├── Builders/
│   │   ├── DailyReportBuilder.cs
│   │   └── KpiBuilder.cs
│   │
│   ├── Services/
│   │   └── SqliteService.cs
│   │
│   ├── App.config
│   └── packages.config

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