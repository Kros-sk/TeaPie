var count = tp.GetVariable<int>("NewCustomersBatchCount");
var names = tp.GetVariable<List<string>>("NewCustomersBatchNames");
var createdCustomerIds = new List<long>();

for (var i = 1; i <= count; i++)
{
    var name = $"AddCustomerBatch{i}";
    var expectedName = names[i - 1];

    await tp.Test($"Customer #{i} added by the loop should be created with status 201.", async () =>
    {
        var statusCode = tp.Responses[name].StatusCode();
        Equal(201, statusCode);

        // Each iteration's response should reflect that iteration's own customer, not a copy of one.
        dynamic responseJson = await tp.Responses[name].GetBodyAsExpandoAsync();
        Equal(expectedName, $"{responseJson.firstName} {responseJson.lastName}");

        createdCustomerIds.Add((long)responseJson.id);
    });
}

// Captured for 003-Car-Rentals/003-Rent-Car-For-Each-Batch-Customer, which loops over this
// collection to rent a car for every customer created here - a loop feeding another loop.
tp.SetVariable("NewCustomerIdsBatch", createdCustomerIds, "customers", "ids");
