using Hexblick.ViewModels;

using Microsoft.UI.Xaml;

namespace Hexblick;

internal sealed partial class EditorControl :
    IDisposable
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
            new PropertyMetadata(null, static (d, p) =>
            {
                ArgumentNullException.ThrowIfNull(p.NewValue, nameof(ViewModel));
            }));

    /// <inheritdoc />
    public void Dispose()
    {
        this.ViewModel.Dispose();
    }
}
