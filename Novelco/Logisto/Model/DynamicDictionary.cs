using System;


namespace Logisto.Models
{
	public class DynamicDictionary
	{
		public int ID { get; set; }
		public string Display { get; set; }
	}

	public class DynamicTrictionary
	{
		public int ID { get; set; }
		public int TargetId { get; set; }
		public string Display { get; set; }
	}
}