using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pinpoint.Core
{
    public static class LINQExtensions
    {
        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
        {
            return await Task.WhenAll(source.Select(async s => await method(s)));
        }
    }
}
