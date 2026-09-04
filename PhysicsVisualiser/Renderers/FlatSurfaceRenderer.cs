using PhysicsSolver.Scenarios;
using PhysicsVisualiser.ViewModels;
using SkiaSharp;
using System;

namespace PhysicsVisualiser.Renderers;

public class FlatSurfaceRenderer
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


    public void Render(SKCanvas canvas, SKImageInfo info, FlatSurfaceState state)
    {
        canvas.Clear(BgColour);

        float widthPx = info.Width;
        float heightPx = info.Height;

        float xAnchorPx = widthPx * _gridOriginAdjustFactorX;
        float yAnchorPx = heightPx * _gridOriginAdjustFactorY;
        
        float boxPositionX = (float)state.Position;
        
        // camera
        _cameraPosition += (boxPositionX - _cameraPosition) * _lerpFactor;
        _cameraPx = _cameraPosition * _pixelsPerMeter;

        canvas.Save();
        canvas.Translate( - _cameraPx, 0);

        // grid
        DrawGrid(canvas, xAnchorPx, yAnchorPx, widthPx, heightPx);

        // box
        float boxPositionXPx = xAnchorPx + boxPositionX * _pixelsPerMeter;
        float boxPositionYPx = yAnchorPx - (_boxHeightPx / 2f);

        DrawBox(canvas, boxPositionXPx, boxPositionYPx);

        // vectors
        if (ShowForceVectors) DrawForceVectors(canvas, boxPositionXPx, boxPositionYPx, state);
        
        if (ShowVelocityVectors) DrawVelocityVectors(canvas, boxPositionXPx, boxPositionYPx, state);

        // mass label on top of vectors so it is never crossed out
        DrawBoxMassLabel(canvas, boxPositionXPx, boxPositionYPx, state.Mass);

        canvas.Restore();
        
    }

    public void ResetCamera()
    {
        _cameraPosition = 0f;
    }

    private void DrawGrid(SKCanvas canvas, float originX, float originY, float width, float height)
    {
        canvas.Save();
        canvas.Translate(originX, originY);

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

        using var axisPaint = new SKPaint
        {
            Color = AxisColour,
            StrokeWidth = 2.5f,
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

        // Continuous visible meter range relative to camera
        float visibleLeftM = (_cameraPx - originX) / _pixelsPerMeter;
        float visibleRightM = (_cameraPx + width - originX) / _pixelsPerMeter;

        float startM = MathF.Floor(visibleLeftM - 2f);
        float endM = MathF.Ceiling(visibleRightM + 2f);

        float upLength = height * _gridOriginAdjustFactorY;
        float downLength = height * (1 - _gridOriginAdjustFactorY);

        for (float m = startM; m <= endM; m += 1f)
        {
            float xPx = m * _pixelsPerMeter;
            bool isMajor = MathF.Abs(m % 5f) < 0.001f;

            if (isMajor)
            {
                canvas.DrawLine(xPx, downLength, xPx, -upLength, majorGridPaint);
                float rightOfGridLine = xPx + 6f;
                float belowXAxis = 22f;
                canvas.DrawText($"{m:0}m", rightOfGridLine, belowXAxis, SKTextAlign.Left, font, textPaint);
            }
            else
            {
                canvas.DrawLine(xPx, downLength, xPx, -upLength, minorGridPaint);
            }
        }

        canvas.DrawLine(startM * _pixelsPerMeter, 0, endM * _pixelsPerMeter, 0, axisPaint);

        canvas.Restore();
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

    private void DrawForceVectors(SKCanvas canvas, float centerX, float centerY, FlatSurfaceState state)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        int numForces = 0;

        if (state.Normal != 0.0) numForces++;
        if (state.Weight != 0.0) numForces++;
        if (state.MaxStaticFriction != 0.0) numForces++; // using MaxStaticFriction since it's always present when there is friction coefficient
        if (state.AppliedForceX != 0.0 || state.AppliedForceY != 0.0) numForces++;

        float scale = 1.5f;
        float lengthBudget = (float)numForces * ( scale * _boxWidthPx);

        double forcesSum = Math.Abs(state.Normal) + Math.Abs(state.Weight) + Math.Abs(state.MaxStaticFriction);
        double appliedForce = Math.Sqrt(state.AppliedForceX * state.AppliedForceX + state.AppliedForceY * state.AppliedForceY);
        forcesSum += appliedForce;

        float normalPortion = (float)(state.Normal / forcesSum) * lengthBudget;
        float weightPortion = (float)(state.Weight / forcesSum) * lengthBudget;
        float appliedForcePortion = (float)(appliedForce / forcesSum) * lengthBudget;

        // Normal
        if (state.Normal != 0.0)
        {
            float endX = 0f;
            float endY = - normalPortion;
            DrawArrow(canvas, endX, endY, ForceNormalColour, "N");
        }

        if (state.Weight != 0.0)
        {
            float endX = 0f;
            float endY = Math.Abs(weightPortion);
            DrawArrow(canvas, endX, endY, ForceWeightColour, "W");
        }


        if (state.KineticFriction != 0.0)
        {
            float endY = 0f;
            float endX = (float)(state.KineticFriction / forcesSum) * lengthBudget;
            DrawArrow(canvas, endX, endY, ForceFrictionColour, "fk");
        }
        else if(state.StaticFriction != 0)
        {
            float endY = 0f;
            float endX = (float)(state.StaticFriction / forcesSum) * lengthBudget;
            DrawArrow(canvas, endX, endY, ForceFrictionColour, "fs");
        }

        if (state.AppliedForceX != 0.0 || state.AppliedForceY != 0.0)
        {
            double componentsSum = Math.Abs(state.AppliedForceX) + Math.Abs(state.AppliedForceY);

            float endX = (float)(state.AppliedForceX / componentsSum) * appliedForcePortion;
            float endY = - (float)(state.AppliedForceY / componentsSum) * appliedForcePortion;
            DrawArrow(canvas, endX, endY, ForceAppliedColour, "Fa");
        }

        canvas.Restore();
    }

    private void DrawVelocityVectors(SKCanvas canvas, float centerX, float centerY, FlatSurfaceState state)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        float scale = 1.2f;

        if(state.Velocity != 0.0)
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
