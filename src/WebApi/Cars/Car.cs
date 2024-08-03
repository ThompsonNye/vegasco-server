using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vegasco.WebApi.Users;

namespace Vegasco.WebApi.Cars;

public class Car
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public string Name { get; set; } = "";

	public string UserId { get; set; } = "";

	public virtual User User { get; set; } = null!;
}

public class CarTableConfiguration : IEntityTypeConfiguration<Car>
{
	public const int NameMaxLength = 50;

	public void Configure(EntityTypeBuilder<Car> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(NameMaxLength);

		builder.Property(x => x.UserId)
			.IsRequired();

		builder.HasOne(x => x.User)
			.WithMany(x => x.Cars);
	}
}
