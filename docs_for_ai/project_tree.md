# Project Tree

LaserMonitor/
├── LaserMonitor.sln

├── Laser.Core/
│   ├── Models/
│   │   ├── DailySummary.cs
│   │   ├── ErrorData.cs
│   │   ├── LogEvent.cs
│   │   ├── LossData.cs
│   │   ├── Machine.cs
│   │   ├── OperationInterval.cs
│   │   ├── OrderInfo.cs
│   │   ├── SheetInfo.cs
│   │   ├── SummaryResult.cs
│   │   ├── TargetMachines.cs
│   │   └── TimeEfficiencyResult.cs
│   │
│   ├── Parsers/
│   │   └── LogParser.cs
│   │
│   ├── Analyzers/
│   │   ├── BottleneckAnalyzer.cs
│   │   ├── ErrorAnalyzer.cs
│   │   ├── LossAnalyzer.cs
│   │   ├── MachineAnalyzer.cs
│   │   ├── OperationAnalyzer.cs
│   │   ├── ScheduleSplitter.cs
│   │   ├── SheetAnalyzer.cs
│   │   ├── SorterAnalyzer.cs
│   │   ├── SystemAnalyzer.cs
│   │   └── TimeEfficiencyAnalyzer.cs
│   │
│   ├── Builders/
│   │   ├── DailyReportBuilder.cs
│   │   └── KpiBuilder.cs
│   │
│   ├── Services/
│   │   ├── DashboardService.cs
│   │   └── SqliteService.cs
│   │
│   └── Laser.Core.csproj

├── Laser.GUI/
│   ├── Command/
│   │   └── RelayCommand.cs
│   │
│   ├── Converters/
│   │   └── ReferenceEqualsMultiConverter.cs
│   │
│   ├── Dashboard/
│   │   └── （今後拡張）
│   │
│   ├── Styles/
│   │   ├── CardStyles.xaml
│   │   └── Colors.xaml
│   │
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   │
│   ├── Views/
│   │   ├── BottomPanelView.xaml
│   │   ├── HeaderView.xaml
│   │   ├── KpiPanelView.xaml
│   │   └── TimelineView.xaml
│   │
│   ├── App.xaml
│   ├── AssemblyInfo.cs
│   └── MainWindow.xaml

├── Laser.CLI/
│   └── （省略）

└── Docs/
    ├── architecture.md
    ├── module_responsibilities.md
    ├── analyzer_responsibility_map.md
    ├── sqlite_design.md
    ├── ui_design_constraints.md
    └── AGENTS.md
