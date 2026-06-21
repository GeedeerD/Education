using Avalonia.Controls;
using FiweClient.ViewModels.Chats;

namespace FiweClient.Views;

public partial class ChatView : UserControl
{
    public ChatView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ChatViewModel vm)
        {
            vm.LoadMessagesCommand.Execute(null);
        }
    }
}
