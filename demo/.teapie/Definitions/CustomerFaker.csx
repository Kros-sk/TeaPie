// Use 'nuget' directives to download NuGet packages with their dependencies.
#nuget "AutoBogus, 2.13.1"
#nuget "Bogus, 31.0.3"

// Reference script with class definition
#load "Customer.csx"

using AutoBogus;

public class CustomerFaker : AutoFaker<Customer>
{
    public CustomerFaker()
    {
        RuleFor(c => c.Id, f => f.Random.Long(101, 200));
        RuleFor(c => c.FirstName, f => f.Name.FirstName());
        RuleFor(c => c.LastName, f => f.Name.LastName());
        RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName));
    }
}
