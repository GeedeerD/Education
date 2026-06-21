using Avalonia.Controls;
using FiweClient.ViewModels.Settings;

namespace FiweClient.Views;

public partial class QrGeneratorView : UserControl
{
    public QrGeneratorView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is QrGeneratorViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
