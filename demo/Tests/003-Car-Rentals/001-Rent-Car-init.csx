tp.SetVariable("RentalDays", 14);

// Register custom function, for generating car return date.
tp.RegisterFunction("$carReturnDate", (int days) =>
{
    return DateTime.Now.AddDays(days).ToString("yyyy-MM-dd");
});
