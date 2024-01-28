using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ElAd2024.Helpers;
public static class ColorInterpolation
{
    public static SolidColorBrush GetBrushBasedOnHumidity(float ratio, Color startColor, Color middleColor, Color endColor)
    {
        Color interpolatedColor;
        if (ratio < 0.5)
        {
            // Interpolate between startColor and middleColor
            interpolatedColor = InterpolateColor(startColor, middleColor, ratio / 0.5);
        }
        else
        {
            // Interpolate between middleColor and endColor
            interpolatedColor = InterpolateColor(middleColor, endColor, (ratio - 0.5) / 0.5);
        }

        return new SolidColorBrush(interpolatedColor);
    }

    private static Color InterpolateColor(Color color1, Color color2, double fraction)
    {
        var r = Interpolate(color1.R, color2.R, fraction);
        var g = Interpolate(color1.G, color2.G, fraction);
        var b = Interpolate(color1.B, color2.B, fraction);
        return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
    }

    private static double Interpolate(double d1, double d2, double fraction) =>  d1 + (d2 - d1) * fraction;
}
