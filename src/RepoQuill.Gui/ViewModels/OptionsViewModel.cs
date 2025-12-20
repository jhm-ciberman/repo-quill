using CommunityToolkit.Mvvm.ComponentModel;

namespace RepoQuill.Gui.ViewModels;

/// <summary>
/// ViewModel for the options panel.
/// </summary>
public partial class OptionsViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _honorGitIgnore = true;

    [ObservableProperty]
    private bool _hideBinaries = true;

    [ObservableProperty]
    private bool _stripComments;

    [ObservableProperty]
    private bool _normalizeWhitespace;
}
