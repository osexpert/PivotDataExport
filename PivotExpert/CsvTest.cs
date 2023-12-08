using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert.CsvTest
{
	//	Region,              Country,Item Type,   Sales Channel,Order Priority, Order Date,Order ID, Ship Date,Units Sold, Unit Price,Unit Cost, Total Revenue,Total Cost, Total Profit
	//Australia and Oceania, Palau, Office Supplies,Online,     H,              3/6/2016,517073523,  3/26/2016,2401,651.21,524.96,1563555.21,1260428.96,303126.25
	public class CsvRow
	{
		[Index(0)]
		public string Region { get; set; }
		[Index(1)]
		public string Country { get; set; }
		[Index(2)]
		public string ItemType { get; set; }
		[Index(3)]
		public string SalesChannel { get; set; }
		[Index(4)]
		public string OrderPriority { get; set; }
		[Index(5)]
		public DateTime OrderDate { get; set; }
		[Index(6)]
		public string OrderID { get; set; }
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


}
