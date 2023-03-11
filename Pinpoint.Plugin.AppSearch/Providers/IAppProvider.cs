using System.Collections.Generic;

namespace Pinpoint.Plugin.AppSearch.Providers
{
    public interface IAppProvider
    {
        IEnumerable<IApp> Provide();
    }
}