using System;

using Microsoft.UI.Xaml;

namespace Hexblick.Extensions;

internal static class ApplicationExtensions
{
    extension(Application app)
    {
        public IServiceProvider Services
        {
            get
            {
                if (app is not IServiceProvider sp)
                {
                    throw new InvalidOperationException();
                }

                return sp;
            }
        }
    }
}
