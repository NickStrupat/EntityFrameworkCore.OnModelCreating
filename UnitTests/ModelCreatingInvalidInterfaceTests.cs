using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NickStrupat.EntityFrameworkCore.OnModelCreating;
using Xunit;

namespace UnitTests;

public class ModelCreatingInvalidInterfaceTests : IDisposable
{
	private readonly Context context;

	public ModelCreatingInvalidInterfaceTests()
	{
		var options = new DbContextOptionsBuilder().UseInMemoryDatabase("test").Options;
		context = new Context(options);
	}

	public void Dispose()
	{
		context.Dispose();
	}	
	
	[Fact]
	public void TriggeringModelCreation_WithInvalidInterface_ThrowsException()
	{
		var exception = Assert.ThrowsAny<Exception>(() => _ = context.Model);
		
		Assert.Contains($"Entity type {typeof(InvalidEntity)} must only implement {nameof(IModelCreating<>)}`1[{typeof(InvalidEntity)}].".AsSpan(), exception.Message.AsSpan());
	}
	
	sealed class InvalidEntity : IModelCreating<ValidEntity>
	{
		public Int64 Id { get; private set; }
		
		static void IModelCreating<ValidEntity>.OnModelCreating(EntityTypeBuilder<ValidEntity> entityTypeBuilder)
		{
		}
	}
	
	sealed class ValidEntity : IModelCreating<ValidEntity>
	{
		public Int64 Id { get; private set; }
		
		public static Int32 initialized;
		
		static void IModelCreating<ValidEntity>.OnModelCreating(EntityTypeBuilder<ValidEntity> entityTypeBuilder)
		{
			initialized++;
		}
	}
	
	sealed class Context(DbContextOptions options) : DbContext(options)
	{
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<InvalidEntity>();
			modelBuilder.Entity<ValidEntity>();
			modelBuilder.OnModelCreatingEntities();
		}
	}
}