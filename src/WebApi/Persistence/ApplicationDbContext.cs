using Microsoft.EntityFrameworkCore;
using Vegasco.WebApi.Cars;
using Vegasco.WebApi.Common;
using Vegasco.WebApi.Users;

namespace Vegasco.WebApi.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<Car> Cars { get; set; }

	public DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(IWebApiMarker).Assembly);
	}
}
