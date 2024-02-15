using System.Diagnostics.CodeAnalysis;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace ElAd2024.Services.General;

public class NavigationViewService(INavigationService navigationService, IPageService pageService) : INavigationViewService
{
    private readonly INavigationService navigationService = navigationService;

    private readonly IPageService pageService = pageService;

    private NavigationView? navigationView;

    public IList<object>? MenuItems => navigationView?.MenuItems;

    public object? SettingsItem => navigationView?.SettingsItem;

    [MemberNotNull(nameof(navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        this.navigationView = navigationView;
        this.navigationView.BackRequested += OnBackRequested;
        this.navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
        if (navigationView is not null)
        {
            navigationView.BackRequested -= OnBackRequested;
            navigationView.ItemInvoked -= OnItemInvoked;
        }
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
        => navigationView is null ? null : GetSelectedItem(navigationView.MenuItems, pageType) ?? GetSelectedItem(navigationView.FooterMenuItems, pageType);

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        => navigationService.GoBack();

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;
            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                navigationService.NavigateTo(pageKey);
            }
        }
    }

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (var item in menuItems.OfType<NavigationViewItem>())
        {
            if (IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            var selectedChild = GetSelectedItem(item.MenuItems, pageType);
            if (selectedChild is not null)
            {
                return selectedChild;
            }
        }
        return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
        => menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey && pageService.GetPageType(pageKey) == sourcePageType;
}
