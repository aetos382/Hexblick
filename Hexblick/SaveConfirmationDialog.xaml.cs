// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System.Collections.Generic;

using Microsoft.UI.Xaml;

namespace Hexblick;

public sealed partial class SaveConfirmationDialog
{
    public SaveConfirmationDialog()
    {
        this.InitializeComponent();
    }

    public IReadOnlyList<string> Items
    {
        get => (IReadOnlyList<string>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items),
        typeof(IReadOnlyList<string>),
        typeof(SaveConfirmationDialog),
        new PropertyMetadata(null));
}
