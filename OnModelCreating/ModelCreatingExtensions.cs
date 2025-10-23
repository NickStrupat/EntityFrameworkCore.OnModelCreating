using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NickStrupat.EntityFrameworkCore.OnModelCreating;

public static class ModelCreatingExtensions
{
	/// Invokes the static <c>OnModelCreating</c> method on all entity types that implement <see cref="IModelCreating{T}"/>.
	/// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
	/// <exception cref="EntityImplementsInvalidModelCreatingException">Thrown when an entity type implements
	/// <see cref="IModelCreating{T}"/> with a generic argument that does not match the entity type itself.</exception>
	public static void OnModelCreatingEntities(this ModelBuilder modelBuilder)
	{
		Object?[] parameters = [modelBuilder];
		foreach (var type in modelBuilder.Model.GetEntityTypes())
		{
			if (GetEntityType(type) is {} entityType)
				MethodInfo.MakeGenericMethod(entityType).Invoke(null, parameters);
		}

		static Type? GetEntityType(IMutableEntityType type)
		{
			Type? entityType = null;
			List<Type>? invalidModelCreatingInterfaces = null;
			foreach (var @interface in type.ClrType.GetInterfaces())
			{
				if (!@interface.IsGenericType || @interface.GetGenericTypeDefinition() != typeof(IModelCreating<>))
					continue;
				var genericArgument = @interface.GenericTypeArguments.Single();
				if (genericArgument != type.ClrType)
					(invalidModelCreatingInterfaces ??= new(1)).Add(@interface);
				entityType = type.ClrType;
			}
			if (invalidModelCreatingInterfaces is not null)
				throw new EntityImplementsInvalidModelCreatingException(type.ClrType, invalidModelCreatingInterfaces);
			return entityType;
		}
	}

	private static readonly MethodInfo MethodInfo = typeof(ModelCreatingExtensions)
		.GetMethod(nameof(InvokeOnModelCreating), BindingFlags.NonPublic | BindingFlags.Static)!
		.GetGenericMethodDefinition();
	
	private static void InvokeOnModelCreating<T>(ModelBuilder mb) where T : class, IModelCreating<T> =>
		T.OnModelCreating(mb.Entity<T>());
}

public sealed class EntityImplementsInvalidModelCreatingException(Type entityClrType, List<Type> @interface)
	: Exception($"Entity type {entityClrType} must only implement {nameof(IModelCreating<>)}`1[{entityClrType}].")
{
	public Type EntityClrType => entityClrType;
	public IReadOnlyList<Type> InvalidInterfaces => @interface;
}