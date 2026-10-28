// Demonstrates nested templating loops: for each customer, a nested loop rents every car
// on that customer's wishlist - one POST per (customer, car) combination.
#load "$teapie/Definitions/GenerateNewCar.csx"

var aliceCars = GenerateCars(2);
var bobCars = GenerateCars(1);

tp.SetVariable("CustomersWithCarWishlist", new[]
{
    new { Name = "Alice Johnson", Cars = aliceCars },
    new { Name = "Bob Smith", Cars = bobCars }
}, "rentals");

tp.SetVariable("WishlistCustomerNames", new List<string> { "Alice Johnson", "Bob Smith" }, "rentals");
tp.SetVariable("WishlistCarIdsPerCustomer", new List<List<long>>
{
    aliceCars.Select(c => c.Id).ToList(),
    bobCars.Select(c => c.Id).ToList()
}, "rentals");
