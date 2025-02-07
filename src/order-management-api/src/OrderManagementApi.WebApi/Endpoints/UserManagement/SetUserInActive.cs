﻿using OrderManagementApi.Domain.Entities;
using OrderManagementApi.Shared.Abstractions.Databases;
using OrderManagementApi.Shared.Abstractions.Enums;
using OrderManagementApi.Shared.Abstractions.Models;
using OrderManagementApi.WebApi.Common;
using OrderManagementApi.WebApi.Endpoints.UserManagement.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace OrderManagementApi.WebApi.Endpoints.UserManagement;

public class SetUserInActive : BaseEndpointWithoutResponse<SetUserInActiveRequest>
{
    private readonly IDbContext _dbContext;

    public SetUserInActive(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPatch("users/{userId}/inactive")]
    [Authorize]
    [RequiredScope(typeof(UserManagementScope))]
    [SwaggerOperation(
        Summary = "Set user inactive",
        Description = "",
        OperationId = "UserManagement.GetById",
        Tags = new[] { "UserManagement" })
    ]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync([FromRoute] SetUserInActiveRequest request,
        CancellationToken cancellationToken = new())
    {
        var user = await _dbContext.Set<User>()
            .Where(e => e.UserId == request.UserId)
            .Select(e => new User
            {
                UserId = e.UserId,
                StatusRecord = e.StatusRecord
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return BadRequest(Error.Create("Data not found"));

        if (user.StatusRecord == StatusRecord.InActive)
            return BadRequest(Error.Create("User already inactive"));

        _dbContext.AttachEntity(user);

        user.StatusRecord = StatusRecord.InActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}