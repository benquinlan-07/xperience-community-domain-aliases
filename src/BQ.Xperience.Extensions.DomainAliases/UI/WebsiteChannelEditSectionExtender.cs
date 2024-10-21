using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BQ.Xperience.Extensions.DomainAliases.UI;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

[assembly: PageExtender(typeof(WebsiteChannelEditSectionExtender))]

namespace BQ.Xperience.Extensions.DomainAliases.UI;

internal class WebsiteChannelEditSectionExtender : PageExtender<ChannelEditSection>
{
    /// <inheritdoc />
    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        properties = await base.ConfigureTemplateProperties(properties);
        var navigation = properties.Navigation;
        navigation.Items = await AddChannelSpecificNavigation(properties.Navigation.Items);
        return properties;
    }

    private async Task<IEnumerable<NavigationItem>> AddChannelSpecificNavigation(IEnumerable<NavigationItem> navigation)
    {
        var async = await AbstractInfo<ChannelInfo, IInfoProvider<ChannelInfo>>.Provider.GetAsync<ChannelInfo>(Page.ObjectId);
        if (async == null)
            return navigation;

        var list = navigation.ToList();
        var editNavigationItems = GetChannelEditNavigationItems(async.ChannelType);
        if (editNavigationItems.Count == 0)
            return navigation;

        list.AddRange(editNavigationItems);
        return list;
    }

    private List<NavigationItem> GetChannelEditNavigationItems(ChannelType channelType)
    {
        if (channelType != ChannelType.Website)
            return new List<NavigationItem>();

        return new List<NavigationItem>()
        {
            new NavigationItem()
            {
                Label = "Domain aliases",
                Path = "domain-aliases"
            }
        };
    }
}