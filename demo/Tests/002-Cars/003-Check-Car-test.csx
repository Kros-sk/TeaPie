// Test 1: Car becomes available after 3 attempts
tp.Test("Car should be available", () =>
{
    var checkCount = tp.GetVariable<int>("CarAvailabilityCheckCount");
    checkCount++;
    tp.SetVariable("CarAvailabilityCheckCount", checkCount);

    var response = tp.Response;
    var statusCode = response.StatusCode();

    if (checkCount >= 3)
    {
        tp.Logger.LogInformation($"Car is now available after {checkCount} attempts!");
        Equal(200, statusCode);
    }
    else
    {
        tp.Logger.LogInformation($"Car not available yet (attempt {checkCount})");
        throw new Exception($"Car not available yet (attempt {checkCount})");
    }
});

// Test 2: Car never becomes available (will fail after max attempts)
await tp.Test("Car with special features should exist", async () =>
{
    var checkCount = tp.GetVariable<int>("UnavailableCarCheckCount");
    checkCount++;
    tp.SetVariable("UnavailableCarCheckCount", checkCount);

    var response = tp.Response;
    var statusCode = response.StatusCode();

    tp.Logger.LogInformation($"Checking for unavailable car (attempt {checkCount}), status: {statusCode}");

    // This will always fail - demonstrating max attempts behavior
    Equal(200, statusCode);

    dynamic carData = await response.GetBodyAsExpandoAsync();
    Equal("SpecialFeatures", carData.Brand);
});

// Test 3: Combined scenario - verify newly created car has correct brand
await tp.Test("Newly created car should have correct brand", async () =>
{
    var checkCount = tp.GetVariable<int>("CarCreationCheckCount");
    checkCount++;
    tp.SetVariable("CarCreationCheckCount", checkCount);

    var response = tp.Response;
    var statusCode = response.StatusCode();

    tp.Logger.LogInformation($"Verifying car creation (attempt {checkCount}), status: {statusCode}");

    if (checkCount >= 2 && (statusCode == 200 || statusCode == 201))
    {
        dynamic carData = await response.GetBodyAsExpandoAsync();
        tp.Logger.LogInformation($"Car found with brand: {carData.Brand}");
        Equal("Audi", carData.Brand);
    }
    else
    {
        tp.Logger.LogInformation($"Car not ready yet (attempt {checkCount})");
        throw new Exception($"Car not ready yet (attempt {checkCount})");
    }
});
