﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core.Results;

namespace Pinpoint.Core
{
    public class EmptyPlugin : IPlugin
    {
        public PluginManifest Manifest { get; set; }

        public PluginStorage Storage { get; set; }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
