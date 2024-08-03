namespace Vegasco.WebApi.Cars;

public class Car
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public string Name { get; set; } = "";

	public Guid UserId { get; set; }
}
