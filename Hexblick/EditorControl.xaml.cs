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
        get
        {
            return (EditorControlViewModel)this.GetValue(ViewModelProperty);
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);

            this.SetValue(ViewModelProperty, value);
        }
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(EditorControlViewModel),
            typeof(EditorControl),
            new PropertyMetadata(null));

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
