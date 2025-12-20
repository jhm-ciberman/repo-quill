using Avalonia.Controls;
using RepoQuill.Gui.ViewModels;

namespace RepoQuill.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    protected override async void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);

        if (this.DataContext is MainWindowViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
