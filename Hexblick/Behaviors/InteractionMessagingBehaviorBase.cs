using CommunityToolkit.WinUI.Behaviors;

using Hexblick.Interactions;
using Hexblick.Windowing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Hexblick.Behaviors;

internal abstract class InteractionMessagingBehaviorBase<T, THandler> :
    BehaviorBase<T>
    where T : FrameworkElement
{
    public static readonly DependencyProperty MessengerProperty = DependencyProperty.Register(
        nameof(MessageHandler),
        typeof(THandler),
        typeof(InteractionMessagingBehaviorBase<T, THandler>),
        new PropertyMetadata(null, OnMessageHandlerChanged));

    public THandler MessageHandler
    {
        get => (THandler)this.GetValue(MessengerProperty);
        set => this.SetValue(MessengerProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAssociatedObjectLoaded()
    {
        SetOwner(this.AssociatedObject, this.MessageHandler);
    }

    private static void SetOwner(
        T associatedObject,
        THandler handler)
    {
        if (handler is null)
        {
            return;
        }

        if (handler is IRequiresWindowId rwi)
        {
            var windowManager = Application.Current.Services.GetRequiredService<IWindowManager>();
            if (windowManager.TryGetWindowForElement(associatedObject, out var window))
            {
                rwi.SetWindowIdAccessor(() => window.AppWindow.Id);
            }
        }
        else if (handler is IRequiresXamlRoot rxr)
        {
            if (associatedObject.XamlRoot is { } xamlRoot)
            {
                rxr.SetXamlRootAccessor(() => xamlRoot);
            }
        }
    }

    private static void OnMessageHandlerChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs args)
    {
        if (d is not InteractionMessagingBehaviorBase<T, THandler> b ||
            args.Property != MessengerProperty ||
            args.NewValue is not THandler handler)
        {
            return;
        }

        if (b.AssociatedObject is null)
        {
            return;
        }

        SetOwner(b.AssociatedObject, handler);
    }
}
