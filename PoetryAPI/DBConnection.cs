using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace PoetryAPI
{
	public class Database
	{
		public static DBConnection DB;
	}

	public class DBConnection
	{
		public string Server { get; set; }
		public string DatabaseName { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public MySqlConnection Connection { get; set; }

		private static DBConnection _instance = null;
		public static DBConnection Instance()
		{
			if (_instance == null)
			{
				_instance = new DBConnection();
			}
			return _instance;
		}

		public bool IsConnect()
		{
			try
			{
				if (Connection == null)
				{
					if (string.IsNullOrEmpty(DatabaseName))
						return false;

					string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}; protocol=Unix",
						Server, DatabaseName, Username, Password);
					Connection = new MySqlConnection(connstring);
					Connection.Open();
					return true;
				}

				if (Connection.State == System.Data.ConnectionState.Closed)
					return false;




				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Close()
		{
			Connection.Close();
		}

	}
}
