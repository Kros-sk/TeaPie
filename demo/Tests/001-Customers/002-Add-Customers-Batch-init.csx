// Demonstrates the templating loop feature: a request block can be repeated once per
// item in a collection, producing N independent requests instead of one hand-written request.
#load "$teapie/Definitions/GenerateNewCustomer.csx"

// Generate a batch of fake customers - each one becomes its own POST request in the loop below.
// Their ids are captured in the post-response script so a later test case can rent a car
// for each of them via a second, chained loop (see 003-Car-Rentals/003-Rent-Car-For-Each-Batch-Customer).
var newCustomers = GenerateCustomers(3);

tp.SetVariable("NewCustomersBatch", newCustomers, "customers");
tp.SetVariable("NewCustomersBatchCount", newCustomers.Count, "customers");

tp.SetVariable("NewCustomersBatchNames", newCustomers.Select(c => $"{c.FirstName} {c.LastName}").ToList(), "customers");
