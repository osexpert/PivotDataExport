using CsvHelper.Configuration.Attributes;

namespace Examples;

public class CsvRow
{
	[Index(0)]
	public string Region { get; set; } = null!;
	[Index(1)]
	public string Country { get; set; } = null!;
	[Index(2)]
	public string ItemType { get; set; } = null!;
	[Index(3)]
	public string SalesChannel { get; set; } = null!;
	[Index(4)]
	public string OrderPriority { get; set; } = null!;
	[Index(5)]
	public DateTime OrderDate { get; set; }
	[Index(6)]
	public string OrderID { get; set; } = null!;
	[Index(7)]
	public DateTime ShipDate { get; set; }
	[Index(8)]
	public long UnitsSold { get; set; }
	[Index(9)]
	public double UnitPrice { get; set; }
	[Index(10)]
	public double UnitCost { get; set; }
	[Index(11)]
	public double TotalRevenue { get; set; }
	[Index(12)]
	public double TotalCost { get; set; }
	[Index(13)]
	public double TotalProfit { get; set; }
}
