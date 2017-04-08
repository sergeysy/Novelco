using System;

namespace Logisto.Models
{
	public class IdPair
	{
		public int Id { get; set; }
		public int Id2 { get; set; }
	}

	public class IdTuple
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string EnName { get; set; }
		public double Value { get; set; }
		public double Value2 { get; set; }
		public string TaxRate { get; set; }
	}
}
