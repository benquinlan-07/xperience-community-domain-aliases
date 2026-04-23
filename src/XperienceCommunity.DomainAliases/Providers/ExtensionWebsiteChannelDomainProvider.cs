using CMS.Websites.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XperienceCommunity.DomainAliases.Models;

namespace XperienceCommunity.DomainAliases.Providers;

internal sealed class ExtensionWebsiteChannelDomainProvider : IWebsiteChannelDomainProvider
{
    internal const string ALIAS_WEBSITE_CHANNEL_CONTEXT_KEY = "DomainAlias.WebsiteChannelDescriptor";

    private readonly IWebsiteChannelDomainProvider _defaultWebsiteChannelDomainProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExtensionWebsiteChannelDomainProvider([FromKeyedServices("kentico")] IWebsiteChannelDomainProvider defaultWebsiteChannelDomainProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _defaultWebsiteChannelDomainProvider = defaultWebsiteChannelDomainProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    private WebsiteChannelAliasDescriptor GetAliasDescriptor()
    {
        // Middleware previously set the website channel descriptor in the HttpContext.Items if a domain alias matched, so attempt to retrieve it here
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items == null)
            return null;

        if (items.TryGetValue(ALIAS_WEBSITE_CHANNEL_CONTEXT_KEY, out var websiteDescriptorValue) && websiteDescriptorValue is WebsiteChannelAliasDescriptor value)
            return value;

        return null;
    }

    public async Task<WebsiteChannelDescriptor> GetChannel(string domainName, CancellationToken cancellationToken)
    {
        // Try get default value
        var defaultChannel = await _defaultWebsiteChannelDomainProvider.GetChannel(domainName, cancellationToken);
        if (defaultChannel != null)
            return defaultChannel;

        // Fall back to alias descriptor
        return GetAliasDescriptor();
    }

    public async Task<ICollection<Uri>> GetAllDomains(int websiteChannelId, CancellationToken cancellationToken)
    {
        // Use default implementation
        var allDomains = await _defaultWebsiteChannelDomainProvider.GetAllDomains(websiteChannelId, cancellationToken);

        // Add alias domain if used for absolute urls
        var aliasDescriptor = GetAliasDescriptor();
        if (aliasDescriptor != null && websiteChannelId == aliasDescriptor.WebsiteChannelID && aliasDescriptor.AliasDomainUseForAbsoluteUrl)
        {
            var aliasDomainLower = aliasDescriptor.AliasDomain.ToLower();
            if (aliasDomainLower.StartsWith("http://") || aliasDomainLower.StartsWith("https://"))
                allDomains.Add(new Uri(aliasDomainLower));
            else 
                allDomains.Add(new Uri($"https://{aliasDomainLower}"));
        }

        return allDomains;
    }

    public async Task<string> GetDomain(int websiteChannelId, CancellationToken cancellationToken)
    {
        var aliasDescriptor = GetAliasDescriptor();
        if (aliasDescriptor != null && websiteChannelId == aliasDescriptor.WebsiteChannelID && aliasDescriptor.AliasDomainUseForAbsoluteUrl)
            return aliasDescriptor.AliasDomain;

        // Use default implementation
        var defaultDomain = await _defaultWebsiteChannelDomainProvider.GetDomain(websiteChannelId, cancellationToken);
        return defaultDomain;
    }

    public async Task<string> GetDomain(Guid websiteChannelGuid, CancellationToken cancellationToken)
    {
        var aliasDescriptor = GetAliasDescriptor();
        if (aliasDescriptor != null && websiteChannelGuid == aliasDescriptor.WebsiteChannelGUID && aliasDescriptor.AliasDomainUseForAbsoluteUrl)
            return aliasDescriptor.AliasDomain;

        // Use default implementation
        var defaultDomain = await _defaultWebsiteChannelDomainProvider.GetDomain(websiteChannelGuid, cancellationToken);
        return defaultDomain;
    }
}