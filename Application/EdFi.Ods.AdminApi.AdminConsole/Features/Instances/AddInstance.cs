// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Database.Commands;
using EdFi.Ods.AdminApi.Features;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EdFi.Ods.AdminApi.AdminConsole.Features.Instances;

public class AddInstance : IAdminConsoleFeature
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        AdminApiAdminConsoleEndpointBuilder.MapPost(endpoints, "/instances", Handle)
            .WithRouteOptions(b => b.WithResponse<AddInstanceResult>(201));
    }

    public async Task<IResult> Handle(Validator validator, IAddInstanceCommand addInstanceCommand, AddInstanceRequest request)
    {
        await validator.GuardAsync(request);
        var addedInstanceResult = addInstanceCommand.Execute(request);

        return Results.Created($"/instances/{addedInstanceResult.DocId}", addedInstanceResult);
    }

    public class AddInstanceRequest : IAddInstanceModel
    {
        public int? DocId { get; set; }

        public int? InstanceId { get; set; }

        public int? EdOrgId { get; set; }

        public required string Document { get; set; }
    }

    public class Validator : AbstractValidator<AddInstanceRequest>
    {
        public Validator()
        {
            RuleFor(m => m.InstanceId)
             .NotNull();

            RuleFor(m => m.EdOrgId)
             .NotNull();

            RuleFor(m => m.Document)
             .NotNull().NotEmpty();
        }
    }
}
