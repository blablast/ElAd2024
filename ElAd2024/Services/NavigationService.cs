using System.Diagnostics.CodeAnalysis;

using ElAd2024.Contracts.Services;
using ElAd2024.Contracts.ViewModels;
using ElAd2024.Helpers;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ElAd2024.Services;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public class NavigationService(IPageService pageService) : INavigationService
{
    private readonly IPageService pageService = pageService;
    private object? lastParameterUsed;
    private Frame? frame;

    public event NavigatedEventHandler? Navigated;

    public Frame? Frame
    {
        get
        {
            if (frame is null)
            {
                frame = App.MainWindow.Content as Frame;
                RegisterFrameEvents();
            }
            return frame;
        }
        set
        {
            UnregisterFrameEvents();
            frame = value;
            RegisterFrameEvents();
        }
    }

    [MemberNotNullWhen(true, nameof(Frame), nameof(frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    private void RegisterFrameEvents()
    {
        if (frame is not null)
        {
            frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (frame is not null)
        {
            frame.Navigated -= OnNavigated;
        }
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            var vmBeforeNavigation = frame.GetPageViewModel();
            frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
            return true;
        }
        return false;
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = pageService.GetPageType(pageKey);
        if (frame is not null && (frame.Content?.GetType() != pageType || (parameter is not null && !parameter.Equals(lastParameterUsed))))
        {
            frame.Tag = clearNavigation;
            var vmBeforeNavigation = frame.GetPageViewModel();
            if (frame.Navigate(pageType, parameter))
            {
                lastParameterUsed = parameter;
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
                return true;
            }
        }
        return false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }
}