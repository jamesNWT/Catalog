using System;

namespace Catalog.Api.Entities
{
/** Record Types: 
    Immutable objects
    With-expression support
    Value-based equality: instances will be equal only if all the properties of the instances are the same
    */
    public record Item
    {
        public Guid Id { get; init; } // init cannot modify property after creation

        public string Name { get; init; }

        public decimal Price { get; init; }

        public DateTimeOffset CreatedDate { get; init; }

    }
}