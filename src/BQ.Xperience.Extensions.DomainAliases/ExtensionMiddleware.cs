using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BQ.Xperience.Extensions.DomainAliases.Models;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CMS.Helpers;
using CMS.Websites;


namespace BQ.Xperience.Extensions.DomainAliases;

public class ExtensionMiddleware
{
    private readonly RequestDelegate _next;

    public ExtensionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
        IInfoProvider<ChannelInfo> channelInfoProvider, 
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<WebsiteChannelDomainAliasInfo> websiteChannelDomainAliasInfoProvider,
        IProgressiveCache cache)
    {
        var hostLowered = context.Request.Host.ToString().ToLower();
        var cacheSettings = new CacheSettings(60, $"BQ.Xperience.Extensions.DomainAliases.ExtensionMiddleware_WebsiteChannel_{hostLowered}");

        var websiteChannel = await cache.LoadAsync(async cacheSettings =>
        {
            cacheSettings.Cached = false;

            var domainAlias = websiteChannelDomainAliasInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasDomain), hostLowered)
                .FirstOrDefault();

            if (domainAlias == null)
                return null;

            var channel = channelInfoProvider.Get()
                .WhereEquals(nameof(ChannelInfo.ChannelName), domainAlias.WebsiteChannelDomainAliasChannelId)
                .FirstOrDefault();

            if (channel == null)
                return null;

            var websiteChannel = websiteChannelInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channel.ChannelID)
                .FirstOrDefault();

            if (websiteChannel == null)
                return null;

            var dependencyCacheKeys = new HashSet<string>
            {
                $"{ChannelInfo.OBJECT_TYPE}|all".ToLower(),
                $"{WebsiteChannelInfo.OBJECT_TYPE}|all".ToLower(),
                $"{WebsiteChannelDomainAliasInfo.OBJECT_TYPE}|all".ToLower()
            };
            cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(dependencyCacheKeys);

            return websiteChannel;
        }, cacheSettings);

        if (websiteChannel != null)
            context.Request.Host = new HostString(websiteChannel.WebsiteChannelDomain);

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }
}
public static class ExtensionMiddlewareExtensions
{
    public static IApplicationBuilder UseDomainAliasesExtension(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExtensionMiddleware>();
    }
}