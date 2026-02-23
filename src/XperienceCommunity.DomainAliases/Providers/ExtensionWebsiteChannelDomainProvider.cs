using CMS.Websites.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

    private WebsiteChannelDescriptor GetAliasDescriptor()
    {
        // Middleware previously set the website channel descriptor in the HttpContext.Items if a domain alias matched, so attempt to retrieve it here
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items == null)
            return null;

        if (items.TryGetValue(ALIAS_WEBSITE_CHANNEL_CONTEXT_KEY, out var websiteDescriptorValue) && websiteDescriptorValue is WebsiteChannelDescriptor value)
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

    public Task<ICollection<Uri>> GetAllDomains(int websiteChannelId, CancellationToken cancellationToken)
    {
        // Use default implementation
        return _defaultWebsiteChannelDomainProvider.GetAllDomains(websiteChannelId, cancellationToken);
    }

    public Task<string> GetDomain(int websiteChannelId, CancellationToken cancellationToken)
    {
        // Use default implementation
        return _defaultWebsiteChannelDomainProvider.GetDomain(websiteChannelId, cancellationToken);
    }

    public Task<string> GetDomain(Guid websiteChannelGuid, CancellationToken cancellationToken)
    {
        // Use default implementation
        return _defaultWebsiteChannelDomainProvider.GetDomain(websiteChannelGuid, cancellationToken);
    }
}