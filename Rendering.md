# Rendering: SkiaSharp Graphics Pipeline & Visual Calculations

`PhysicsVisualiser` uses **SkiaSharp** (`SkiaSharp.Views.Maui.Controls`) to render real-time physics visualizations with high performance and sub-pixel accuracy. All visual drawing logic is encapsulated in `FlatSurfaceRenderer.cs`.

---

## 1. Coordinate Systems & Scaling

The renderer bridges two distinct coordinate spaces:

1. **Physics World Space (Meters)**:
   - Origin $(0, 0)$ is the block's initial contact point on the surface.
   - Horizontal axis: $+X$ points right.
   - Vertical axis: $+Y$ points upward.
2. **Screen Canvas Space (Pixels)**:
   - Origin $(0, 0)$ is the top-left corner of the canvas.
   - Horizontal axis: $+X$ points right.
   - Vertical axis: $+Y$ points downward.

### Transformation Constants:
- **Scale**: $\text{PixelsPerMeter} = 80.0 \text{ px/m}$.
- **Ground Line**: Placed at $70\%$ of canvas height:
  $$Y_{\text{ground}} = \text{Height} \times 0.70$$
- **Block Size**: A standard $1.0\text{ m} \times 1.0\text{ m}$ body corresponds to an $80\text{ px} \times 80\text{ px}$ square.

---

## 2. Dynamic Camera Tracking

To ensure continuous visibility as the block accelerates across the surface, the renderer implements smooth horizontal camera tracking:

```csharp
private float _cameraPx = 0f;

public void Draw(SKCanvas canvas, SKImageInfo info, FlatSurfaceState state)
{
    float boxWorldPx = (float)state.Position * PixelsPerMeter;
    float targetCameraLead = info.Width * 0.35f;
    
    // Smooth camera tracking offset
    _cameraPx = Math.Max(0f, boxWorldPx - targetCameraLead);
    ...
}
```

- **Leading Offset ($35\%$ Width)**: The camera keeps the block in the left-third of the screen, giving the user an unobstructed view of the upcoming track and motion vectors.
- **`ResetCamera()`**: Re-anchors the view back to the starting point ($0\text{ m}$) upon simulation restart.

---

## 3. Continuous Infinite Grid Mathematics

Rather than rendering a static grid that scrolls off the screen, the grid is calculated dynamically relative to the camera's current position:

```csharp
float leftWorldMeter = (float)Math.Floor((_cameraPx - 100f) / PixelsPerMeter);
float rightWorldMeter = (float)Math.Ceiling((_cameraPx + info.Width + 100f) / PixelsPerMeter);

for (float m = leftWorldMeter; m <= rightWorldMeter; m += 1.0f)
{
    float screenX = (m * PixelsPerMeter) - _cameraPx;
    
    // Draw vertical grid line
    canvas.DrawLine(screenX, 0, screenX, info.Height, _gridLinePaint);
    
    // Draw coordinate marker
    string label = $"{m:0}m";
    canvas.DrawText(label, screenX + 4f, groundY + 22f, _gridLabelPaint);
}
```

### Key Properties:
- **Viewport Culling**: Only grid lines currently within the visible screen bounds are computed and drawn, maintaining $O(1)$ constant rendering time regardless of distance traveled.

---

## 4. Vector Arrow Architecture

When enabled via the UI toggles, force and velocity vectors are drawn from the center of the block.

### 4.1 Arrow Sizing & Clamping
Vector lengths are scaled proportionally according to magnitude, bounded by minimum and maximum screen pixel lengths:

$$\text{Length}_{\text{arrow}} = \text{clamp}\left(\frac{\text{Magnitude}}{\text{ScaleFactor}}, \ 25\text{ px}, \ 160\text{ px}\right)$$

### 4.2 Trigonometric Arrowhead Generation
Arrowheads are rendered using standard trigonometric rotations:

```csharp
private void DrawArrow(SKCanvas canvas, float startX, float startY, float endX, float endY, SKPaint paint)
{
    // Draw shaft
    canvas.DrawLine(startX, startY, endX, endY, paint);

    // Calculate angle
    float angle = (float)Math.Atan2(endY - startY, endX - startX);
    float headLength = 12f;
    float headAngle = (float)(Math.PI / 6.0); // 30 degrees

    SKPath headPath = new SKPath();
    headPath.MoveTo(endX, endY);
    headPath.LineTo(
        endX - headLength * (float)Math.Cos(angle - headAngle),
        endY - headLength * (float)Math.Sin(angle - headAngle));
    headPath.LineTo(
        endX - headLength * (float)Math.Cos(angle + headAngle),
        endY - headLength * (float)Math.Sin(angle + headAngle));
    headPath.Close();

    canvas.DrawPath(headPath, paint);
}
```

### 4.3 Visual Layering Order
To prevent visual artifacts (e.g., arrow shafts slicing through the mass label), elements are drawn strictly in back-to-front painter's order:

1. Background
2. Coordinate Grid & Distance Labels
3. Ground Plane
4. Block
5. Vector Shafts
6. Mass Label
7. Vector Arrowheads & Badges
