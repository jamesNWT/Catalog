using System;

namespace Catalog.Api.Dtos
{
    public record ItemDto
    {
        public Guid Id { get; init; } // init cannot modify property after creation

        public string Name { get; init; }

        public decimal Price { get; init; }

        public DateTimeOffset CreatedDate { get; init; }

    }
}