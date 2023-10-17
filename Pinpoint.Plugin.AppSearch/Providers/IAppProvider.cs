using System.Collections.Generic;
using System.Threading.Tasks;
using Pinpoint.Plugin.AppSearch.Models;

namespace Pinpoint.Plugin.AppSearch.Providers;

public interface IAppProvider
{
    Task<IEnumerable<IApp>> Provide();
}