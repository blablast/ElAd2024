using Microsoft.Windows.ApplicationModel.Resources;

namespace ElAd2024.Helpers.General;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();
    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
}
