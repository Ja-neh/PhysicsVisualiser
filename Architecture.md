# Architecture: System Design & Data Flow

`PhysicsVisualiser` is structured around a strict **three-tier decoupled architecture**, ensuring that physical domain logic remains completely isolated from the user interface and graphics frameworks.

---

## 1. System Layers

```mermaid
graph TD
    subgraph UI ["Presentation Layer (PhysicsVisualiser)"]
        View[MainPage.xaml]
        VM[FlatSurfaceViewModel]
        Renderer[FlatSurfaceRenderer - SkiaSharp]
    end

    subgraph Domain ["Core Physics Domain (PhysicsSolver)"]
        Scenario[FlatSurface Scenario]
        Formulas[Forces & Kinematics]
        Bodies[Box Body Model]
        State[FlatSurfaceState Record]
    end

    subgraph Testing ["Verification Layer (PhysicsSolver.Tests)"]
        Tests[xUnit Test Suite - 150 Tests]
    end

    View <-->|Data Binding| VM
    VM -->|Configures & Updates| Scenario
    Scenario -->|Uses| Formulas
    Scenario -->|Updates| Bodies
    Scenario -->|Produces| State
    State -->|Consumed by| VM
    State -->|Rendered by| Renderer
    Tests -->|Directly Tests| Scenario
    Tests -->|Directly Tests| Formulas
```

---

## 2. Layer Responsibilities

### 2.1 Domain Layer: `PhysicsSolver`
- **Zero UI Dependencies**: Written in standard, platform-agnostic .NET 10.
- **Deterministic**: Contains no asynchronous scheduling, random number generation, or UI thread synchronization.
- **Pure Functions**: Formulas for forces, acceleration, and kinematic motion exist as pure static methods in `Forces.cs` and `Kinematics.cs`.
- **Immutable State Snapshots**: Simulation state is modeled using C# immutable records (`ScenarioState`, `FlatSurfaceState`).

### 2.2 Presentation Layer: `PhysicsVisualiser`
- **MVVM Pattern**: Built with `CommunityToolkit.Mvvm`, utilizing source-generated `[ObservableProperty]` and `[RelayCommand]` attributes.
- **Input Anti-Jitter**: Decouples active text typing from numerical properties to prevent formatting feedback loops and backspacing cursor jumps.
- **SkiaSharp Rendering**: Encapsulates 2D graphics in standalone renderer classes (`FlatSurfaceRenderer`) that accept read-only state records and an `SKCanvas`.

### 2.3 Verification Layer: `PhysicsSolver.Tests`
- Contains 150 automated unit tests verifying:
  - Theoretical stopping distances and times under friction.
  - Newton's second law ($F = ma$) under multi-force angles.
  - Normal force transitions, friction threshold clamping, and liftoff conditions.

---

## 3. Data Flow & Lifecycle

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant View as MainPage.xaml
    participant VM as FlatSurfaceViewModel
    participant Solver as FlatSurface (Solver)
    participant Renderer as FlatSurfaceRenderer (SkiaSharp)

    User->>View: Adjusts Mass or Applied Force
    View->>VM: UserMassInput = "10.0"
    VM->>VM: TryParseDouble("10.0") -> 10.0
    VM->>Solver: Mass = 10.0
    VM->>Solver: GetCurrentState()
    Solver-->>VM: FlatSurfaceState Snapshot
    VM->>View: Notify Live Outputs Updated
    VM->>Renderer: InvalidateSurface() / PaintSurface()

    Note over VM,Solver: On Play Command (60 FPS Timer)
    loop Every 16.6ms (IDispatcherTimer)
        VM->>Solver: Update(delta = 1/60s)
        Solver->>Solver: Integrate Kinematics & Friction
        Solver-->>VM: GetCurrentState()
        VM->>View: Update Real-Time Labels (Position, Velocity, etc.)
        VM->>Renderer: PaintSurface(canvas, state)
    end
```

---

## 4. Concurrency & State Safety

- **Single-Threaded UI Loop**: Simulation ticks are scheduled on the MAUI `IDispatcherTimer`, executing on the main application thread. This eliminates multi-threading locking overhead and prevents race conditions between solver calculations and canvas drawing.
- **Record Immutability**: Because `FlatSurfaceState` is an immutable record, snapshots can be passed to renderers, loggers, or exported without fear of data corruption or mid-frame mutation.
