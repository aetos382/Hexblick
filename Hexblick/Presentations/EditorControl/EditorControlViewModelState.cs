using Microsoft.UI.Xaml.Controls;

namespace Hexblick.Presentations;

internal abstract class EditorControlViewModelState
{
    public IconSource Icon { get; set; }

    public string Title { get; set; }
}

internal sealed class EditorControlViewModelNewFileState :
    EditorControlViewModelState
{
}

internal sealed class EditorControlViewModelExistingFileState :
    EditorControlViewModelState
{
}
