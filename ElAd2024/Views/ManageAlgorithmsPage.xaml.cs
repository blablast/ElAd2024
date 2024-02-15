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
        if ((ListView)sender != TargetListView)
        {
            HandleSourceListViewDragItemsStarting(e);
        }
        else
        {
            HandleTargetListViewDragItemsStarting(e);
        }
    }

    // Specific handling for starting drag from the source ListView
    private void HandleSourceListViewDragItemsStarting(DragItemsStartingEventArgs e)
    {
        Debug.WriteLine("HandleSourceListViewDragItemsStarting");
        var items = string.Join(",", e.Items.Cast<AlgorithmStepViewModel>().Select(item => item.AlgorithmStep!.Step.Id));
        e.Data.SetText(items);
        e.Data.RequestedOperation = DataPackageOperation.Copy;
        Debug.WriteLine($"e.Data.RequestedOperation: {e.Data.RequestedOperation}, item.AlgorithmStep.Step.Id: {items}");
    }

    // Specific handling for starting drag from the target ListView
    private void HandleTargetListViewDragItemsStarting(DragItemsStartingEventArgs e)
    {
        Debug.WriteLine("HandleTargetListViewDragItemsStarting");
        var items = e.Items.OfType<AlgorithmStepViewModel>();
        if (!items.Any(item => item.IsMoveable))
        {
            e.Cancel = true;
        }
        else
        {
            var itemsTxt = string.Join(",", e.Items.Cast<AlgorithmStepViewModel>().Select(item => item.AlgorithmStep!.Id)); // Assuming YourItemType has an Id property
            e.Data.SetText(itemsTxt);
            e.Data.RequestedOperation = DataPackageOperation.Move;
            Debug.WriteLine($"e.Data.RequestedOperation: {e.Data.RequestedOperation}, ");
        }
    }

    // Called when the drag operation is completed
    private void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        Debug.WriteLine("ListView_DragItemsCompleted");
        if (sender != TargetListView)
        {
            Debug.WriteLine("DragItemsCompleted from Source ListView");
        }
        else
        {
            Debug.WriteLine("DragItemsCompleted from Target ListView");
            ViewModel.Reorder();
        }
    }

    // Handles the drag over event for both ListViews
    private void ListView_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
       
        if ((ListView)sender == TargetListView)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            Debug.WriteLine($"ListView_DragOver, e.AcceptedOperation: {e.AcceptedOperation}");
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.Move;

            Debug.WriteLine($"ListView_DragOver, e.AcceptedOperation: {e.AcceptedOperation}");
        }
    }
    // Handles the drop event for both ListViews
    private async void ListView_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.Text))
        {
            var idText = await e.DataView.GetTextAsync();
            var ids = idText.Split(',').Select(int.Parse); // Assuming IDs are int

            if ((ListView)sender != TargetListView)
            {
                // Logic to remove items from ViewModel.SelectedAlgorithmSteps based on dropped IDs
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
                    ViewModel.AddAvailableStep(item);
                }
            }
        }
    }
}
