namespace Taskeen.Application.DTOs;

public class AdminReportDTO
{
    public AdminReportDTO(string unitCode, decimal baseAmount, decimal taxAmount, decimal totalAmount)
    {
        UnitCode = unitCode;
        BaseAmount = baseAmount;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
    }

    public string UnitCode { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
