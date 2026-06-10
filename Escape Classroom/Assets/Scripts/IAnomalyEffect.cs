/// <summary>
/// Implement this interface on any GameObject that acts as an anomaly.
/// ClassroomAnomalyRandomizer calls Activate() / Deactivate() each round.
/// </summary>
public interface IAnomalyEffect
{
    /// <summary>Called when this anomaly is chosen for the current round.</summary>
    void Activate();

    /// <summary>Called when this anomaly should return to its normal state.</summary>
    void Deactivate();
}
