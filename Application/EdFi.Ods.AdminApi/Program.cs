// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AspNetCoreRateLimit;
using Autofac.Core;
using EdFi.Ods.AdminApi.AdminConsole.DataAccess;
using EdFi.Ods.AdminApi.AdminConsole.Helpers;
using EdFi.Ods.AdminApi.AdminConsole.Services;
using EdFi.Ods.AdminApi.Features;
using EdFi.Ods.AdminApi.Helpers;
using EdFi.Ods.AdminApi.Infrastructure;
using EdFi.Ods.AdminApi.Infrastructure.MultiTenancy;
using log4net;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//Rate Limit
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
builder.Services.AddTransient<IEncryptionKeySettings>(sp => sp.GetService<IOptions<AdminConsoleSettings>>().Value);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
builder.Services.AddTransient<IEncryptionKeyResolver, OptionsEncryptionKeyResolver>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();


builder.AddServices();

//DbSetup
DbSetup.ConfigureDatabase(builder.Services, builder.Configuration);

// logging
var _logger = LogManager.GetLogger("Program");
_logger.Info("Starting Admin API");

var app = builder.Build();


var pathBase = app.Configuration.GetValue<string>("AppSettings:PathBase");
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase($"/{pathBase.Trim('/')}");
    app.UseForwardedHeaders();
}

AdminApiVersions.Initialize(app);

app.UseIpRateLimiting();
//The ordering here is meaningful: Logging -> Routing -> Auth -> Endpoints
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapFeatureEndpoints();
//Map AdminConsole endpoints if the flag is enable
if (app.Configuration.GetValue<bool>("AppSettings:EnableAdminConsoleAPI"))
{
    app.MapAdminConsoleFeatureEndpoints();
}
app.MapControllers();
app.UseHealthChecks("/health");

if (app.Configuration.GetValue<bool>("SwaggerSettings:EnableSwagger"))
{
    app.UseSwagger();
    app.DefineSwaggerUIWithApiVersions(AdminApiVersions.GetAllVersionStrings());
}

app.Run();
