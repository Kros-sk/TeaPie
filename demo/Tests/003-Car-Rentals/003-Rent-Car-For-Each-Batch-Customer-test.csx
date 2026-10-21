var customerIds = tp.GetVariable<List<long>>("NewCustomerIdsBatch");

for (var i = 1; i <= customerIds.Count; i++)
{
    var name = $"RentCarForBatchCustomer{i}";
    var expectedCustomerId = customerIds[i - 1];

    await tp.Test($"Rental #{i} for a batch customer should be created with status 201.", async () =>
    {
        Equal(201, tp.Responses[name].StatusCode());

        // Each iteration's rental should reference its own customer from the previous loop,
        // not a copy of one.
        dynamic responseJson = await tp.Responses[name].GetBodyAsExpandoAsync();
        Equal(expectedCustomerId, (long)responseJson.customerId);
    });
}
