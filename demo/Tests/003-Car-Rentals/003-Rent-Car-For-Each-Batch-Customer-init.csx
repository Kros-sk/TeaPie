// Loop feeds loop: pair each already-created batch customer (from
// 001-Customers/002-Add-Customers-Batch, run earlier in the same collection) with a freshly
// generated car, so the loop in the request file below can rent one car per customer.
#load "$teapie/Definitions/GenerateNewCar.csx"

var customerIds = tp.GetVariable<List<long>>("NewCustomerIdsBatch");
var cars = GenerateCars(customerIds.Count);

var bookings = customerIds
    .Select((customerId, i) => new { CustomerId = customerId, Car = cars[i] })
    .ToList();

tp.SetVariable("BatchCustomerBookings", bookings, "rentals");
