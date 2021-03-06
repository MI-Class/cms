﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using SiteServer.CMS.Api;
using SiteServer.CMS.Core;
using SiteServer.CMS.Model;
using SiteServer.CMS.Plugin;

namespace SiteServer.API.Controllers.Pages.Settings
{
    [RoutePrefix("api/pages/settings/adminAccessTokens")]
    public class AdminAccessTokensController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetList()
        {
            try
            {
                var request = new AuthRequest();
                if (!request.IsAdminLoggin ||
                    !request.AdminPermissions.HasSystemPermissions(ConfigManager.SettingsPermissions.Admin))
                {
                    return Unauthorized();
                }

                var adminNames = new List<string>();

                if (request.AdminPermissions.IsConsoleAdministrator)
                {
                    adminNames = DataProvider.AdministratorDao.GetUserNameList();
                }
                else
                {
                    adminNames.Add(request.AdminName);
                }

                var scopes = new List<string>(AccessTokenManager.ScopeList);

                foreach (var service in PluginManager.Services)
                {
                    if (service.IsApiAuthorization)
                    {
                        scopes.Add(service.PluginId);
                    }
                }

                return Ok(new
                {
                    Value = DataProvider.AccessTokenDao.GetAccessTokenInfoList(),
                    adminNames,
                    scopes,
                    request.AdminName
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route(Route)]
        public IHttpActionResult Delete([FromBody] IdObj delObj)
        {
            try
            {
                var request = new AuthRequest();
                if (!request.IsAdminLoggin ||
                    !request.AdminPermissions.HasSystemPermissions(ConfigManager.SettingsPermissions.Admin))
                {
                    return Unauthorized();
                }

                DataProvider.AccessTokenDao.Delete(delObj.Id);

                return Ok(new
                {
                    Value = DataProvider.AccessTokenDao.GetAccessTokenInfoList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit([FromBody] AccessTokenInfo itemObj)
        {
            try
            {
                var request = new AuthRequest();
                if (!request.IsAdminLoggin ||
                    !request.AdminPermissions.HasSystemPermissions(ConfigManager.SettingsPermissions.Admin))
                {
                    return Unauthorized();
                }

                if (itemObj.Id > 0)
                {
                    var tokenInfo = DataProvider.AccessTokenDao.GetAccessTokenInfo(itemObj.Id);

                    if (tokenInfo.Title != itemObj.Title && DataProvider.AccessTokenDao.IsTitleExists(itemObj.Title))
                    {
                        return BadRequest("保存失败，已存在相同标题的API密钥！");
                    }

                    tokenInfo.Title = itemObj.Title;
                    tokenInfo.AdminName = itemObj.AdminName;
                    tokenInfo.Scopes = itemObj.Scopes;

                    DataProvider.AccessTokenDao.Update(tokenInfo);

                    request.AddAdminLog("修改API密钥", $"Access Token:{tokenInfo.Title}");
                }
                else
                {
                    if (DataProvider.AccessTokenDao.IsTitleExists(itemObj.Title))
                    {
                        return BadRequest("保存失败，已存在相同标题的API密钥！");
                    }

                    var tokenInfo = new AccessTokenInfo
                    {
                        Title = itemObj.Title,
                        AdminName = itemObj.AdminName,
                        Scopes = itemObj.Scopes
                    };

                    DataProvider.AccessTokenDao.Insert(tokenInfo);

                    request.AddAdminLog("新增API密钥", $"Access Token:{tokenInfo.Title}");
                }

                return Ok(new
                {
                    Value = DataProvider.AccessTokenDao.GetAccessTokenInfoList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
