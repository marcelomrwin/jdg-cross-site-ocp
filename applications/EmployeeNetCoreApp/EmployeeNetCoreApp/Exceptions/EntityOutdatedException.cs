using System;
using System.Data.Common;

namespace EmployeeNetCoreApp.Exceptions
{
    [Serializable]
    public class EntityOutdatedException: Exception
	{
		public string UUID { get; }
        public string? UpdatedBy { get; }
        public DateTime? UpdatedDate { get; }
        public int LocalVersion { get; }
        public int LastVersion { get; }

        public EntityOutdatedException(string UUID, string? UpdatedBy, DateTime? UpdatedDate, int LocalVersion, int LastVersion)
		{
			this.UUID = UUID;
			this.UpdatedBy = UpdatedBy;
			this.UpdatedDate = UpdatedDate;
			this.LocalVersion = LocalVersion;
			this.LastVersion = LastVersion;
		}

        public new string Message()
        {
            return string.Format("The local version of employee {0} is out of date. The most up-to-date version is {1}, updated by {2} on {3} and local version is {4}. Please update your local version", UUID, LastVersion, UpdatedBy, UpdatedDate, LocalVersion) ;
        }
    }
}

