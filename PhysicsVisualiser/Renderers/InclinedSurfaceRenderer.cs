using PhysicsSolver.Scenarios;
using SkiaSharp;
using System;

namespace PhysicsVisualiser.Renderers;

public class InclinedSurfaceRenderer
{
    // PROPERTIES
    #region TOGGLES
    public bool ShowForceVectors { get; set; } = false;
    public bool ShowVelocityVectors { get; set; } = false;
    #endregion

    // FIELDS
    #region COLOURS
    private static readonly SKColor BgColour = new SKColor(206, 212, 220);          // Muted drafting paper / board (#CED4DC)
    private static readonly SKColor GridMinorColour = new SKColor(190, 197, 207);   // 1m graph paper line
    private static readonly SKColor GridMajorColour = new SKColor(160, 170, 184);   // 5m graph paper rule
    private static readonly SKColor AxisColour = new SKColor(47, 55, 70);           // Crisp dark ink ground axis
    private static readonly SKColor GridTextColour = new SKColor(68, 79, 96);       // Coordinate numbers

    private static readonly SKColor BoxFillColour = new SKColor(235, 239, 245);     // Block fill (contrasting light card)
    private static readonly SKColor BoxStrokeColour = new SKColor(15, 23, 42);      // Crisp black ink outline (2px)
    private static readonly SKColor BoxTextColour = new SKColor(15, 23, 42);        // Black ink mass text

    private static readonly SKColor ForceAppliedColour = new SKColor(220, 38, 38);  // Red pen (Fa)
    private static readonly SKColor ForceNormalColour = new SKColor(2, 132, 199);    // Blue pen (N)
    private static readonly SKColor ForceWeightColour = new SKColor(180, 83, 9);     // Graphite pen (W)
    private static readonly SKColor ForceFrictionColour = new SKColor(109, 40, 217); // Violet pen (f)
    private static readonly SKColor VelocityColour = new SKColor(4, 120, 87);       // Green pen (v)
    #endregion

    #region VIEW(CAMERA, BOX) SETUP
    private const float _pixelsPerMeter = 100f;

    private const float _gridOriginAdjustFactorX = 0.45f;
    private const float _gridOriginAdjustFactorY = 0.65f;

    private float _cameraPosition = 0f;
    private float _cameraPx = 0f;
    private const float _lerpFactor = 1f;

#if ANDROID
    private const float _boxWidthPx = 45f;
    private const float _boxHeightPx = 30f;
#else
    private const float _boxWidthPx = 60f;
    private const float _boxHeightPx = 45f;
#endif
    #endregion

    public void Render(SKCanvas canvas, SKImageInfo info, InclinedSurfaceState state)
    {
        if (state == null) return;

        canvas.Clear(BgColour);

        float widthPx = info.Width;
        float heightPx = info.Height;

        float xAnchorPx = widthPx * _gridOriginAdjustFactorX;
        float yAnchorPx = heightPx * _gridOriginAdjustFactorY;

        float boxPositionX = (float)state.Position;

        // Camera tracking along the incline
        _cameraPosition += (boxPositionX - _cameraPosition) * _lerpFactor;
        _cameraPx = _cameraPosition * _pixelsPerMeter;

        // Determine incline angle in radians and degrees
        float angleRad = (float)state.SurfaceInclination;
        if (angleRad == 0f && (state.WeightX != 0.0 || state.WeightY != 0.0))
        {
            angleRad = (float)Math.Atan2(-state.WeightX, -state.WeightY);
        }
        float angleDeg = angleRad * (180f / MathF.PI);

        canvas.Save();
        // Translate to origin anchor on canvas
        canvas.Translate(xAnchorPx, yAnchorPx);

        // Grid vertical lines and labels (unrotated, exactly vertical)
        DrawVerticalGrid(canvas, angleRad, widthPx, heightPx, xAnchorPx, yAnchorPx);

        // Draw horizontal ground reference baseline & angle arc at 0m mark when inclined
        DrawInclineBase(canvas, angleRad, widthPx, heightPx);

        // Rotate canvas counter-clockwise by angleDeg so local +X is along the incline
        canvas.RotateDegrees(-angleDeg);

        // Camera offset along the incline
        canvas.Translate(-_cameraPx, 0);

        // Incline surface axis
        DrawInclineSurfaceAxis(canvas, widthPx, heightPx);

        // Box sitting on the incline surface
        float boxPositionXPx = boxPositionX * _pixelsPerMeter;
        float boxPositionYPx = -(_boxHeightPx / 2f);

        DrawBox(canvas, boxPositionXPx, boxPositionYPx);

        // Vectors
        if (ShowForceVectors) DrawForceVectors(canvas, boxPositionXPx, boxPositionYPx, state);

        if (ShowVelocityVectors) DrawVelocityVectors(canvas, boxPositionXPx, boxPositionYPx, state);

        // Mass label on top of vectors so it is never crossed out
        DrawBoxMassLabel(canvas, boxPositionXPx, boxPositionYPx, state.Mass);

        canvas.Restore();
    }

