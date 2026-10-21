var count = tp.GetVariable<int>("NewCarsBatchCount");
var brands = tp.GetVariable<List<string>>("NewCarsBatchBrands");

for (var i = 1; i <= count; i++)
{
    var name = $"AddCarBatch{i}";
    var expectedBrand = brands[i - 1];

    await tp.Test($"Car #{i} added by the loop should be created with status 201.", async () =>
    {
        var statusCode = tp.Responses[name].StatusCode();
        Equal(201, statusCode);

        // Each iteration's response should reflect that iteration's own car, not a copy of one.
        dynamic responseJson = await tp.Responses[name].GetBodyAsExpandoAsync();
        Equal(expectedBrand, responseJson.brand);
    });
}
