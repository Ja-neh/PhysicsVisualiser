# PhysicsVisualiser: Application Architecture & UI Design

`PhysicsVisualiser` is the cross-platform presentation layer built with **.NET 10 MAUI**, implementing the **Model-View-ViewModel (MVVM)** pattern through `CommunityToolkit.Mvvm` and hosting a custom **SkiaSharp** drafting canvas.

---

## 1. User Interface Layout

The interface is structured into two main functional areas:

- **Simulation Panel (Left)**:
  - **Playback Controls**: `PLAY`, `PAUSE`, and `RESET` buttons, alongside a simulation time readout.
  - **Vector Toggles**: Checkboxes to toggle visibility of force and velocity vectors on the canvas.
  - **Liftoff Banner**: Alerts when upward applied force exceeds gravitational weight ($F_{\text{applied}, y} \ge mg$).
  - **2D Canvas (`SKCanvasView`)**: Displays the ground, moving block, grid coordinates, and active vectors.
- **Sidebar (Right)**:
  - **OUTPUT Panel**: Live numerical readouts for position, velocity, acceleration, normal force, friction forces, weight, and net forces.
  - **PARAMETERS Panel**: Numeric input fields for configuring mass, initial velocity, applied force, force angle, static/kinetic friction coefficients, and gravity.

---

## 2. MVVM Architecture

```text
[ MainPage.xaml (View) ]
        │  Two-Way Data Binding
        ▼
[ FlatSurfaceViewModel (ViewModel) ]
        │  Method Calls & Timer Ticks
        ▼
[ FlatSurface (Model / Solver) ] ──Produces──► [ FlatSurfaceState (Immutable Snapshot) ]
                                                        │
                                                        ▼
                                             [ FlatSurfaceRenderer (SkiaSharp) ]
```

### 2.1 View: `MainPage.xaml`
- **Simulation View**: Displays the interactive canvas, playback buttons (`PLAY`, `PAUSE`, `RESET`), elapsed time, vector toggles, and liftoff warnings.
- **Sidebar**: Hosts the `OUTPUT` measurement readouts and `PARAMETERS` input entries.

### 2.2 ViewModel: `FlatSurfaceViewModel`
Manages the application lifecycle, timer orchestration, and data synchronization:

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CanPlay))]
[NotifyPropertyChangedFor(nameof(CanPause))]
[NotifyCanExecuteChangedFor(nameof(PlayCommand))]
[NotifyCanExecuteChangedFor(nameof(PauseCommand))]
public partial bool IsRunning { get; set; }
```

- **Commands**:
  - `PlayCommand`: Validated by `CanPlay => !IsRunning`. Starts the 60 FPS `IDispatcherTimer`.
  - `PauseCommand`: Validated by `CanPause => IsRunning`. Halts the timer.
  - `RestartCommand`: Validated by `CanRestart => true`. Restores initial conditions and resets the camera.

---

## 3. Input Stability & Anti-Jitter Architecture

### The Problem with Direct Numeric Binding
Binding an editable `Entry.Text` directly to a `double` with a format string (e.g., `Text="{Binding UserMass, StringFormat='{0:F3}'}"`) causes an aggressive feedback loop:
1. Deleting a digit or typing a decimal point triggers `TextChanged`.
2. Two-way data binding parses the intermediate string and updates the ViewModel `double`.
3. The ViewModel raises `PropertyChanged`, causing MAUI to re-evaluate `{0:F3}` and push `"5.000"` back into the entry mid-keystroke.
4. Programmatically replacing text moves the cursor to the ends, scrambling subsequent keystrokes.

### The Decoupled String Solution
`FlatSurfaceViewModel` decouples user input strings from underlying numerical values:

```csharp
[ObservableProperty]
public partial string UserMassInput { get; set; } = "5.0";

public double UserMass { get; private set; } = 5.0;

partial void OnUserMassInputChanged(string value)
{
    if (TryParseDouble(value, out double val) && val > 0)
    {
        UserMass = val;
        _flatScenario.Mass = val;
        if (!IsRunning)
        {
            SyncViewWithSolver();
            RequestRepaint();
        }
    }
}
```

- **Smooth Keystrokes**: Users can freely backspace, delete decimals, or clear the box without text rewriting.
- **Culture-Agnostic Parsing**: `TryParseDouble` normalizes both commas (`5,2`) and periods (`5.2`).
- **Live Canvas Feedback**: Adjusting mass or applied force while paused immediately updates vectors and measurements on the screen.

---

## 4. Simulation Loop & Dispatching

The simulation runs on a high-precision `IDispatcherTimer` tied to the UI thread:

```csharp
private const double _fixedTimeStep = 1.0 / 60.0; // 60 FPS
private double _accumulatedTime = 0.0;

private void OnTimerTick(object? sender, EventArgs e)
{
    _accumulatedTime += _timer.Interval.TotalSeconds;

    while (_accumulatedTime >= _fixedTimeStep)
    {
        _flatScenario.Update(_fixedTimeStep);
        _accumulatedTime -= _fixedTimeStep;
    }

    SyncViewWithSolver();
    RequestRepaint();
}
```

- Accumulator logic ensures discrete integration stability even if system timer intervals experience minor OS scheduling variance.
- `RequestRepaint()` triggers `SKCanvasView.InvalidateSurface()`.