    public void ResetCamera()
    {
        _cameraPosition = 0f;
        _cameraPx = 0f;
    }

    private void DrawVerticalGrid(SKCanvas canvas, float angleRad, float width, float height, float xAnchorPx, float yAnchorPx)
    {
        canvas.Save();

        using var minorGridPaint = new SKPaint
        {
            Color = GridMinorColour,
            StrokeWidth = 0.75f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var majorGridPaint = new SKPaint
        {
            Color = GridMajorColour,
            StrokeWidth = 1.25f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = GridTextColour,
            IsAntialias = true
        };

        using var gridBoldTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold);
        using var font = new SKFont(gridBoldTypeface, 16f);

        float cosAngle = MathF.Max(MathF.Cos(angleRad), 0.05f);
        float sinAngle = MathF.Sin(angleRad);

        float leftSpanPx = (xAnchorPx + 100f) / cosAngle;
        float rightSpanPx = (width - xAnchorPx + 100f) / cosAngle;

        float startM = MathF.Floor((_cameraPx - leftSpanPx) / _pixelsPerMeter) - 1f;
        float endM = MathF.Ceiling((_cameraPx + rightSpanPx) / _pixelsPerMeter) + 1f;

        float topY = -yAnchorPx;
        float bottomY = height - yAnchorPx;

        for (float m = startM; m <= endM; m += 1f)
        {
            float d = m * _pixelsPerMeter - _cameraPx;
            float xLine = d * cosAngle;

            // Only draw lines that fall within screen horizontal bounds
            if (xLine < -xAnchorPx - 20f || xLine > width - xAnchorPx + 20f)
            {
                continue;
            }

            bool isMajor = MathF.Abs(m % 5f) < 0.001f;

            if (isMajor)
            {
                canvas.DrawLine(xLine, bottomY, xLine, topY, majorGridPaint);
                float yRamp = -d * sinAngle;
                float rightOfGridLine = xLine + 6f;
                float belowRamp = yRamp + 22f;
                canvas.DrawText($"{m:0}m", rightOfGridLine, belowRamp, SKTextAlign.Left, font, textPaint);
            }
            else
            {
                canvas.DrawLine(xLine, bottomY, xLine, topY, minorGridPaint);
            }
        }

        canvas.Restore();
    }

