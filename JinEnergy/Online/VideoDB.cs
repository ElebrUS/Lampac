﻿using JinEnergy.Engine;
using Microsoft.JSInterop;
using Shared.Engine.Online;
using Shared.Model.Online;

namespace JinEnergy.Online
{
    public class VideoDBController : BaseController
    {
        static List<HeadersModel> baseheader = HeadersModel.Init(
            ("cache-control", "no-cache"),
            ("dnt", "1"),
            ("origin", "https://kinoplay2.site"),
            ("pragma", "no-cache"),
            ("priority", "u=1, i"),
            ("referer", "https://kinoplay2.site/"),
            ("sec-ch-ua", "\"Google Chrome\";v=\"129\", \"Not = A ? Brand\";v=\"8\", \"Chromium\";v=\"129\""),
            ("sec-ch-ua-mobile", "?0"),
            ("sec-ch-ua-platform", "\"Windows\""),
            ("sec-fetch-dest", "empty"),
            ("sec-fetch-mode", "cors"),
            ("sec-fetch-site", "cross-site")
        );

        [JSInvokable("lite/videodb")]
        async public static ValueTask<string> Index(string args)
        {
            var init = AppInit.VideoDB.Clone();
            bool userapn = IsApnIncluded(init);

            var arg = defaultArgs(args);
            int s = int.Parse(parse_arg("s", args) ?? "-1");
            int sid = int.Parse(parse_arg("sid", args) ?? "-1");
            string? t = parse_arg("t", args);

            var oninvk = new VideoDBInvoke
            (
               null,
               init.corsHost(),
               (url, head) => JsHttpClient.Get(init.cors(url), httpHeaders(args, init, baseheader)),
               streamfile => userapn ? HostStreamProxy(init, streamfile) : DefaultStreamProxy(streamfile),
               AppInit.log
            );

            string memkey = $"videodb:view:{arg.kinopoisk_id}";
            refresh: var content = await InvokeCache(arg.id, memkey, () => oninvk.Embed(arg.kinopoisk_id));

            string html = oninvk.Html(content, arg.account_email, arg.kinopoisk_id, arg.title, arg.original_title, t, s, sid, false);
            if (string.IsNullOrEmpty(html))
            {
                IMemoryCache.Remove(memkey);
                if (IsRefresh(init, NotUseDefaultApn: true))
                    goto refresh;
            }

            return html;
        }

        [JSInvokable("lite/videodb/manifest.m3u8")]
        async public static ValueTask<string> Manifest(string args)
        {
            var init = AppInit.VideoDB;
            string? link = parse_arg("link", args);
            if (link == null)
                return string.Empty;

            return await JsHttpClient.Get(init.cors(link), httpHeaders(args, init, baseheader)) ?? string.Empty;
        }
    }
}
