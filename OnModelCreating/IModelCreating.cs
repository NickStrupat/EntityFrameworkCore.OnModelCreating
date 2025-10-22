using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NickStrupat.EntityFrameworkCore.OnModelCreating;

public interface IModelCreating<T> where T : class, IModelCreating<T>
{
	static abstract void OnModelCreating(EntityTypeBuilder<T> entityTypeBuilder);
}