    private void DrawInclineBase(SKCanvas canvas, float angleRad, float width, float height)
    {
        if (MathF.Abs(angleRad) < 0.001f) return;

        canvas.Save();
        // Since the camera is displaced by _cameraPx along the incline at angle angleRad:
        float originX = -_cameraPx * MathF.Cos(angleRad);
        float originY = _cameraPx * MathF.Sin(angleRad);

        using var baseLinePaint = new SKPaint
        {
            Color = AxisColour.WithAlpha(120),
            StrokeWidth = 1.5f,
            PathEffect = SKPathEffect.CreateDash(new[] { 6f, 4f }, 0),
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        // Horizontal ground reference line from the ramp base
        float baseLen = MathF.Min(width * 0.4f, 250f);
        canvas.DrawLine(originX, originY, originX + baseLen, originY, baseLinePaint);

        // Angle arc showing inclination
        float arcRadius = 45f;
        float angleDeg = angleRad * (180f / MathF.PI);

        using var arcPaint = new SKPaint
        {
            Color = GridTextColour,
            StrokeWidth = 1.5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var arcBuilder = new SKPathBuilder();
        var oval = new SKRect(originX - arcRadius, originY - arcRadius, originX + arcRadius, originY + arcRadius);
        arcBuilder.AddArc(oval, 0, -angleDeg);
        using var arcPath = arcBuilder.Detach();
        canvas.DrawPath(arcPath, arcPaint);

        // Angle text label
        using var font = new SKFont(SKTypeface.Default, 13f);
        font.Embolden = true;
        using var textPaint = new SKPaint
        {
            Color = GridTextColour,
            IsAntialias = true
        };

        float midAngleRad = -angleRad / 2f;
        float textDist = arcRadius + 18f;
        float textX = originX + textDist * MathF.Cos(midAngleRad);
        float textY = originY + textDist * MathF.Sin(midAngleRad) + 4f;

        canvas.DrawText($"{MathF.Abs(angleDeg):0.#}°", textX, textY, SKTextAlign.Center, font, textPaint);

        canvas.Restore();
    }

    private void DrawInclineSurfaceAxis(SKCanvas canvas, float width, float height)
    {
        using var axisPaint = new SKPaint
        {
            Color = AxisColour,
            StrokeWidth = 2.5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        float maxExtent = MathF.Max(width, height) * 2f;
        float startX = _cameraPx - maxExtent;
        float endX = _cameraPx + maxExtent;

        canvas.DrawLine(startX, 0, endX, 0, axisPaint);
    }

    private void DrawBox(SKCanvas canvas, float centerX, float centerY)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        using var boxFill = new SKPaint
        {
            Color = BoxFillColour,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var boxStroke = new SKPaint
        {
            Color = BoxStrokeColour,
            StrokeWidth = 2.5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        var rect = new SKRoundRect(new SKRect(-_boxWidthPx / 2f, -_boxHeightPx / 2f, _boxWidthPx / 2f, _boxHeightPx / 2f), 4f, 4f);
        canvas.DrawRoundRect(rect, boxFill);
        canvas.DrawRoundRect(rect, boxStroke);

        canvas.Restore();
    }

    private void DrawBoxMassLabel(SKCanvas canvas, float centerX, float centerY, double mass)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        using var textPaint = new SKPaint
        {
            Color = BoxTextColour,
            IsAntialias = true
        };

        using var massBoldTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold);
        using var font = new SKFont(massBoldTypeface, 15f);

        canvas.DrawText($"{mass:0.0} kg", 0, 5.5f, SKTextAlign.Center, font, textPaint);

        canvas.Restore();
    }

    private void DrawForceVectors(SKCanvas canvas, float centerX, float centerY, InclinedSurfaceState state)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        int numForces = 0;

        if (state.Normal != 0.0) numForces++;
        double totalWeight = Math.Sqrt(state.WeightX * state.WeightX + state.WeightY * state.WeightY);
        if (totalWeight != 0.0) numForces++;
        if (state.MaxStaticFriction != 0.0) numForces++;
        if (state.AppliedForceX != 0.0 || state.AppliedForceY != 0.0) numForces++;

        float scale = 1.5f;
        float lengthBudget = (float)numForces * (scale * _boxWidthPx);

        double forcesSum = Math.Abs(state.Normal) + totalWeight + Math.Abs(state.MaxStaticFriction);
        double appliedForce = Math.Sqrt(state.AppliedForceX * state.AppliedForceX + state.AppliedForceY * state.AppliedForceY);
        forcesSum += appliedForce;

        if (forcesSum < 0.001)
        {
            canvas.Restore();
            return;
        }

        float normalPortion = (float)(state.Normal / forcesSum) * lengthBudget;
        float weightPortion = (float)(totalWeight / forcesSum) * lengthBudget;
        float appliedForcePortion = (float)(appliedForce / forcesSum) * lengthBudget;

        // Normal: Perpendicular to incline pointing away from surface (-Y in local screen coordinates)
        if (state.Normal != 0.0)
        {
            float endX = 0f;
            float endY = -normalPortion;
            DrawArrow(canvas, endX, endY, ForceNormalColour, "N");
        }

        // Weight: Gravity vector. In rotated incline coordinates, its components are (WeightX, -WeightY)
        // Since WeightY is negative (-mg cos theta), -WeightY is positive (pointing down into surface)
        if (totalWeight > 0.0)
        {
            float endX = (float)(state.WeightX / totalWeight) * weightPortion;
            float endY = (float)(-state.WeightY / totalWeight) * weightPortion;
            DrawArrow(canvas, endX, endY, ForceWeightColour, "W");
        }

        // Friction: Along incline surface (local X)
        if (state.KineticFriction != 0.0)
        {
            float endY = 0f;
            float endX = (float)(state.KineticFriction / forcesSum) * lengthBudget;
            DrawArrow(canvas, endX, endY, ForceFrictionColour, "fk");
        }
        else if (state.StaticFriction != 0.0)
        {
            float endY = 0f;
            float endX = (float)(state.StaticFriction / forcesSum) * lengthBudget;
            DrawArrow(canvas, endX, endY, ForceFrictionColour, "fs");
        }

        // Applied Force: Components along and perpendicular to incline
        if (state.AppliedForceX != 0.0 || state.AppliedForceY != 0.0)
        {
            double componentsSum = Math.Abs(state.AppliedForceX) + Math.Abs(state.AppliedForceY);

            float endX = (float)(state.AppliedForceX / componentsSum) * appliedForcePortion;
            float endY = -(float)(state.AppliedForceY / componentsSum) * appliedForcePortion;
            DrawArrow(canvas, endX, endY, ForceAppliedColour, "Fa");
        }

        canvas.Restore();
    }

    private void DrawVelocityVectors(SKCanvas canvas, float centerX, float centerY, InclinedSurfaceState state)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        float scale = 1.2f;

        if (state.Velocity != 0.0)
        {
            float endY = 0f;
            float endX = (float)state.Velocity * scale * _boxWidthPx;
            DrawArrow(canvas, endX, endY, VelocityColour, "vx");
        }

        canvas.Restore();
    }

