using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using MessagePipe;

using Hexblick.Localization;
using Hexblick.Presentations;

namespace Hexblick.Interactions;

internal sealed class ConfirmSaveMessage
{
    private readonly string[] _titles;
    private readonly TaskCompletionSource<SaveConfirmationResult> _result = new();

    public IReadOnlyCollection<string> Titles => this._titles;

    public ConfirmSaveMessage(
        IReadOnlyCollection<string> titles)
    {
        ArgumentNullException.ThrowIfNull(titles);

        this._titles = titles.ToArray();
    }

    public void SetResult(SaveConfirmationResult result)
    {
        this._result.SetResult(result);
    }

    public Task<SaveConfirmationResult> GetResultAsync(CancellationToken cancellationToken)
    {
        return this._result.Task.WaitAsync(cancellationToken);
    }
}

internal interface IConfirmSaveRequestHandler :
    IAsyncRequestHandler<ConfirmSaveMessage, SaveConfirmationResult>,
    IRequiresXamlRoot;

internal sealed class ConfirmSaveRequestHandler :
    IConfirmSaveRequestHandler
{
    private readonly IStringLoader _stringLoader;
    private readonly ServiceScopeMarker _scopeMarker;

    public ConfirmSaveRequestHandler(
        IStringLoader stringLoader,
        ServiceScopeMarker scopeMarker)
    {
        ArgumentNullException.ThrowIfNull(stringLoader);
        ArgumentNullException.ThrowIfNull(scopeMarker);

        this._stringLoader = stringLoader;
        this._scopeMarker = scopeMarker;
    }

    private XamlRoot? _xamlRoot;

    void IRequiresXamlRoot.SetXamlRoot(XamlRoot xamlRoot)
    {
        ArgumentNullException.ThrowIfNull(xamlRoot);

        this._xamlRoot = xamlRoot;
    }

    public async ValueTask<SaveConfirmationResult> InvokeAsync(
        ConfirmSaveMessage request,
        CancellationToken cancellationToken)
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
                Items = request.Titles.ToArray()
            },
            XamlRoot = this._xamlRoot
        };

        var dialogResult = await dialog.ShowAsync();

        var confirmationResult = dialogResult switch
        {
            ContentDialogResult.None => SaveConfirmationResult.Cancel,
            ContentDialogResult.Primary => SaveConfirmationResult.Save,
            ContentDialogResult.Secondary => SaveConfirmationResult.Discard
        };

        request.SetResult(confirmationResult);

        return confirmationResult;
    }
}
