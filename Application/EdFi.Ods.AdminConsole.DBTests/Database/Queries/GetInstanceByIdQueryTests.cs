// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Database.Commands;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Database.Queries;
using EdFi.Ods.AdminApi.AdminConsole.Services;
using EdFi.Ods.AdminApi.Helpers;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace EdFi.Ods.AdminConsole.DBTests.Database.CommandTests;

[TestFixture]
public class GetInstanceByIdQueryTests : PlatformUsersContextTestBase
{
    private IOptions<AppSettings> _options { get; set; }

    [OneTimeSetUp]
    public virtual async Task FixtureSetup()
    {
        AdminConsoleSettings appSettings = new AdminConsoleSettings();
        await Task.Yield();
    }

    [Test]
    public void ShouldExecute()
    {
        var instanceDocument = "{\"instanceId\":\"DEF456\",\"tenantId\":\"def456\",\"instanceName\":\"Mock Instance 2\",\"instanceType\":\"Type B\",\"connectionType\":\"Type Y\",\"clientId\":\"CLIENT321\",\"clientSecret\":\"SECRET456\",\"baseUrl\":\"https://localhost/api\",\"authenticationUrl\":\"https://localhost/api/oauth/token\",\"resourcesUrl\":\"https://localhost/api\",\"schoolYears\":[2024,2025],\"isDefault\":false,\"verificationStatus\":null,\"provider\":\"Local\"}";
        AddInstanceResult result = null;

        var newInstance = new TestInstance
        {
            InstanceId = 1,
            TenantId = 1,
            EdOrgId = 1,
            Document = instanceDocument
        };

        Transaction(dbContext =>
        {
            var command = new AddInstanceCommand(dbContext, Testing.GetEncryptionKeyResolver(), new EncryptionService());

            result = command.Execute(newInstance);
        });

        Transaction(dbContext =>
        {
            var query = new GetInstanceByIdQuery(dbContext, Testing.GetEncryptionKeyResolver(), new EncryptionService());
            var instance = query.Execute(result.DocId);

            instance.DocId.ShouldBe(result.DocId);
            instance.TenantId.ShouldBe(newInstance.TenantId);
            instance.InstanceId.ShouldBe(newInstance.InstanceId);
            instance.EdOrgId.ShouldBe(newInstance.EdOrgId);
            instance.Document.ShouldBe(newInstance.Document);
        });
    }

    private class TestInstance : IAddInstanceModel
    {
        public int? TenantId { get; set; }
        public int? InstanceId { get; set; }
        public int? EdOrgId { get; set; }
        public string Document { get; set; }
    }
}
