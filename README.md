# PhysicsVisualiser

A 2D physics simulation and visualization application built with **.NET 10 MAUI** and **SkiaSharp**.

`PhysicsVisualiser` pairs a zero-dependency physics simulation engine (`PhysicsSolver`) with an interactive 2D SkiaSharp visualizer (`PhysicsVisualiser`).

---

## Key Features

- **Physics Simulation**: Calculates motion, forces, static and kinetic friction, and liftoff conditions at 60 FPS.
- **2D Canvas Rendering**: SkiaSharp based drawing with a meter grid, camera tracking, and optional force and velocity vectors.
- **Clear Measurement Readouts**: Displays live values for position, velocity, acceleration, normal force, and net forces.
- **Configurable Parameters**: Easily tweak mass, initial velocity, applied force (magnitude & angle), friction coefficients, and gravity.
- **Automated Test Suite**: unit tests verifying physical formulas and scenario calculations.

---

## Repository Structure

```text
PhysicsVisualiser/
├── PhysicsSolver/          # Core physics domain engine (pure .NET 10, zero UI dependencies)
│   ├── Bodies/             # Physical bodies (Box, etc.)
│   ├── Constants/          # Physical constants (g = 9.8 m/s²)
│   ├── Formulas/           # Pure mathematical formulas (Forces, Kinematics, Conversions)
│   ├── Quantities/         # Direction and vector helpers
│   └── Scenarios/          # Simulation scenarios (FlatSurface, states, segments)
│
├── PhysicsVisualiser/      # .NET MAUI 10 cross-platform graphical application(still a work in progress)
│   ├── Renderers/          # SkiaSharp rendering pipeline (FlatSurfaceRenderer)
│   ├── Resources/          # Design tokens, colors, and styles (Colors.xaml, Styles.xaml)
│   ├── ViewModels/         # MVVM presentation logic (FlatSurfaceViewModel)
│   └── MainPage.xaml       # Main application layout
│
├── PhysicsSolver.Tests/    # Automated xUnit test suite
│   ├── ConstantsTests.cs
│   ├── FlatSurfaceTests.cs
│   ├── ForcesTests.cs
│   └── KinematicsTests.cs
│
├── Architecture.md         # Detailed architectural design and data flow
├── Solver.md               # Physics engine, formulas, and integration model
├── Visualiser.md           # UI design, MVVM architecture, and user controls(still a work in progress)
├── Rendering.md            # SkiaSharp canvas, coordinate mapping, and grid math
└── Scenario.md             # Scenario lifecycle and guide to adding new scenarios
```

---

## Documentation Index

Explore the technical guides for in-depth explanations of every subsystem:

1. [**Architecture.md**](Architecture.md): System architecture, three-tier separation of concerns, and data flow.
2. [**Solver.md**](Solver.md): Mathematical formulations, friction models, contact mechanics, and discrete integration.
3. [**Visualiser.md**](Visualiser.md): MVVM architecture, user controls, and input stability.
4. [**Rendering.md**](Rendering.md): SkiaSharp graphics pipeline, camera tracking, and infinite grid mathematics.
5. [**Scenario.md**](Scenario.md): The `Scenario` contract, state lifecycle, and walkthrough for building new scenarios.

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `.NET MAUI` workload:
  ```powershell
  dotnet workload install maui
  ```
- Windows 10/11

### Building the Solution
Clone the repository and build using the .NET CLI:
```powershell
dotnet build PhysicsVisualiser.slnx
```

### Running Unit Tests
Execute the entire test suite across all physics domain components:
```powershell
dotnet test PhysicsSolver.Tests/PhysicsSolver.Tests.csproj
```
Expected output: **Passed: 150, Failed: 0**.

### Running the Visualizer App
Launch the desktop visualizer on Windows:
```powershell
dotnet build -t:Run -f net10.0-windows10.0.19041.0 PhysicsVisualiser/PhysicsVisualiser.csproj
```

---

## Scenario Roadmap

- **Flat Surface** (Implemented)
- **Inclined Plane**
- **Pulley System** (One body on surface, another hanging from table side, connected by a string)
- **Projectile Motion**

---

## Tech Stack
- **Framework**: [.NET 10](https://dotnet.microsoft.com/) & [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- **2D Graphics**: [SkiaSharp](https://github.com/mono/SkiaSharp) (`SkiaSharp.Views.Maui.Controls`)
- **MVVM Architecture**: [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- **Testing**: [xUnit](https://xunit.net/) & [FluentAssertions](https://fluentassertions.com/)
