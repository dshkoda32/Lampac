﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Lampac.Engine;
using Lampac.Engine.CORE;
using Microsoft.Extensions.Caching.Memory;
using System;
using Shared.Engine.SISI;
using System.Linq;

namespace Lampac.Controllers.Xnxx
{
    public class ViewController : BaseController
    {
        [HttpGet]
        [Route("xnx/vidosik")]
        async public Task<ActionResult> Index(string uri)
        {
            if (!AppInit.conf.Xnxx.enable)
                return OnError("disable");

            string memKey = $"xnxx:view:{uri}";
            if (!memoryCache.TryGetValue(memKey, out Dictionary<string, string> stream_links))
            {
                stream_links = await XnxxTo.StreamLinks(AppInit.conf.Xnxx.host, uri, url => HttpClient.Get(url, timeoutSeconds: 10, useproxy: AppInit.conf.Xnxx.useproxy));

                if (stream_links == null || stream_links.Count == 0)
                    return OnError("stream_links");

                memoryCache.Set(memKey, stream_links, DateTime.Now.AddMinutes(AppInit.conf.multiaccess ? 20 : 5));
            }

            return Json(stream_links.ToDictionary(k => k.Key, v => HostStreamProxy(AppInit.conf.Xnxx.streamproxy, v.Value)));
        }
    }
}