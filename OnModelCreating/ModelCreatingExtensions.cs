using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NickStrupat.EntityFrameworkCore.OnModelCreating;

public static class ModelCreatingExtensions
{
	public static void OnModelCreatingEntities(this ModelBuilder modelBuilder)
	{
		foreach (var type in modelBuilder.Model.GetEntityTypes())
			MethodInfo
				.MakeGenericMethod(GetEntityType(type))
				.Invoke(null, [modelBuilder]);

		static Type GetEntityType(IMutableEntityType type)
		{
			Type? entityType = null;
			foreach (var @interface in type.ClrType.GetInterfaces())
			{
				if (!@interface.TryGetSingleGenericArgument(typeof(IModelCreating<>), out var genericArgument))
					continue;
				if (genericArgument != type.ClrType)
					throw new EntityImplementsInvalidModelCreatingException(type.ClrType, @interface);
				entityType = type.ClrType;
			}

			return entityType ?? throw new EntityTypeDoesNotImplementModelCreatingException(type.ClrType);
		}
	}

	private sealed class EntityImplementsInvalidModelCreatingException(Type entityClrType, Type @interface) : Exception(
		$"Entity type `{entityClrType}` implements {@interface} but should only implement {nameof(IModelCreating<>)}`1[{entityClrType}]");
	
	private sealed class EntityTypeDoesNotImplementModelCreatingException(Type entityClrType) : Exception(
		$"Entity type `{entityClrType}` does not implement {nameof(IModelCreating<>)}`1[{entityClrType}]");

	private static readonly MethodInfo MethodInfo = typeof(ModelCreatingExtensions)
		.GetMethod(nameof(InvokeOnModelCreating), BindingFlags.NonPublic | BindingFlags.Static)!
		.GetGenericMethodDefinition();
	
	private static void InvokeOnModelCreating<T>(ModelBuilder mb) where T : class, IModelCreating<T> =>
		T.OnModelCreating(mb.Entity<T>());
}