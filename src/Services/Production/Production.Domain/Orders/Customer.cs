using Blocks.Entitities;

namespace Production.Domain.Orders;

using OrdersList = List<Order>;
public class Customer : Entity
{
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public OrdersList Orders { get; } = new();
}
