using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsVisualiser.Renderers;

internal static class VectorScaler
{
    private const float _maxDisplayLength = 200f;
    private const float _minVisibleLength = 20f;
    private const float _linearTreshold = 5f;
    private const float _linearMultiplier = 15f;
    private const float _logMultiplier = 30f;

    // scale num
    public static float Scale(float value)
    {
        if (Math.Abs(value) < 0.001f) return 0f;

        int sign = Math.Sign(value);
        float absValue = Math.Abs(value);
        float scaled;

        if (absValue <= _linearTreshold)
        {
            // Linear for small values
            scaled = absValue * _linearMultiplier;
        }
        else
        {
            // Logarithmic for large values
            scaled = _linearTreshold * _linearMultiplier + (float)Math.Log(absValue - _linearTreshold + 1) * _logMultiplier;
        }

        scaled = Math.Min(scaled, _maxDisplayLength);

        if (scaled < _minVisibleLength && absValue > 0.001f)
        {
            scaled = _minVisibleLength;
        }

        return sign * scaled;
    }

    // Scale a vector
    public static (float scaledX, float scaledY) ScaleVector(float endX, float endY)
    {
        float magnitude = MathF.Sqrt(endX * endX + endY * endY);

        if (magnitude < 0.001f) return (0f, 0f);

        float scaledMagnitude = Scale(magnitude);
        float ratio = scaledMagnitude / magnitude;

        return (endX * ratio, endY * ratio);
    }
}
