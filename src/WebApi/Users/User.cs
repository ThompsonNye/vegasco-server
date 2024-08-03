using Vegasco.WebApi.Cars;

namespace Vegasco.WebApi.Users;

public class User
{
	public string Id { get; set; } = "";

	public virtual IList<Car> Cars { get; set; } = [];
}
