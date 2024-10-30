// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Dynamic;
using EdFi.Ods.AdminApi.AdminConsole.Features.OdsInstances;
using EdFi.Ods.AdminApi.AdminConsole.Infrastructure.Database.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace EdFi.Ods.AdminApi.AdminConsole.Features.UserProfiles;

public class ReadInstances : IFeature
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        AdminApiAdminConsoleEndpointBuilder.MapGet(endpoints, "/instances", GetInstances)
            .BuildForVersions();

        AdminApiAdminConsoleEndpointBuilder.MapGet(endpoints, "/instances/{id}", GetInstance)
            .WithRouteOptions(b => b.WithResponse<InstanceModel>(200))
            .BuildForVersions();
    }

    internal Task<IResult> GetInstances()
    {
        using (StreamReader r = new StreamReader("Mockdata/data-odsinstances.json"))
        {
            string json = r.ReadToEnd();
            ExpandoObject result = JsonConvert.DeserializeObject<ExpandoObject>(json);
            return Task.FromResult(Results.Ok(result));
        }
    }

    internal Task<IResult> GetInstance([FromServices] GetInstanceByIdQuery getInstanceByIdQuery, [FromQuery] int docId)
    {
        var instance = getInstanceByIdQuery.Execute(docId);

        if (instance == null)
        {
            throw new NotFoundException<int>("instance", docId);
        }
        return Task.FromResult(Results.Ok(instance));

        //using (StreamReader r = new StreamReader("Mockdata/data-odsinstances.json"))
        //{
        //    string json = r.ReadToEnd();
        //    ExpandoObject result = JsonConvert.DeserializeObject<ExpandoObject>(json);
        //    return Task.FromResult(Results.Ok(result));
        //}
    }
}
