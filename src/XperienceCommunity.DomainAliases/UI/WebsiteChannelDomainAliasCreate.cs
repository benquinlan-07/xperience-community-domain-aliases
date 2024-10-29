using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XperienceCommunity.DomainAliases.Models;
using XperienceCommunity.DomainAliases.UI;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(parentType: typeof(WebsiteChannelDomainAliasListing), slug: "create", uiPageType: typeof(WebsiteChannelDomainAliasCreate), name: "Create", templateName: TemplateNames.EDIT, order: 10000)]
[assembly: UIPage(parentType: typeof(WebsiteChannelDomainAliasListing), slug: PageParameterConstants.PARAMETERIZED_SLUG, uiPageType: typeof(WebsiteChannelDomainAliasEdit), name: "Edit", templateName: TemplateNames.EDIT, order: 10000)]

namespace XperienceCommunity.DomainAliases.UI;

public class WebsiteChannelDomainAliasEdit : WebsiteChannelDomainAliasCreate
{
    public WebsiteChannelDomainAliasEdit(IFormItemCollectionProvider formItemCollectionProvider, IFormDataBinder formDataBinder, IInfoProvider<WebsiteChannelDomainAliasInfo> websiteChannelDomainAliasInfoProvider) : base(formItemCollectionProvider, formDataBinder, websiteChannelDomainAliasInfoProvider)
    {
    }
}
public class WebsiteChannelDomainAliasCreate : ModelEditPage<WebsiteChannelDomainAliasCreate.EditModel>
{
    private readonly IInfoProvider<WebsiteChannelDomainAliasInfo> _websiteChannelDomainAliasInfoProvider;
    private EditModel _model;

    public WebsiteChannelDomainAliasCreate(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<WebsiteChannelDomainAliasInfo> websiteChannelDomainAliasInfoProvider)
        : base(formItemCollectionProvider, formDataBinder)
    {
        _websiteChannelDomainAliasInfoProvider = websiteChannelDomainAliasInfoProvider;
    }

    [PageParameter(typeof(IntPageModelBinder), typeof(ChannelEditSection))]
    public int ChannelId { get; set; }

    [PageParameter(typeof(IntPageModelBinder))]
    public int ObjectId { get; set; }

    /// <inheritdoc />
    public override async Task ConfigurePage()
    {
        var websiteChannelDomainAlias = GetDomainAlias();

        if (websiteChannelDomainAlias != null)
        {
            Model.Domain = websiteChannelDomainAlias.WebsiteChannelDomainAliasDomain;
        }

        await base.ConfigurePage();
    }

    private WebsiteChannelDomainAliasInfo GetDomainAlias()
    {
        if (this is WebsiteChannelDomainAliasEdit)
        {
            return _websiteChannelDomainAliasInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasId), ObjectId)
                .FirstOrDefault();
        }

        return null;
    }

    /// <inheritdoc />
    protected override async Task<ICommandResponse> ProcessFormData(EditModel model, ICollection<IFormItem> formItems)
    {
        WebsiteChannelDomainAliasInfo websiteChannelDomainAlias;
        if (this is WebsiteChannelDomainAliasEdit)
        {
            websiteChannelDomainAlias = GetDomainAlias();
        }
        else
        {
            websiteChannelDomainAlias = new WebsiteChannelDomainAliasInfo()
            {
                WebsiteChannelDomainAliasGuid = Guid.NewGuid(),
                WebsiteChannelDomainAliasChannelId = ChannelId
            };
        }

        websiteChannelDomainAlias.WebsiteChannelDomainAliasDomain = model.Domain;

        _websiteChannelDomainAliasInfoProvider.Set(websiteChannelDomainAlias);

        // Initializes a client response
        var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationSuccess)
        {
            // Returns the submitted field values to the client (repopulates the form)
            Items = await formItems.OnlyVisible().GetClientProperties(),
        });

        response.AddSuccessMessage($"Domain alias has been {(this is WebsiteChannelDomainAliasEdit ? "updated" : "created")}.");

        return response;
    }

    protected override EditModel Model
    {
        get { return _model ??= new EditModel(); }
    }

    public class EditModel
    {
        [TextInputComponent(Label = "Domain")]
        [RequiredValidationRule(ErrorMessage = "Domain is required.")]
        public string Domain { get; set; }
    }
}