using XperienceCommunity.DomainAliases.UI;
using CMS.Core;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Admin.Websites.UIPages;
using System.Threading.Tasks;
using XperienceCommunity.DomainAliases.Models;

[assembly: UIPage(parentType: typeof(ChannelEditSection), slug: "domain-aliases", uiPageType: typeof(WebsiteChannelDomainAliasListing), name: "Domain aliases", templateName: TemplateNames.LISTING, order: 10000)]

namespace XperienceCommunity.DomainAliases.UI
{
    [UINavigation(false)]
    public class WebsiteChannelDomainAliasListing : ListingPage
    {
        protected override string ObjectType => WebsiteChannelDomainAliasInfo.OBJECT_TYPE;

        public override Task ConfigurePage()
        {
            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasDomain), "Domain");

            PageConfiguration.QueryModifiers.Add(new QueryModifier((query, settings) => query.WhereEquals(nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasChannelId), ChannelId)));

            PageConfiguration.HeaderActions.AddLink<WebsiteChannelDomainAliasCreate>("New domain alias", parameters: new[] { ChannelId.ToString() });
            PageConfiguration.AddEditRowAction<WebsiteChannelDomainAliasEdit>(parameters: new[] { ChannelId.ToString() });
            PageConfiguration.TableActions.AddDeleteAction("Delete");

            return base.ConfigurePage();
        }

        [PageCommand]
        public override Task<ICommandResponse<RowActionResult>> Delete(int id)
        {
            return base.Delete(id);
        }

        [PageParameter(typeof(IntPageModelBinder), typeof(ChannelEditSection))]
        public int ChannelId { get; set; }
    }
}
