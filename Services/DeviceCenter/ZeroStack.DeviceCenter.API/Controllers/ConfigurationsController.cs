﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ZeroStack.DeviceCenter.Application.Services.Permissions;

namespace ZeroStack.DeviceCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfigurationsController : ControllerBase
    {
        private readonly ILogger<ConfigurationsController> _logger;
        private readonly AuthorizationOptions _authorizationOptions;
        private readonly RequestLocalizationOptions _requestLocalizationOptions;
        private readonly IPermissionDefinitionManager _permissionDefinitionManager;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public ConfigurationsController(ILogger<ConfigurationsController> logger,
          IHttpContextAccessor httpContextAccessor,
          IOptions<AuthorizationOptions> authorizationOptions,
          IOptions<RequestLocalizationOptions> requestLocalizationOptions,
          IPermissionDefinitionManager permissionDefinitionManager,
          IPermissionChecker permissionChecker,
          IAuthorizationService authorizationService,
          IStringLocalizerFactory stringLocalizerFactory)
        {
            _logger = logger ?? NullLogger<ConfigurationsController>.Instance;
            _httpContextAccessor = httpContextAccessor;
            _authorizationOptions = authorizationOptions.Value;
            _requestLocalizationOptions = requestLocalizationOptions.Value;
            _permissionDefinitionManager = permissionDefinitionManager;
            _permissionChecker = permissionChecker;
            _authorizationService = authorizationService;
            _stringLocalizerFactory = stringLocalizerFactory;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ApplicationConfiguration> GetAsync()
        {
            _logger.LogDebug("Executing ConfigurationApplicationService.GetAsync()...");

            var result = new ApplicationConfiguration
            {
                Permissions = await GetPermissionConfigurationAsync()
            };

            _logger.LogDebug("Executed ConfigurationApplicationService.GetAsync().");

            return result;
        }

        [NonAction]
        private async Task<PermissionConfiguration> GetPermissionConfigurationAsync()
        {
            PermissionConfiguration permissionConfiguration = new();

            IEnumerable<string> policyNames = _permissionDefinitionManager.GetPermissions().Select(p => p.Name);

            PropertyInfo? policyMapProperty = typeof(AuthorizationOptions).GetProperty("PolicyMap", BindingFlags.Instance | BindingFlags.NonPublic);
            if (policyMapProperty is not null)
            {
                object? policyMapPropertyValue = policyMapProperty.GetValue(_authorizationOptions);
                if (policyMapPropertyValue is not null)
                {
                    policyNames = policyNames.Union(((IDictionary<string, AuthorizationPolicy>)policyMapPropertyValue).Keys.ToList());
                }
            }

            List<string> permissionPolicyNames = new();
            List<string> otherPolicyNames = new();

            foreach (var policyName in policyNames)
            {
                if (_permissionDefinitionManager.GetOrNull(policyName) is not null)
                {
                    permissionPolicyNames.Add(policyName);
                }
                else
                {
                    otherPolicyNames.Add(policyName);
                }
            }

            foreach (var policyName in otherPolicyNames)
            {
                permissionConfiguration.Policies[policyName] = true;

                if (_httpContextAccessor is not null && _httpContextAccessor.HttpContext is not null)
                {
                    if ((await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, policyName)).Succeeded)
                    {
                        permissionConfiguration.GrantedPolicies[policyName] = true;
                    }
                }
            }

            MultiplePermissionGrantResult result = await _permissionChecker.IsGrantedAsync(permissionPolicyNames.ToArray());

            foreach (var (key, value) in result.Result)
            {
                permissionConfiguration.Policies[key] = true;
                if (value == PermissionGrantResult.Granted)
                {
                    permissionConfiguration.GrantedPolicies[key] = true;
                }
            }

            return permissionConfiguration;
        }
    }

    [Serializable]
    public class ApplicationConfiguration
    {
        public PermissionConfiguration? Permissions { get; set; }
    }

    [Serializable]
    public class PermissionConfiguration
    {
        public Dictionary<string, bool> Policies { get; set; }

        public Dictionary<string, bool> GrantedPolicies { get; set; }

        public PermissionConfiguration()
        {
            Policies = new Dictionary<string, bool>();
            GrantedPolicies = new Dictionary<string, bool>();
        }
    }
}
