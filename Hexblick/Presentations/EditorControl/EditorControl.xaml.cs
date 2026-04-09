using Microsoft.UI.Xaml;

namespace Hexblick.Presentations;

internal sealed partial class EditorControl
{
    public EditorControl()
    {
        this.InitializeComponent();
    }

    public EditorControlViewModel ViewModel
    {
        get => (EditorControlViewModel)this.GetValue(ViewModelProperty);
        set => this.SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(EditorControlViewModel),
            typeof(EditorControl),
            new PropertyMetadata(null));
}
