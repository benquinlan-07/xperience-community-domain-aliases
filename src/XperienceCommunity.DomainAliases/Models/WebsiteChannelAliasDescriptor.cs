using System;
using CMS.Websites.Internal;

namespace XperienceCommunity.DomainAliases.Models
{
    public class WebsiteChannelAliasDescriptor  : WebsiteChannelDescriptor
    {
        public Guid WebsiteChannelGUID { get; set; }
        public string AliasDomain { get; set; }
        public bool AliasDomainUseForAbsoluteUrl { get; set; }
    }
}
