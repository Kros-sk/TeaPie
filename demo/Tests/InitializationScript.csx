public void SetVariables()
{
    // Variables set this way are stored in the 'CollectionVariables' scope.
    tp.SetVariable("ApiBaseUrl", "http://localhost:3001");
    tp.SetVariable("ApiCustomersSection", "/customers");
    tp.SetVariable("ApiCarsSection", "/cars");
    tp.SetVariable("ApiCarRentalSection", "/rental");
}
