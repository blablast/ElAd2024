using System.Windows.Input;
using Microsoft.UI.Xaml;
using Telerik.UI.Xaml.Controls.Grid.Commands;

namespace ElAd2024.Telerik;

public class CustomTelerikDataGridCommand : DataGridCommand
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(CustomTelerikDataGridCommand), new PropertyMetadata(null));

    public ICommand Command
    {
        get =>  (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public override bool CanExecute(object parameter)
        => (Owner?.CommandService?.CanExecuteDefaultCommand(Id, parameter) ?? false) && (Command?.CanExecute(parameter) ?? true);

    public override void Execute(object parameter)
    {
        Owner?.CommandService?.ExecuteDefaultCommand(Id, parameter);
        Command?.Execute(parameter);
    }
}
