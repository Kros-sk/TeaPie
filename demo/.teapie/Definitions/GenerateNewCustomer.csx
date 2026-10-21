#load "CustomerFaker.csx"

using AutoBogus;

public Customer GenerateCustomer()
{
    var faker = new CustomerFaker();
    return faker.Generate();
}

public List<Customer> GenerateCustomers(int count)
{
    var faker = new CustomerFaker();
    return faker.Generate(count);
}
