﻿using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ZeroStack.IdentityServer.API.Infrastructure.Tenants;

namespace ZeroStack.IdentityServer.API.Models
{
    public static class SampleDatas
    {
        public static IEnumerable<IdentityResource> Ids => new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("userinfo", "Your user information", new []
            {
                JwtClaimTypes.Role,
                JwtRegisteredClaimNames.UniqueName,
                JwtClaimTypes.NickName,
                JwtClaimTypes.Address,
                JwtClaimTypes.Email
            })
        };

        public static IEnumerable<ApiResource> Apis => new List<ApiResource>
        {
            new ApiResource("devicecenterapi", "Device Center API",new []{ JwtClaimTypes.Email })
            {
                Scopes= { "openapi", "devicecenter" }
            },
            new ApiResource("identityserverapi", "Identity Server API",new []{ JwtClaimTypes.PhoneNumber,JwtClaimTypes.Gender})
            {
                Scopes= { "openapi", "identityserver" }
            }
        };

        public static IEnumerable<ApiScope> ApiScopes => new List<ApiScope>
        {
            new ApiScope("openapi", "All open web api", new []{ JwtClaimTypes.Role, TenantClaimTypes.TenantId, JwtClaimTypes.Name}),
            new ApiScope("identityserver", "Identity server api", new []{ JwtClaimTypes.Role, TenantClaimTypes.TenantId, JwtClaimTypes.Name}),
            new ApiScope("devicecenter", "Device center api", new []{ JwtClaimTypes.Role, TenantClaimTypes.TenantId,JwtClaimTypes.Name})
        };

        public static IEnumerable<Client> Clients => new List<Client>
        {
            new Client
            {
                ClientId = "devicecenterswagger",
                ClientName = "Device Center Swagger",
                ClientSecrets = { new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret=false,
                AlwaysSendClientClaims=true,
                AlwaysIncludeUserClaimsInIdToken=true,
                RequireConsent = true,
                RedirectUris = {
                    "https://localhost:6001/swagger/oauth2-redirect.html",
                    "https://devicecenterapi.xcode.me:6001/swagger/oauth2-redirect.html"
                },
                PostLogoutRedirectUris = {
                    "https://localhost:6001/swagger",
                    "https://devicecenterapi.xcode.me:6001/swagger"
                },
                AllowOfflineAccess=true,
                RequirePkce = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "devicecenter"
                }
            },
            new Client
            {
                ClientId = "identityserverswagger",
                ClientName = "Identity Server Swagger",
                ClientSecrets = { new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret=false,
                AlwaysSendClientClaims=true,
                AlwaysIncludeUserClaimsInIdToken=true,
                RequireConsent = true,
                RedirectUris = {
                    "https://localhost:5001/swagger/oauth2-redirect.html",
                    "https://identityserver.xcode.me:5001/swagger/oauth2-redirect.html"
                },
                PostLogoutRedirectUris = {
                    "https://localhost:5001/swagger",
                    "https://identityserver.xcode.me:5001/swagger"
                },
                AllowOfflineAccess=true,
                RequirePkce = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "identityserver"
                }
            },
            new Client
            {
                ClientId = "devicecenterweb",
                ClientName = "Device Center Web",
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret=false,
                AlwaysSendClientClaims=true,
                AlwaysIncludeUserClaimsInIdToken=true,
                RequireConsent = true,
                RedirectUris = {
                    "http://localhost:8000/authorization/login-callback",
                },
                PostLogoutRedirectUris = {
                    "http://localhost:8000/authorization/logout-callback",
                },
                AllowOfflineAccess=true,
                RequirePkce = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "userinfo",
                    "openapi",
                    "identityserver",
                    "devicecenter"
                },
                AllowedCorsOrigins = { "http://localhost:8000"},
                AccessTokenLifetime = 36000
            }
        };

        public static IEnumerable<(int UserId, string UserName, string Password, string PhoneNumber, string Email, Guid? TenantId, IEnumerable<Claim> Claims)> Users()
        {
            var result = new List<(int UserId, string UserName, string Password, string PhoneNumber, string Email, Guid? TenantId, IEnumerable<Claim> Claims)>
            {
                (UserId:1, UserName:"user1", Password: "user1", PhoneNumber:"13789685636", Email:"user1@xcode.me",null, new Claim[]
                {
                    new(JwtClaimTypes.Gender, "male", ClaimValueTypes.String),
                    new(JwtClaimTypes.BirthDate, "1992-11-10", ClaimValueTypes.Date),
                    new(JwtClaimTypes.Role, "role1", ClaimValueTypes.String)
                }),

                (UserId:2, UserName:"user2", Password: "user2", PhoneNumber:"18965636598", Email:"user2@xcode.me",new Guid("F30E402B-9DE2-4B48-9FF0-C073CF499102"), new Claim[]
                {
                    new(JwtClaimTypes.NickName, "female", ClaimValueTypes.String),
                    new(JwtClaimTypes.BirthDate, "1996-06-12", ClaimValueTypes.Date),
                    new(JwtClaimTypes.Role, "role2", ClaimValueTypes.String)
                }),

                (UserId:3, UserName:"user3", Password: "user3", PhoneNumber:"13656598653", Email:"user3@xcode.me",new Guid("F30E402B-9DE2-4B48-9FF0-C073CF499103"), new Claim[]
                {
                    new(JwtClaimTypes.Gender, "male", ClaimValueTypes.String),
                    new(JwtClaimTypes.BirthDate, "1998-03-18", ClaimValueTypes.Date),
                    new(JwtClaimTypes.Role, "IdentityManager", ClaimValueTypes.String),
                    new(JwtClaimTypes.Role, "role1", ClaimValueTypes.String),
                    new(JwtClaimTypes.Role, "role2", ClaimValueTypes.String)
                })
            };

            System.Random random = new(System.Environment.TickCount);

            for (int i = 5; i < 100; i++)
            {
                result.Add((UserId: i, UserName: $"user{i}", Password: $"user{i}", PhoneNumber: $"{random.Next(130, 190)}{random.Next(10000000, 99999999)}", Email: $"{System.IO.Path.GetRandomFileName().Replace(".", string.Empty)}@xcode.me", null, new Claim[]
                {
                    new(JwtClaimTypes.Role, "IdentityManager", ClaimValueTypes.String),
                    new(JwtClaimTypes.Role, "role1", ClaimValueTypes.String),
                    new(JwtClaimTypes.Role, "role2", ClaimValueTypes.String)
                }));
            }

            return result;
        }
    }
}