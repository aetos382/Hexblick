using System.Collections.Generic;

using Microsoft.UI.Xaml;

namespace Hexblick.Presentations;

internal sealed partial class SaveConfirmationDialog
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
