using System.Collections.ObjectModel;
using System.Diagnostics;
using ElAd2024.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace ElAd2024.Views;

public sealed partial class ManageAlgorithmsPage : Page
{
    public ManageAlgorithmsViewModel ViewModel
    {
        get;
    }

    public ManageAlgorithmsPage()
    {
        ViewModel = App.GetService<ManageAlgorithmsViewModel>();
        InitializeComponent();
    }


    private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        var items = e.Items.OfType<AlgorithmStepViewModel>();
        if ((ListView)sender != TargetListView)
        {
            var itemsTxt = string.Join(",", items.Select(item => item.AlgorithmStep!.Step.Id));
            e.Data.SetText(itemsTxt);
            e.Data.RequestedOperation = DataPackageOperation.Copy;
        }
        else if (!items.Any(item => item.IsMoveable))
        {
            e.Cancel = true;
        }
        else
        {
            var itemsTxt = string.Join(",", items.Select(item => item.AlgorithmStep!.Id));
            e.Data.SetText(itemsTxt);
            e.Data.RequestedOperation = DataPackageOperation.Move;
        }

    }

    private async void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        if (sender == TargetListView)
        {
            ViewModel.Reorder();
        }
        await ViewModel.Save(null);
    }

    private void ListView_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        => e.AcceptedOperation = (ListView)sender == TargetListView ? DataPackageOperation.Copy : DataPackageOperation.Move;

    private async void ListView_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.Text))
        {
            var listView = (ListView)sender;
            var dropIndex = CalculateDropIndexForMultiRow(listView, e.GetPosition(listView));

            var idText = await e.DataView.GetTextAsync();
            var ids = idText.Split(',').Select(int.Parse);

            if ((ListView)sender != TargetListView)
            {
                var itemsToRemove = ViewModel.SelectedAlgorithmSteps.Where(item => ids.Contains(item.Id)).ToList();
                foreach (var item in itemsToRemove)
                {
                    ViewModel.SelectedAlgorithmSteps.Remove(item);
                }
            }
            else
            {
                foreach (var item in ids)
                {
                    ViewModel.AddAvailableStep(item, dropIndex++);
                }
            }
        }
    }
    private static int CalculateDropIndexForMultiRow(ListView listView, Point dropPosition)
    {
        var columns = CalculateColumns(listView);
        var itemWidth = listView.ActualWidth / columns;
        var itemHeight = DetermineItemHeight(listView);
        var row = (int)(dropPosition.Y / itemHeight);
        var column = (int)(dropPosition.X / itemWidth);
        var index = row * columns + column;
        return Math.Min(index, listView.Items.Count);
    }

    private static int CalculateColumns(ListView listView)
    {
        var sampleItemContainer = (ListViewItem)listView.ContainerFromIndex(0);
        if (sampleItemContainer is not null)
        {
            var itemWidth = sampleItemContainer.ActualWidth + sampleItemContainer.Margin.Left + sampleItemContainer.Margin.Right;
            if (itemWidth > 0)
            {
                return Math.Max((int)(listView.ActualWidth / itemWidth), 1); // Ensure at least one column
            }
        }
        return 1; // A default column count if unable to determine dynamically.
    }

    private static double DetermineItemHeight(ListView listView)
    {
        var sampleItemContainer = (ListViewItem)listView.ContainerFromIndex(0);
        if (sampleItemContainer is not null)
        {
            return sampleItemContainer.ActualHeight + sampleItemContainer.Margin.Top + sampleItemContainer.Margin.Bottom;
        }
        return 150; // A default item height if unable to determine dynamically.
    }


}
