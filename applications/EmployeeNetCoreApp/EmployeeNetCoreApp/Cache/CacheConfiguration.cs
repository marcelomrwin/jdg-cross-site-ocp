using System;
namespace EmployeeNetCoreApp.Cache
{
	public class CacheConfiguration
	{
		public string Protocol { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public string Cache { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
    }
}

