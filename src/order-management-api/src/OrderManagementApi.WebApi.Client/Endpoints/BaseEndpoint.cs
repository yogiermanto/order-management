﻿using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable RouteTemplates.RouteTokenNotResolved

namespace OrderManagementApi.WebApi.Client.Endpoints;

[Route("api/[namespace]")]
public abstract class BaseEndpoint<TReq, TRes> : EndpointBaseAsync.WithRequest<TReq>.WithActionResult<TRes>
{
}

[Route("api/[namespace]")]
public abstract class BaseEndpoint<TRes> : EndpointBaseAsync.WithoutRequest.WithActionResult<TRes>
{
}


[Route("api/[namespace]")]
public abstract class BaseEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult
{
}