// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.DataAccess.Models;
using EdFi.Ods.AdminApi.AdminConsole.DataAccess.Models;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Database.Commands;
using EdFi.Ods.AdminApi.AdminConsole.Services;
using EdFi.Ods.AdminApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Respawn;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EdFi.Ods.AdminConsole.DBTests.Database.CommandTests;

[TestFixture]
public class AddInstanceCommandTests : PlatformUsersContextTestBase
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
        var document = "nee";
        AddInstanceResult result = null;
        Transaction(dbContext =>
        {
            var command = new AddInstanceCommand(dbContext, Testing.GetEncryptionKeyResolver(), new EncryptionService());
            var newInstance = new TestInstance
            {
                InstanceId = 1,
                DocId = null,
                EdOrgId = 1,
                Document = document
            };

            result = command.Execute(newInstance);
        });

        Transaction(dbContext =>
        {
            var persistedInstance = dbContext.Instances;
            persistedInstance.Count().ShouldBe(1);
            persistedInstance.First().DocId.ShouldNotBeNull();
            persistedInstance.First().InstanceId.ShouldBe(1);
            persistedInstance.First().EdOrgId.ShouldBe(1);
            persistedInstance.First().Document.ShouldBe(document);
        });
    }

    private class TestInstance : IAddInstanceModel
    {
        public int? DocId { get; set; }
        public int? InstanceId { get; set; }
        public int? EdOrgId { get; set; }
        public string Document { get; set; }
    }
}