    private void DrawArrow(SKCanvas canvas, float endX, float endY, SKColor color, string label)
    {
        var (scaledX, scaledY) = VectorScaler.ScaleVector(endX, endY);

        // Arrow shaft
        using var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = 3f,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        canvas.DrawLine(0, 0, scaledX, scaledY, paint);

        // Arrow head
        using var headPaint = new SKPaint
        {
            Color = color,
            StrokeWidth = 5f,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        float angle = MathF.Atan2(scaledY, scaledX);

        float headLen = 12f;
        float headAngle = MathF.PI / 6; // 30 degrees

        float h1X = scaledX - headLen * MathF.Cos(angle - headAngle);
        float h1Y = scaledY - headLen * MathF.Sin(angle - headAngle);

        float h2X = scaledX - headLen * MathF.Cos(angle + headAngle);
        float h2Y = scaledY - headLen * MathF.Sin(angle + headAngle);

        using var headBuilder = new SKPathBuilder();
        headBuilder.MoveTo(scaledX, scaledY);
        headBuilder.LineTo(h1X, h1Y);
        headBuilder.LineTo(h2X, h2Y);
        headBuilder.Close();

        using var headPath = headBuilder.Detach();
        canvas.DrawPath(headPath, headPaint);

        // Label
        using var labelPaint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };

        float fontSize = 15f;
        using var labelFont = new SKFont(SKTypeface.Default, fontSize);
        labelFont.Embolden = true;

        float labelOffset = 18f;
        float labelX = scaledX + labelOffset * MathF.Cos(angle);
        float labelY = scaledY + labelOffset * MathF.Sin(angle);

        canvas.DrawText(label, labelX, labelY, SKTextAlign.Center, labelFont, labelPaint);
    }
}
