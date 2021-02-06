﻿using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bangs
{
    public class BangResult : UrlQueryResult
    {
        public BangResult(string url) : base(url)
        {
            Title = "Launch !Bang";
        }
    }
}
