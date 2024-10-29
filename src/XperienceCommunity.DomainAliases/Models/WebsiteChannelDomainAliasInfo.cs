using System;
using System.Data;
using System.Runtime.Serialization;
using XperienceCommunity.DomainAliases.Models;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(WebsiteChannelDomainAliasInfo), WebsiteChannelDomainAliasInfo.OBJECT_TYPE)]

namespace XperienceCommunity.DomainAliases.Models;

/// <summary>
/// Data container class for <see cref="WebsiteChannelDomainAliasInfo"/>.
/// </summary>
[Serializable]
public partial class WebsiteChannelDomainAliasInfo : AbstractInfo<WebsiteChannelDomainAliasInfo, IInfoProvider<WebsiteChannelDomainAliasInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "xpcm.websitechanneldomainalias";
    public const string OBJECT_CLASS_NAME = "XPCM.WebsiteChannelDomainAlias";
    public const string OBJECT_CLASS_DISPLAYNAME = "Website Channel Domain Alias";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<WebsiteChannelDomainAliasInfo>), OBJECT_TYPE, OBJECT_CLASS_NAME, nameof(WebsiteChannelDomainAliasId), null, nameof(WebsiteChannelDomainAliasGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Content type allowed type id.
    /// </summary>
    [DatabaseField]
    public virtual int WebsiteChannelDomainAliasId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WebsiteChannelDomainAliasId)), 0);
        set => SetValue(nameof(WebsiteChannelDomainAliasId), value);
    }


    /// <summary>
    /// Content type allowed type Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid WebsiteChannelDomainAliasGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(WebsiteChannelDomainAliasGuid)), default);
        set => SetValue(nameof(WebsiteChannelDomainAliasGuid), value);
    }


    /// <summary>
    /// Website channel domain alias id.
    /// </summary>
    [DatabaseField]
    public virtual int WebsiteChannelDomainAliasChannelId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WebsiteChannelDomainAliasChannelId)), default);
        set => SetValue(nameof(WebsiteChannelDomainAliasChannelId), value);
    }


    /// <summary>
    /// Website channel domain alias id.
    /// </summary>
    [DatabaseField]
    public virtual string WebsiteChannelDomainAliasDomain
    {
        get => ValidationHelper.GetString(GetValue(nameof(WebsiteChannelDomainAliasDomain)), default);
        set => SetValue(nameof(WebsiteChannelDomainAliasDomain), value);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Constructor for de-serialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected WebsiteChannelDomainAliasInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="WebsiteChannelDomainAliasInfo"/> class.
    /// </summary>
    public WebsiteChannelDomainAliasInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="WebsiteChannelDomainAliasInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public WebsiteChannelDomainAliasInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
