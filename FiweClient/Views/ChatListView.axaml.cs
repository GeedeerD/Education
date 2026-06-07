using FiweClient.ViewModels.Chats;
using Avalonia.Controls;


namespace FiweClient.Views;

public partial class ChatListView : UserControl
{
    public ChatListView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ChatListViewModel vm)
        {
            vm.LoadChatsCommand.Execute(null);
        }
    }
}
