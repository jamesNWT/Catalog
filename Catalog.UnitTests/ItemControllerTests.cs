using System;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using DnsClient.Protocol;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Catalog.UnitTests
{
    public class ItemControllerTests
    {
        
        private readonly Mock<IItemsRepository> repositoryStub = new();
        private readonly Mock<ILogger<ItemsController>> loggerStub = new();
        private readonly Random rand = new();

        // Test naming convention:
        // UnitOfWork_StateUnderTest_ExpectedBehavior()
        [Fact]
        public async Task GetItemAsync_WithUnexistingItem_ReturnsNotFound()
        {
            // Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
            
            // Act
            var controllerActionResult = await controller.GetItemAsync(Guid.NewGuid());
            
            // Assert
            controllerActionResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetItemAsync_WithExistingItem_ReturnsExpectingItem()
        {
            // arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(expectedItem);
            
            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // act 
            var controllerActionResult = await controller.GetItemAsync(Guid.NewGuid());

            // assert
            controllerActionResult.Value.Should().BeEquivalentTo(
                expectedItem,
                options => options.ComparingByMembers<Item>()); 
        }

        [Fact]
        public async Task GetItemsAsync_WithExistingItem_ReturnsAllItems()
        {
            // arrange
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem()};

            repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);
            
            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // act 
            var actualItems = await controller.GetItemsAsync();

            // assert
            actualItems.Should().BeEquivalentTo(
                expectedItems,
                options => options.ComparingByMembers<Item>()); 
        }

        [Fact]
        public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
        {
            // arrange
            var itemToCreate = new CreateItemDto() {
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000)
            };

             var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // act 
            var result = await controller.CreateItemAsync(itemToCreate);
            
            // assert
            var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
            itemToCreate.Should().BeEquivalentTo(
                createdItem,
                options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers()
            );
            createdItem.Id.Should().NotBeEmpty();
            createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, new TimeSpan(100000000));
        }

        [Fact]
        public async Task UpdateItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            var itemId = existingItem.Id;
            var ItemToUpdate = new UpdateItemDto() {
                Name = Guid.NewGuid().ToString(),
                Price = existingItem.Price + 3
            };
            
            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // act 
            var result = await controller.UpdateItemAsync(itemId, ItemToUpdate);
            
            // assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // arrange
            var existingItem = CreateRandomItem();
            var itemId = existingItem.Id;

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);
            
            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // act 
            var result = await controller.DeleteItemAsync(itemId);
            
            // assert
            result.Should().BeOfType<NoContentResult>();
        }

        private Item CreateRandomItem()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }
    }
}
