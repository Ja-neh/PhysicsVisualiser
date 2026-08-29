using System;
using SkiaSharp;
using PhysicsVisualiser.ViewModels;
using PhysicsEngine.Scenarios;

namespace PhysicsVisualiser.Renderers;

public class FlatSurfaceRenderer
{

    // FIELDS
    #region COLOURS
    private static readonly SKColor BgColor = SKColors.White;
    private static readonly SKColor GridColor = new SKColor(51, 65, 85, 90);
    private static readonly SKColor AxisColor = new SKColor(100, 116, 139, 140);

    private static readonly SKColor BoxFillColor = new SKColor(79, 70, 229);
    private static readonly SKColor BoxStrokeColor = new SKColor(165, 180, 252);
    private static readonly SKColor BoxTextColor = SKColors.White;

    private static readonly SKColor ForceAppliedColor = new SKColor(239, 68, 68);
    private static readonly SKColor ForceNormalColor = new SKColor(14, 165, 233);
    private static readonly SKColor ForceWeightColor = new SKColor(234, 179, 8);
    private static readonly SKColor ForceFrictionColor = new SKColor(168, 85, 247);
    private static readonly SKColor VelocityColor = new SKColor(16, 185, 129);
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
        canvas.Clear(BgColor);

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
            Color = GridColor,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var axisPaint = new SKPaint
        {
            Color = AxisColor,
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
