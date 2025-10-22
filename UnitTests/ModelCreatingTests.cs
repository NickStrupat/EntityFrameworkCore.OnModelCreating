using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NickStrupat.EntityFrameworkCore.OnModelCreating;
using Xunit;

namespace UnitTests;

public class ModelCreatingTests : IDisposable
{
	private readonly Context context;
	
	public ModelCreatingTests()
	{
		var options = new DbContextOptionsBuilder().UseInMemoryDatabase("test").Options;
		context = new Context(options);
	}
	
	public void Dispose()
	{
		context.Dispose();
	}

	[Fact]
	public void NotTriggeringModelCreation_DoesNotCallOnModelCreating()
	{
		Assert.Equal(0, Entity.initialized);
	}
	
	[Fact]
	public void TriggeringModelCreation_CallsOnModelCreating()
	{
		_ = context.Model; // Trigger model creation
		
		Assert.Equal(1, Entity.initialized);
	}
	
	sealed class Entity : IModelCreating<Entity>
	{
		public Int64 Id { get; private set; }
		
		public static Int32 initialized;
		
		static void IModelCreating<Entity>.OnModelCreating(EntityTypeBuilder<Entity> entityTypeBuilder)
		{
			initialized++;
		}
	}
	
	sealed class Context(DbContextOptions options) : DbContext(options)
	{
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Entity>();
			modelBuilder.OnModelCreatingEntities();
		}
	}
}