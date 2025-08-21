public class CarRent
{
    public long Id { get; set; }
    public long CarId { get; set; }
    public long CustomerId { get; set; }
    public string RentalStart { get; set; }
    public string RentalEnd { get; set; }
    public decimal Price { get; set; }
    public string Notes { get; set; }
    public short NumberOfDrivers { get; set; }
}
