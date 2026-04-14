using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using MessagePipe;

using Hexblick.Presentations;

namespace Hexblick.Interactions;

internal sealed class ChooseFontMessage :
    InteractionMessage<object>
{
}

internal interface IChooseFontRequestHandler :
    IAsyncRequestHandler<ChooseFontMessage, object>,
    IRequiresXamlRoot;

internal sealed class ChooseFontRequestHandler :
    IChooseFontRequestHandler
{
    private Func<XamlRoot>? _xamlRootAccessor;

    /// <inheritdoc />
    public async ValueTask<object> InvokeAsync(
        ChooseFontMessage request,
        CancellationToken cancellationToken)
    {
        if (this._xamlRootAccessor is not { } accessor)
        {
            throw new InvalidOperationException();
        }

        using var content = new FontDialog();

        var dialog = new ContentDialog
        {
            Content = content,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
            PrimaryButtonText = "OK",
            CloseButtonText = "キャンセル",
            XamlRoot = accessor()
        };

        var dialogResult = await dialog.ShowAsync();

        return new();
    }

    /// <inheritdoc />
    void IRequiresXamlRoot.SetXamlRootAccessor(Func<XamlRoot> accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        this._xamlRootAccessor = accessor;
    }
}
