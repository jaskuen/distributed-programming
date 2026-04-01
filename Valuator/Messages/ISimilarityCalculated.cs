namespace Valuator.Messages;

public interface ISimilarityCalculated
{
    public string Id { get; set; }
    public double Similarity { get; set; }
}