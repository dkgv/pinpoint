using System.Collections.Generic;

namespace Pinpoint.Plugin.AppSearch
{
    public interface IAppProvider
    {
        IEnumerable<IApp> Provide();
    }
}