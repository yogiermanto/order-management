﻿using System.Text;
using OrderManagementApi.Shared.Abstractions.Databases;
using OrderManagementApi.Shared.Abstractions.Files;
using OrderManagementApi.WebApi.Dto;
using OrderManagementApi.WebApi.Endpoints.FileRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace OrderManagementApi.IntegrationTests.Endpoints.FileRepository;

[Collection(nameof(FileRepositoryFixture))]
public class UploadFileTests
{
    private readonly FileRepositoryFixture _serviceFixture;

    public UploadFileTests(FileRepositoryFixture serviceFixture)
    {
        _serviceFixture = serviceFixture;
    }

    [Fact]
    public async Task UploadFile_ShouldBe_Correct()
    {
        // Arrange
        using var scope = _serviceFixture.ServiceProvider.CreateScope();

        //Act
        var content = "Hello World from a Fake File";
        var fileName = "test.pdf";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        //create FormFile with desired data
        IFormFile file = new FormFile(stream, 0, stream.Length, "id_from_form", fileName);

        var request = new UploadFileRequest
        {
            File = file,
            Source = "test"
        };

        var uploadFile = new UploadFile(
            scope.ServiceProvider.GetRequiredService<IDbContext>(),
            scope.ServiceProvider.GetRequiredService<IFileService>());

        var result = await uploadFile.HandleAsync(request, CancellationToken.None);
        // Assert the expected results
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<UploadFileDto>();

        var data = await scope.ServiceProvider.GetRequiredService<IDbContext>().Set<Domain.Entities.FileRepository>()
            .Where(e => e.FileRepositoryId == result.Value.FileId).FirstOrDefaultAsync(CancellationToken.None);

        data.ShouldNotBeNull();
    }
}