namespace ElAd2024.Models.Database;
public class TestValueBaseTable<T> : TestChildBaseTable
{
    public T? Value { get; set; }
}

public class TestChildBaseTable
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public Test Test { get; set; } = default!;
}