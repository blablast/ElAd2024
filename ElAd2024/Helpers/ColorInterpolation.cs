using Windows.UI;
using Microsoft.UI.Xaml.Media;

namespace ElAd2024.Helpers;

public static class ColorInterpolation
{
    public static SolidColorBrush GetBrushBasedOnHumidity(float ratio, Color startColor, Color middleColor,
        Color endColor)
        => new(InterpolateColor(ratio < 0.5 ? startColor : middleColor, ratio < 0.5 ? middleColor : endColor,
            (ratio < 0.5 ? ratio : ratio - 0.5) / 0.5));

    private static Color InterpolateColor(Color color1, Color color2, double fraction)
        => Color.FromArgb(255, Interpolate(color1.R, color2.R, fraction), Interpolate(color1.G, color2.G, fraction),
            Interpolate(color1.B, color2.B, fraction));

    private static byte Interpolate(double d1, double d2, double fraction)
        => (byte)(d1 + (d2 - d1) * fraction);
}