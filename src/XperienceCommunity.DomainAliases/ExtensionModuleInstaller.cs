using XperienceCommunity.DomainAliases.Models;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace XperienceCommunity.DomainAliases;

internal class ExtensionModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> _resourceProvider;

    public ExtensionModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider)
    {
        _resourceProvider = resourceProvider;
    }

    public void Install()
    {
        var resource = _resourceProvider.Get(Constants.ResourceName)
                       ?? new ResourceInfo();

        InitializeResource(resource);
        InstallWebsiteChannelDomainAliasInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = Constants.ResourceDisplayName;
        resource.ResourceName = Constants.ResourceName;
        resource.ResourceDescription = Constants.ResourceDescription;
        resource.ResourceIsInDevelopment = false;
        
        if (resource.HasChanged)
            _resourceProvider.Set(resource);

        return resource;
    }

    public void InstallWebsiteChannelDomainAliasInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(WebsiteChannelDomainAliasInfo.OBJECT_TYPE) ?? DataClassInfo.New(WebsiteChannelDomainAliasInfo.OBJECT_TYPE);

        info.ClassName = WebsiteChannelDomainAliasInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = WebsiteChannelDomainAliasInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = WebsiteChannelDomainAliasInfo.OBJECT_CLASS_DISPLAYNAME;
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasChannelId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = ChannelInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(WebsiteChannelDomainAliasInfo.WebsiteChannelDomainAliasDomain),
            AllowEmpty = false,
            Visible = true,
            Size = 200,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}