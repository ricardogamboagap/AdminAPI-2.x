// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.DataAccess.Contexts;
using EdFi.Ods.AdminApi.AdminConsole.DataAccess.Contexts.AdminConsoleSql;
using EdFi.Ods.AdminApi.AdminConsole.Helpers;
using EdFi.Ods.AdminApi.AdminConsole.Services;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Database.Commands;

public interface IAddInstanceCommand
{
    AddInstanceResult Execute(IAddInstanceModel applicationModel);
}

public class AddInstanceCommand : IAddInstanceCommand
{
    private readonly IDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;
    private readonly string _encryptionKey;

    public AddInstanceCommand(IDbContext dbContext, IEncryptionKeyResolver encryptionKeyResolver, IEncryptionService encryptionService)
    {
        _dbContext = dbContext;
        _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
        _encryptionService = encryptionService;
    }

    public AddInstanceResult Execute(IAddInstanceModel applicationModel)
    {
        JsonNode? jnDocument = JsonNode.Parse(applicationModel.Document);

        var clientId = jnDocument!["clientId"]?.AsValue().ToString();
        var clientSecret = jnDocument!["clientSecret"]?.AsValue().ToString();

        var encryptedClientId = string.Empty;
        var encryptedClientSecret = string.Empty;

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            _encryptionService.TryEncrypt(clientId, _encryptionKey, out encryptedClientId);
            _encryptionService.TryEncrypt(clientSecret, _encryptionKey, out encryptedClientSecret);

            jnDocument!["clientId"] = encryptedClientId;
            jnDocument!["clientSecret"] = encryptedClientSecret;
        }

        var newInstance = new DataAccess.Models.Instance
        {
            DocId = null,
            InstanceId = applicationModel.InstanceId,
            TenantId = applicationModel.TenantId,
            EdOrgId = applicationModel.EdOrgId,
            Document = jnDocument!.ToJsonString(),
        };

        _dbContext.Instances.Add(newInstance);

        _dbContext.SaveChanges();

        return new AddInstanceResult
        {
            DocId = newInstance.DocId.Value
        };
    }
}

public interface IAddInstanceModel
{
    int? DocId { get; set; }
    int? InstanceId { get; set; }
    int? TenantId { get; set; }
    int? EdOrgId { get; set; }
    string Document { get; set; }
}

public class AddInstanceResult
{
    public int DocId { get; set; }
}
