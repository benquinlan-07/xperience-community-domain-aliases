using CMS;
using CMS.Core;
using CMS.Websites.Internal;
using CMS.Websites.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using XperienceCommunity.DomainAliases.Providers;

namespace XperienceCommunity.DomainAliases;

public static class ExtensionStartupExtensions
{
    /// <summary>
    /// Adds page type restrictions extension dependencies
    /// </summary>
    /// <param name="serviceCollection">the <see cref="IServiceCollection"/> which will be modified</param>
    /// <returns>Returns this instance of <see cref="IServiceCollection"/>, allowing for further configuration in a fluent manner.</returns>
    public static IServiceCollection AddDomainAliasesExtensionServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<ExtensionModuleInstaller>();

        var websiteChannelDomainProvider = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(IWebsiteChannelDomainProvider));
        if (websiteChannelDomainProvider != null)
        {
            serviceCollection.Remove(websiteChannelDomainProvider);
            serviceCollection.AddKeyedSingleton(typeof(IWebsiteChannelDomainProvider), "kentico", websiteChannelDomainProvider.ImplementationType);
            serviceCollection.AddSingleton<IWebsiteChannelDomainProvider, ExtensionWebsiteChannelDomainProvider>();
        }

        return serviceCollection;
    }
}