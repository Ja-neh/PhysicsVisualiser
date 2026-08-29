using System;
using SkiaSharp;
using PhysicsVisualiser.ViewModels;
using PhysicsEngine.Scenarios;

namespace PhysicsVisualiser.Renderers;

public class FlatSurfaceRenderer
{
    // Colors
    private static readonly SKColor BgColor = new SKColor(255, 255, 255);
    private static readonly SKColor GridColor = new SKColor(51, 65, 85, 90);
    private static readonly SKColor AxisColor = new SKColor(100, 116, 139, 140);

    private static readonly SKColor BoxFillColor = new SKColor(79, 70, 229);
    private static readonly SKColor BoxStrokeColor = new SKColor(165, 180, 252);
    private static readonly SKColor BoxTextColor = SKColors.White;

    // View configuration
    private const float _pixelsPerMeter = 100f;

    private const float _gridOriginAdjustFactorX = 0.25f;
    private const float _gridOriginAdjustFactorY = 0.75f;

    private float _cameraPosition = 0f;
    private float _cameraPx = 0f;
    private const float _lerpFactor = 0.1f;

#if ANDROID
    private const float _boxWidthPx = 45f;
    private const float _boxHeightPx = 30f;
#else
    private const float _boxWidthPx = 60f;
    private const float _boxHeightPx = 45f;
#endif


    public void Render(SKCanvas canvas, SKImageInfo info, FlatSurfaceState state)
    {
        canvas.Clear(BgColor);

        float widthPx = info.Width;
        float heightPx = info.Height;

        float xAnchorPx = widthPx * _gridOriginAdjustFactorX;
        float yAnchorPx = heightPx * _gridOriginAdjustFactorY;

        // Camera: lerp toward the box
        float boxPosition = (float)state.Position;

        _cameraPosition += (boxPosition - _cameraPosition) * _lerpFactor;
        _cameraPx = _cameraPosition * _pixelsPerMeter;

        // Apply camera transform
        canvas.Save();
        canvas.Translate(xAnchorPx - _cameraPosition * _pixelsPerMeter, 0); 

        DrawGrid(canvas, widthPx, xAnchorPx, yAnchorPx);

        float boxPx = boxPosition * _pixelsPerMeter;
        float boxCenterYPx = yAnchorPx - (_boxHeightPx / 2f);
        DrawBox(canvas, boxPx, boxCenterYPx, state.Mass);

        canvas.Restore();
    }

    public void ResetCamera()
    {
        _cameraPosition = 0f;
    }

    private void DrawGrid(SKCanvas canvas, float viewWidth, float anchorX, float groundY)
    {
        using var gridPaint = new SKPaint
        {
            Color = GridColor,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var axisPaint = new SKPaint
        {
            Color = AxisColor,
            StrokeWidth = 1.5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = new SKColor(148, 163, 184, 180),
            IsAntialias = true
        };

        using var font = new SKFont(SKTypeface.Default, 10f);

        const float gridSpacing = 5f;
        float leftWorldM = _cameraPosition - anchorX / _pixelsPerMeter;
        float rightWorldM = _cameraPosition + (viewWidth - anchorX) / _pixelsPerMeter;

        float startM = MathF.Floor(leftWorldM / gridSpacing) * gridSpacing;
        float endM = MathF.Ceiling(rightWorldM / gridSpacing) * gridSpacing;

        for (float meters = startM; meters <= endM; meters += gridSpacing)
        {
            float x = meters * _pixelsPerMeter;
            canvas.DrawLine(x, 0, x, 2 * groundY, gridPaint);

            canvas.DrawText($"{meters:0}m", x + 4, groundY + 20, SKTextAlign.Left, font, textPaint);
        }

        // Ground line
        float groundLeft = (startM - 50) * _pixelsPerMeter;
        float groundRight = (endM + 50) * _pixelsPerMeter;
        canvas.DrawLine(groundLeft, groundY, groundRight, groundY, axisPaint);
    }


    private void DrawBox(SKCanvas canvas, float centerX, float centerY, double mass)
    {
        canvas.Save();
        canvas.Translate(centerX, centerY);

        using var boxFill = new SKPaint
        {
            Color = BoxFillColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var boxStroke = new SKPaint
        {
            Color = BoxStrokeColor,
            StrokeWidth = 2.5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = BoxTextColor,
            IsAntialias = true
        };

        using var font = new SKFont(SKTypeface.Default, 12f);

        var rect = new SKRoundRect(new SKRect(-_boxWidthPx / 2f, -_boxHeightPx / 2f, _boxWidthPx / 2f, _boxHeightPx / 2f), 6f, 6f);
        canvas.DrawRoundRect(rect, boxFill);
        canvas.DrawRoundRect(rect, boxStroke);

        canvas.DrawText($"{mass:0.0} kg", 0, 4, SKTextAlign.Center, font, textPaint);

        canvas.Restore();
    }

}
