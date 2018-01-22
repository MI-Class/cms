﻿using System.Collections.Generic;
using SiteServer.Utils;
using SiteServer.Utils.Table;
using SiteServer.CMS.Core;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;
using SiteServer.Plugin;

namespace SiteServer.CMS.StlParser.StlEntity
{
    [Stl(Usage = "通用实体", Description = "通过 {stl.} 实体在模板中显示对应数据")]
    public class StlStlEntities
	{
		private StlStlEntities()
		{
		}

        public const string EntityName = "stl";

        public static string PoweredBy = "PoweredBy";
        public static string SiteName = "SiteName";
        public static string SiteId = "SiteId";
        public static string SiteDir = "SiteDir";
        public static string SiteUrl = "SiteUrl";
		public static string RootUrl = "RootUrl";
        public static string ApiUrl = "ApiUrl";
        public static string CurrentUrl = "CurrentUrl";
        public static string ChannelUrl = "ChannelUrl";

	    public static SortedList<string, string> AttributeList => new SortedList<string, string>
	    {
	        {PoweredBy, "PoweredBy 链接"},
	        {SiteName, "站点名称"},
	        {SiteId, "站点ID"},
	        {SiteDir, "站点文件夹"},
	        {SiteUrl, "站点根目录地址"},
	        {RootUrl, "系统根目录地址"},
            {ApiUrl, "Api地址"},
            {CurrentUrl, "当前页地址"},
	        {ChannelUrl, "栏目页地址"}
	    };

        internal static string Parse(string stlEntity, PageInfo pageInfo, ContextInfo contextInfo)
        {
            var parsedContent = string.Empty;
            try
            {
                var entityName = StlParserUtility.GetNameFromEntity(stlEntity);
                var attributeName = entityName.Substring(5, entityName.Length - 6);

                if (StringUtils.EqualsIgnoreCase(PoweredBy, attributeName))//支持信息
                {
                    parsedContent = @"Powered by <a href=""http://www.siteserver.cn"" target=""_blank"">SiteServer CMS</a>";
                }
                else if (StringUtils.EqualsIgnoreCase(RootUrl, attributeName))//系统根目录地址
                {
                    parsedContent = PageUtils.ParseConfigRootUrl("~");
                    if (!string.IsNullOrEmpty(parsedContent))
                    {
                        parsedContent = parsedContent.TrimEnd('/');
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(ApiUrl, attributeName))//API地址
                {
                    parsedContent = pageInfo.ApiUrl.TrimEnd('/');
                }
                else if (StringUtils.EqualsIgnoreCase(SiteId, attributeName))//ID
                {
                    parsedContent = pageInfo.PublishmentSystemId.ToString();
                }
                else if (StringUtils.EqualsIgnoreCase(SiteName, attributeName))//名称
                {
                    parsedContent = pageInfo.PublishmentSystemInfo.PublishmentSystemName;
                }
                else if (StringUtils.EqualsIgnoreCase(SiteUrl, attributeName))//域名地址
                {
                    parsedContent = PageUtility.GetPublishmentSystemUrl(pageInfo.PublishmentSystemInfo, pageInfo.IsLocal).TrimEnd('/');
                }
                else if (StringUtils.EqualsIgnoreCase(SiteDir, attributeName))//文件夹
                {
                    parsedContent = pageInfo.PublishmentSystemInfo.PublishmentSystemDir;
                }
                else if (StringUtils.EqualsIgnoreCase(CurrentUrl, attributeName))//当前页地址
                {
                    parsedContent = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId, pageInfo.IsLocal);
                }
                else if (StringUtils.EqualsIgnoreCase(ChannelUrl, attributeName))//栏目页地址
                {
                    parsedContent = PageUtility.GetChannelUrl(pageInfo.PublishmentSystemInfo, NodeManager.GetNodeInfo(pageInfo.PublishmentSystemId, contextInfo.ChannelId), pageInfo.IsLocal);
                }
                //else if (StringUtils.EqualsIgnoreCase(HomeUrl, attributeName))//用户中心地址
                //{
                //    parsedContent = pageInfo.HomeUrl.TrimEnd('/');
                //}
                //else if (StringUtils.EqualsIgnoreCase(attributeName, LoginUrl))
                //{
                //    var returnUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
                //    parsedContent = HomeUtils.GetLoginUrl(pageInfo.HomeUrl, returnUrl);
                //}
                //else if (StringUtils.EqualsIgnoreCase(attributeName, LogoutUrl))
                //{
                //    var returnUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
                //    parsedContent = HomeUtils.GetLogoutUrl(pageInfo.HomeUrl, returnUrl);
                //}
                //else if (StringUtils.EqualsIgnoreCase(attributeName, RegisterUrl))
                //{
                //    var returnUrl = StlUtility.GetStlCurrentUrl(pageInfo.PublishmentSystemInfo, contextInfo.ChannelId, contextInfo.ContentId, contextInfo.ContentInfo, pageInfo.TemplateInfo.TemplateType, pageInfo.TemplateInfo.TemplateId);
                //    parsedContent = HomeUtils.GetRegisterUrl(pageInfo.HomeUrl, returnUrl);
                //}
                else if (StringUtils.StartsWithIgnoreCase(attributeName, "TableFor"))//
                {
                    if (StringUtils.EqualsIgnoreCase(attributeName, "TableForContent"))
                    {
                        parsedContent = pageInfo.PublishmentSystemInfo.AuxiliaryTableForContent;
                    }
                }
                else if (StringUtils.StartsWithIgnoreCase(attributeName, "Site"))//
                {
                    parsedContent = pageInfo.PublishmentSystemInfo.Additional.GetString(attributeName.Substring(4));
                }
                else
                {
                    if (pageInfo.PublishmentSystemInfo.Additional.ContainsKey(attributeName))
                    {
                        parsedContent = pageInfo.PublishmentSystemInfo.Additional.GetString(attributeName);
                         
                        if (!string.IsNullOrEmpty(parsedContent))
                        {
                            var styleInfo = TableStyleManager.GetTableStyleInfo(DataProvider.PublishmentSystemDao.TableName, attributeName, RelatedIdentities.GetRelatedIdentities(pageInfo.PublishmentSystemId, pageInfo.PublishmentSystemId));
                            
                            // 如果 styleInfo.TableStyleId <= 0，表示此字段已经被删除了，不需要再显示值了 ekun008
                            if (styleInfo.TableStyleId > 0)
                            {
                                parsedContent = InputTypeUtils.EqualsAny(styleInfo.InputType, InputType.Image,
                                    InputType.File)
                                    ? PageUtility.ParseNavigationUrl(pageInfo.PublishmentSystemInfo, parsedContent,
                                        pageInfo.IsLocal)
                                    : InputParserUtility.GetContentByTableStyle(parsedContent, string.Empty,
                                        pageInfo.PublishmentSystemInfo, styleInfo, string.Empty, null, string.Empty,
                                        true);
                            }
                            else
                            { // 如果字段已经被删除或不再显示了，则此字段的值为空。有时虚拟字段值不会清空
                                parsedContent = string.Empty;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return parsedContent;
        }
	}
}