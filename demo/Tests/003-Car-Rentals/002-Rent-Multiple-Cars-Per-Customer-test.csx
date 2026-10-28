var customerNames = tp.GetVariable<List<string>>("WishlistCustomerNames");
var carIdsPerCustomer = tp.GetVariable<List<List<long>>>("WishlistCarIdsPerCustomer");

for (var i = 1; i <= customerNames.Count; i++)
{
    var carIds = carIdsPerCustomer[i - 1];

    for (var j = 1; j <= carIds.Count; j++)
    {
        var name = $"RentCar{i}_{j}";
        var expectedCarId = carIds[j - 1];

        await tp.Test(
            $"Rental {i}.{j} for {customerNames[i - 1]} should be created with status 201.",
            async () =>
            {
                Equal(201, tp.Responses[name].StatusCode());

                // Each (customer, car) pair produced by the nested loop should reflect its own
                // car, not the same one repeated across iterations.
                dynamic responseJson = await tp.Responses[name].GetBodyAsExpandoAsync();
                Equal(expectedCarId, (long)responseJson.carId);
            });
    }
}
