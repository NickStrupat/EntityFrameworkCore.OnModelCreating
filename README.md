# EntityFrameworkCore.OnModelCreating

An extension to EF Core that enables adding `OnModelCreating` to each entity so fluent model configuration can be kept in the entity class.

## Overview

This library allows you to define Entity Framework Core model configuration directly within your entity classes, rather than centralizing all configuration in `DbContext.OnModelCreating`. This promotes better encapsulation and keeps entity configuration close to the entity definition.

## Features

- Define model configuration within entity classes using a static abstract interface method
- Automatically discovers and invokes configuration for all registered entities
- Type-safe configuration using generic constraints
- Validates that entities implement the interface correctly
- Works with EF Core 9.0+

## Installation

```bash
dotnet add package NickStrupat.EntityFrameworkCore.OnModelCreating
```

## Usage

### 1. Implement `IModelCreating<T>` on your entity

```csharp
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NickStrupat.EntityFrameworkCore.OnModelCreating;

public class Product : IModelCreating<Product>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    static void IModelCreating<Product>.OnModelCreating(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}
```

### 2. Call `OnModelCreatingEntities()` in your DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using NickStrupat.EntityFrameworkCore.OnModelCreating;

public class MyDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Invoke configuration from all entities implementing IModelCreating<T>
        modelBuilder.OnModelCreatingEntities();
    }
}
```

## How It Works

1. Your entity class implements `IModelCreating<T>` where `T` is the entity type itself
2. The interface requires a static abstract method `OnModelCreating` that receives an `EntityTypeBuilder<T>`
3. When you call `modelBuilder.OnModelCreatingEntities()`, the library:
   - Discovers all registered entity types in the model
   - Checks if each entity implements `IModelCreating<T>`
   - Invokes the static `OnModelCreating` method for each implementing entity
   - Validates that the generic type argument matches the entity type

## Requirements

- .NET 9.0 or later (uses static abstract interface members)
- Entity Framework Core 9.0 or later

## Benefits

- **Encapsulation**: Keep entity configuration with the entity definition
- **Maintainability**: Changes to an entity and its configuration are in the same file
- **Organization**: Avoid massive `OnModelCreating` methods in DbContext
- **Type Safety**: Generic constraints ensure configuration is applied to the correct entity
- **Discoverability**: Configuration is easy to find when working on an entity

## Error Handling

The library validates that entities implement `IModelCreating<T>` correctly. If an entity implements the interface with a different type argument than itself, an `EntityImplementsInvalidModelCreatingException` is thrown with details about the invalid implementation.
