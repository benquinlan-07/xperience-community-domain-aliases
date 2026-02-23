using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XperienceCommunity.DomainAliases.Models;
using XperienceCommunity.DomainAliases.Providers;

namespace XperienceCommunity.DomainAliases;

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
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        IProgressiveCache cache)
    {
        var hostLowered = context.Request.Host.ToString().ToLower();
        var cacheSettings = new CacheSettings(60, $"XperienceCommunity.DomainAliases.ExtensionMiddleware_WebsiteChannel_{hostLowered}");

        // Attempt to get the website channel from the domain alias
        var channelData = await cache.LoadAsync(async cacheSettings =>
        {
            // Add the cache dependencies
            var cacheDependencies = cacheDependencyBuilderFactory.Create()
                .ForInfoObjects<ChannelInfo>().All()
                .Builder().ForInfoObjects<WebsiteChannelInfo>().All()
                .Builder().ForInfoObjects<WebsiteChannelDomainAliasInfo>().All()
                .Builder().Build();
            cacheSettings.CacheDependency = cacheDependencies;

            // Check to see if the domain matches an alias
            var domainAlias = websiteChannelDomainAliasInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasDomain), hostLowered)
                .FirstOrDefault();

            if (domainAlias == null)
                return (null, null);

            // Get the channel for the alias
            var channel = channelInfoProvider.Get()
                .WhereEquals(nameof(ChannelInfo.ChannelID), domainAlias.WebsiteChannelDomainAliasChannelId)
                .FirstOrDefault();

            if (channel == null)
                return (null, null);

            // Get the website for the channel
            var websiteChannel = websiteChannelInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channel.ChannelID)
                .FirstOrDefault();

            if (websiteChannel == null)
                return (null, null);

            return (WebsiteChannel: websiteChannel, Channel: channel);
        }, cacheSettings);

        // If domain alias exists, set the website channel descriptor in the HttpContext.Items for later retrieval in the pipeline
        if (channelData.WebsiteChannel != null && channelData.Channel != null)
        {
            var channelDescriptor = new WebsiteChannelDescriptor
            {
                WebsiteChannelID = channelData.WebsiteChannel.WebsiteChannelID,
                WebsiteChannelName = channelData.Channel.ChannelName
            };
            context.Items[ExtensionWebsiteChannelDomainProvider.ALIAS_WEBSITE_CHANNEL_CONTEXT_KEY] = channelDescriptor;
        }

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