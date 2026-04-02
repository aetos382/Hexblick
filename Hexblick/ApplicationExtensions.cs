using System;

using Microsoft.UI.Xaml;

namespace Hexblick;

internal static class ApplicationExtensions
{
    extension(Application application)
    {
        public IServiceProvider Services
        {
            get
            {
                if (application is not IServiceProvider sp)
                {
                    throw new NotSupportedException();
                }

                return sp;
            }
        }
    }
}
