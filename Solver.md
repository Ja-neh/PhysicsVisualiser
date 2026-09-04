# PhysicsSolver: Physics Engine & Mathematical Formulation

`PhysicsSolver` is the zero-dependency, pure C# computational core of `PhysicsVisualiser`. It models classical Newtonian mechanics, contact dynamics, friction thresholds, and numerical kinematics integration without any knowledge of graphics, UI, or rendering frameworks.

---

## 1. Architectural Principles

1. **Zero External Dependencies**: Depends strictly on the .NET 10 base class library. Can run as a standalone library in console apps, web servers, or cloud microservices.
2. **Determinism**: Given identical initial conditions and time steps ($dt$), simulation execution produces bitwise-identical results across platforms.
3. **Immutable State Snapshots**: State is broadcast via immutable record structures (`FlatSurfaceState`, `ScenarioState`), guaranteeing thread safety and eliminating race conditions between simulation and rendering.

---

## 2. Core Modules

```text
PhysicsSolver/
├── Bodies/
│   └── Box.cs                 # Physical body model (Mass, Dimensions, Initial Velocity)
├── Constants/
│   └── Constants.cs           # Physical constants (g = 9.8 m/s²)
├── Formulas/
│   ├── Conversions.cs         # Angular conversions (Degrees <-> Radians)
│   ├── Forces.cs              # Normal force, weight, friction, vector components
│   └── Kinematics.cs          # Motion equations (displacement, final velocity, stopping time)
├── Quantities/
│   ├── Direction.cs           # Direction enum (Positive, Negative, None)
│   └── DirectionXY.cs         # 2D Cartesian directions (Xpositive, Xnegative, Ypositive, Ynegative)
└── Scenarios/
    ├── Scenario.cs            # Abstract scenario base class
    └── FlatSurface.cs         # Complete horizontal flat surface mechanics solver
```

---

## 3. Mathematical & Physical Formulations

### 3.1 Force Decomposition
Applied forces acting on a body at an angle $\theta$ relative to the horizontal plane are decomposed into orthogonal Cartesian components:

$$F_{\text{applied}, x} = F_{\text{applied}} \cdot \cos(\theta)$$

$$F_{\text{applied}, y} = F_{\text{applied}} \cdot \sin(\theta)$$

Where:
- Positive $\theta$ indicates a force directed upward and forward (pulling).
- Negative $\theta$ indicates a force directed downward and forward (pushing into the surface).

### 3.2 Gravitational Force (Weight)
Weight acts purely along the negative vertical axis:

$$W = m \cdot g$$

Where $m$ is body mass (kg) and $g = 9.8 \text{ m/s}^2$ (standard Earth gravity).

### 3.3 Normal Force & Liftoff Dynamics
The normal force $N$ represents the perpendicular contact reaction provided by the surface:

$$N = W - F_{\text{applied}, y} = (m \cdot g) - F_{\text{applied}} \sin(\theta)$$

#### Liftoff Threshold:
If the upward component of the applied force equals or exceeds the gravitational weight:

$$F_{\text{applied}, y} \ge W \implies N \le 0$$

Under this condition:
- The normal force clamps to $0 \text{ N}$ (the ground cannot pull downward).
- Surface contact is lost, causing friction to immediately drop to $0 \text{ N}$.
- A boolean flag `LiftOffWarning = true` is set on the scenario state.

### 3.4 Friction Mechanics

Friction is modeled in two distinct physical regimes:

#### 1. Static Regime ($v = 0$):
The surface resists initiation of motion up to the maximum static threshold:

$$f_{s, \max} = \mu_s \cdot N$$

- If $|F_{\text{applied}, x}| \le f_{s, \max}$, the block remains stationary ($a_x = 0$, $v = 0$).
- The actual opposing static friction force equals the applied force exactly:

$$f_s = -F_{\text{applied}, x}$$

#### 2. Kinetic Regime ($v \neq 0$ or $|F_{\text{applied}, x}| > f_{s, \max}$):
Once motion begins, dynamic friction opposes the direction of velocity:

$$f_k = \mu_k \cdot N \cdot (-\operatorname{sgn}(v))$$

Where $\mu_s \ge \mu_k \ge 0$.

### 3.5 Net Force & Acceleration
Along the horizontal axis:

$$F_{\text{net}, x} = F_{\text{applied}, x} + f_{\text{friction}}$$

$$a_x = \frac{F_{\text{net}, x}}{m}$$

Along the vertical axis (while on surface):

$$F_{\text{net}, y} = 0$$

---

## 4. Discrete Time Integration

`FlatSurface` updates physical state via discrete fixed-time integration:

```csharp
public void Update(double delta)
```

### Stopping Condition & Numerical Jitter Prevention
A classic failure mode in numerical friction solvers is **artificial oscillation**: when friction decelerates a body to zero velocity, naive integration can cause velocity to flip sign and oscillate indefinitely.

`PhysicsSolver` solves this by calculating the exact stopping time $t_{\text{stop}}$ for deceleration under kinetic friction:

$$t_{\text{stop}} = \frac{|v_0|}{|a_x|}$$

If $t_{\text{stop}} < \Delta t$, the solver integrates motion for the remaining fraction $t_{\text{stop}}$, sets velocity exactly to $0$, and transitions immediately to the static friction evaluation for the remaining time slice.

---

## 5. State Immutability: `FlatSurfaceState`

Every evaluation produces an immutable record snapshot:

```csharp
public record FlatSurfaceState(
    double Time,
    double Mass,
    double Position,
    double Velocity,
    double Acceleration,
    double Normal,
    double Weight,
    double StaticFrictionCoefficient,
    double KineticFrictionCoefficient,
    double MaxStaticFriction,
    double StaticFriction,
    double KineticFriction,
    double AppliedForceX,
    double AppliedForceY,
    double FNetX,
    double FNetY,
    bool LiftOffWarning
) : ScenarioState();
```

This guarantees that the visualizer or logger can inspect the complete state of the physical system at any instant without mutex locking or risk of state mutation.
