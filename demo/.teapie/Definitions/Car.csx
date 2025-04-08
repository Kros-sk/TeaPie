public class Car
{
    public long Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string EngineType { get; set; }
    public string TransmissionType { get; set; }
    public int PeopleCapacity { get; set; }
    public string Color { get; set; }
    public int Year { get; set; }
    public double DrivenKilometres { get; set; }
    public string Description { get; set; }

    public override string ToString() => $"{Brand} {Model}, {Year}";
}
