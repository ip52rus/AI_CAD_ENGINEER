using AI_CAD_ENGINEER.Engineering.Decision;

namespace AI_CAD_ENGINEER.Engineering.Models;

public class ViewCandidate
{
    public StandardViewOrientation Orientation { get; set; }

    public string OrientationName { get; set; } = string.Empty;

    public ViewStatistics Statistics { get; set; } = new();

    public double Score { get; set; }
}