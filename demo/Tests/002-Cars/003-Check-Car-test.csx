// Test 1: Car becomes available after 3 attempts
tp.Test("Car should be available", () =>
{
    var attempt = tp.GetVariable<int>("CarAvailabilityCheckCount");
    attempt++;
    tp.SetVariable("CarAvailabilityCheckCount", attempt);

    var response = tp.Response;
    var statusCode = response.StatusCode();

    if (attempt >= 3)
    {
        Equal(200, statusCode);
    }
    else
    {
        Fail($"Car not available yet (attempt {attempt})");
    }
});

// Test 2: Car never becomes available (will fail after max attempts)
await tp.Test("Car with special features should exist", async () =>
{
    var attempt = tp.GetVariable<int>("UnavailableCarCheckCount");
    attempt++;
    tp.SetVariable("UnavailableCarCheckCount", attempt);

    var response = tp.Response;
    var statusCode = response.StatusCode();

    // This will always fail - demonstrating max attempts behavior
    Equal(200, statusCode);

    dynamic carData = await response.GetBodyAsExpandoAsync();
    Equal("SpecialFeatures", carData.Brand);
});

// Test 3: Combined scenario - verify newly created car has correct brand
await tp.Test("Newly created car should have correct brand", async () =>
{
    var attempt = tp.GetVariable<int>("CarCreationCheckCount");
    attempt++;
    tp.SetVariable("CarCreationCheckCount", attempt);

    var response = tp.Response;
    var statusCode = response.StatusCode();

    if (attempt >= 2 && (statusCode == 200 || statusCode == 201))
    {
        dynamic carData = await response.GetBodyAsExpandoAsync();
        Equal(200, statusCode);
    }
    else
    {
        Fail($"Car not ready yet (attempt {attempt})");
    }
});
