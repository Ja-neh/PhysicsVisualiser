using PhysicsEngine.Scenarios;
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
    private static readonly SKColor BgColour = SKColors.White;
    private static readonly SKColor GridColour = new SKColor(51, 65, 85, 90);
    private static readonly SKColor AxisColour = new SKColor(100, 116, 139, 140);

    private static readonly SKColor BoxFillColour = new SKColor(79, 70, 229);
    private static readonly SKColor BoxStrokeColour = new SKColor(165, 180, 252);
    private static readonly SKColor BoxTextColour = SKColors.White;

    private static readonly SKColor ForceAppliedColour = new SKColor(239, 68, 68);
    private static readonly SKColor ForceNormalColour = new SKColor(14, 165, 233);
    private static readonly SKColor ForceWeightColour = new SKColor(234, 179, 8);
    private static readonly SKColor ForceFrictionColour = new SKColor(168, 85, 247);
    private static readonly SKColor VelocityColour = new SKColor(16, 185, 129);
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
        DrawGrid(canvas, xAnchorPx, yAnchorPx, heightPx);

        // box
        float boxPositionXPx = xAnchorPx + boxPositionX * _pixelsPerMeter;
        float boxPositionYPx = yAnchorPx - (_boxHeightPx / 2f);

        DrawBox(canvas, boxPositionXPx, boxPositionYPx, state.Mass);

        // vectors
        if (ShowForceVectors) DrawForceVectors(canvas, boxPositionXPx, boxPositionYPx, state);
        
        if (ShowVelocityVectors) DrawVelocityVectors(canvas, boxPositionXPx, boxPositionYPx, state);

        canvas.Restore();
        
    }

    public void ResetCamera()
    {
        _cameraPosition = 0f;
    }

    private void DrawGrid(SKCanvas canvas, float originX, float originY, float height)
    {
        canvas.Save();
        canvas.Translate(originX, originY);

        using var gridPaint = new SKPaint
        {
            Color = GridColour,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var axisPaint = new SKPaint
        {
            Color = AxisColour,
            StrokeWidth = 5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = new SKColor(148, 163, 184, 180),
            IsAntialias = true
        };

        using var font = new SKFont(SKTypeface.Default, 10f);

        // meters
        float leftLimit = - 100f;
        float rightLimit = 100f;
        float gridSpacing = 5f;

        for(float gridLine = leftLimit; gridLine <= rightLimit; gridLine += gridSpacing)
        {
            float xPx = gridLine * _pixelsPerMeter;

            float upLength = height * _gridOriginAdjustFactorY;
            float downLength = height * (1 - _gridOriginAdjustFactorY);
            canvas.DrawLine(xPx, downLength, xPx, -upLength, gridPaint);

            float rightOfGridLine = xPx + 4f;
            float belowXAxis = 20f;
            canvas.DrawText($"{gridLine}m", rightOfGridLine, belowXAxis, SKTextAlign.Left, font, textPaint);
        }

        canvas.DrawLine(leftLimit * _pixelsPerMeter, 0, rightLimit * _pixelsPerMeter, 0, axisPaint);

        canvas.Restore();

    }


    private void DrawBox(SKCanvas canvas, float centerX, float centerY, double mass)
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

        using var textPaint = new SKPaint
        {
            Color = BoxTextColour,
            IsAntialias = true
        };

        using var font = new SKFont(SKTypeface.Default, 12f);

        var rect = new SKRoundRect(new SKRect(-_boxWidthPx / 2f, -_boxHeightPx / 2f, _boxWidthPx / 2f, _boxHeightPx / 2f), 6f, 6f);
        canvas.DrawRoundRect(rect, boxFill);
        canvas.DrawRoundRect(rect, boxStroke);

        canvas.DrawText($"{mass:0.0} kg", 0, 4, SKTextAlign.Center, font, textPaint);

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
