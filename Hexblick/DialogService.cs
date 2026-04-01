using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Controls;

using Hexblick.Localization;

namespace Hexblick;

internal enum SaveConfirmationesult
{
    Save,
    Discard,
    Cancel
}

internal interface IDialogService
{
    Task<SaveConfirmationesult> ShowSaveConfirmationDialogAsync(string[] titles);
}

internal class DialogService :
    IDialogService
{
    private readonly IStringLoader _stringLoader;
    private readonly IXamlRootProvider _xamlRootProvider;

    public DialogService(
        IStringLoader stringLoader,
        IXamlRootProvider xamlRootProvider)
    {
        ArgumentNullException.ThrowIfNull(stringLoader);
        ArgumentNullException.ThrowIfNull(xamlRootProvider);

        this._stringLoader = stringLoader;
        this._xamlRootProvider = xamlRootProvider;
    }

    /// <inheritdoc />
    public async Task<SaveConfirmationesult> ShowSaveConfirmationDialogAsync(string[] titles)
    {
        var stringLoader = this._stringLoader;

        var dialog = new ContentDialog
        {
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            PrimaryButtonText = stringLoader.GetString("SaveConfirmationDialog.SaveButton.Text"),
            SecondaryButtonText = stringLoader.GetString("SaveConfirmationDialog.DiscardButton.Text"),
            CloseButtonText = stringLoader.GetString("SaveConfirmationDialog.CancelButton.Text"),
            DefaultButton = ContentDialogButton.Close,
            Title = "Hexblick",
            Content = new SaveConfirmationDialog
            {
                Items = titles
            },
            XamlRoot = this._xamlRootProvider.XamlRoot
        };

        var result = await dialog.ShowAsync();

        return result switch
        {
            ContentDialogResult.None => SaveConfirmationesult.Cancel,
            ContentDialogResult.Primary => SaveConfirmationesult.Save,
            ContentDialogResult.Secondary => SaveConfirmationesult.Discard
        };
    }
}
