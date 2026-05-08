namespace Taskeen.Application.DTOs;

public class OwnerCalendarDTO
{
    public OwnerCalendarDTO(string unitCode, DateTime startDate, DateTime endDate, decimal baseAmount)
    {
        UnitCode = unitCode;
        StartDate = startDate;
        EndDate = endDate;
        BaseAmount = baseAmount;
    }

    public string UnitCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BaseAmount { get; set; }
}
