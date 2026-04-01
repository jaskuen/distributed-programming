namespace RankCalculator.Messages;

public interface IRankCalculated
{
    public string Id { get; set; }
    public double Rank { get; set; }
}