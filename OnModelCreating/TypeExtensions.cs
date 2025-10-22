using System;
using System.Diagnostics.CodeAnalysis;

namespace NickStrupat.EntityFrameworkCore.OnModelCreating;

public static class TypeExtensions
{
	public static Boolean TryGetSingleGenericArgument(this Type type, Type genericType, [MaybeNullWhen(false)] out Type genericArgument)
	{
		if (!genericType.IsGenericTypeDefinition)
			throw new ArgumentException($"The type `{genericType}` must be a generic type definition.", nameof(genericType));
		
		if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType && type.GenericTypeArguments is [var t])
		{
			genericArgument = t;
			return true;
		}

		genericArgument = null;
		return false;
	}
}