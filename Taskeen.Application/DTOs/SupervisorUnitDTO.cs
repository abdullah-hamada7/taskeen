namespace Taskeen.Application.DTOs;

public class SupervisorUnitDTO
{
    public SupervisorUnitDTO(string unitCode, int totalBeds, List<string> occupants)
    {
        UnitCode = unitCode;
        TotalBeds = totalBeds;
        Occupants = occupants;
    }

    public string UnitCode { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public List<string> Occupants { get; set; } = new List<string>();
}
