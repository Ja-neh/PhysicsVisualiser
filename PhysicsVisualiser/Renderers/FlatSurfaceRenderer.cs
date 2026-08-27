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
    private const float _pixelsPerMeter = 40f;

    private const float originXAdjustFactor = 0.25f;
    private const float originYAdjustFactor = 0.75f;

#if ANDROID
    private const float boxWidthPx = 45f;
    private const float boxHeightPx = 30f;
#else
    private const float boxWidthPx = 60f;
    private const float boxHeightPx = 45f;
#endif


    public void Render(SKCanvas canvas, SKImageInfo info, FlatSurfaceState state)
    {
        // setup
        canvas.Clear(BgColor);

        float width = info.Width;
        float height = info.Height;

        float originX = width * originXAdjustFactor;
        float originY = height * originYAdjustFactor;

        float boxPosMeters = (float)state.Position;

        // Grid
        DrawGrid(canvas, width, height, originX, originY);

        // Box
        float horizontalOffsetPx = boxPosMeters * _pixelsPerMeter;

        float surfaceContactX = originX + horizontalOffsetPx;
        float surfaceContactY = originY;

        float boxCenterX = surfaceContactX;
        float boxCenterY = surfaceContactY - (boxHeightPx / 2f);

        DrawBox(canvas, boxCenterX, boxCenterY, boxWidthPx, boxHeightPx, state.Mass);

        canvas.Restore();
    }

    private void DrawGrid(SKCanvas canvas, float width, float height, float originX, float originY)
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

        // Vertical grid lines
        for (float meters = -50; meters <= 50; meters += 5)
        {
            float x = originX + meters * _pixelsPerMeter;
            canvas.DrawLine(x, - 2 * height, x, 2 * height, gridPaint);

            if(meters % 10 == 0)
            {
                canvas.DrawText($"{meters:0}m", x + 4, originY + 20, SKTextAlign.Left, font, textPaint);
            }
        }

        // Ground
        canvas.DrawLine(- 2 * width, originY, 2 * width, originY, axisPaint);
    }


    private void DrawBox(SKCanvas canvas, float centerX, float centerY, float bWidth, float bHeight, double mass)
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

        var rect = new SKRoundRect(new SKRect(-bWidth / 2f, -bHeight / 2f, bWidth / 2f, bHeight / 2f), 6f, 6f);
        canvas.DrawRoundRect(rect, boxFill);
        canvas.DrawRoundRect(rect, boxStroke);

        canvas.DrawText($"{mass:0.0} kg", 0, 4, SKTextAlign.Center, font, textPaint);

        canvas.Restore();
    }

}
