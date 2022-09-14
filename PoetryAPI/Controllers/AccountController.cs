using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace PoetryAPI.Controllers
{
    //[Route("api/Login")]
    [ApiController]
    public class AccountController : Controller
    {
        [HttpPost]
		[Route("api/Login")]
        public string Post(User user)
        {
            return UserManager.GetUser(user.Username, user.Password);
        }
		
		[HttpPost]
		[Route("api/Register")]
		public bool PostReg(User user)
        {
			return UserManager.CreateUser(user.Username, user.Password);
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserManager
    {
        public static string GetUser(string username, string password)
        {
			string name = "", score = "", games = "", created = "", achievements = "", password_check = "";
			if (Database.DB.IsConnect())
			{
				//suppose col0 and col1 are defined as VARCHAR in the DB
				string query = "SELECT * FROM Users WHERE `Username` = '" + username + "'";
				using (MySqlCommand cmd = new MySqlCommand(query, Database.DB.Connection))
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{

							if (!reader.HasRows)
								return "{\"success\": \"false\"}";

							if (!reader.IsDBNull(2))
								password_check = reader.GetString(2);

							if (password_check != password)
								return "{\"success\": \"false\"}";

							if (!reader.IsDBNull(3))
								name = reader.GetString(3);
							if (!reader.IsDBNull(4))
								score = reader.GetInt32(4).ToString();
							if (!reader.IsDBNull(5))
								games = reader.GetInt32(5).ToString();
							if (!reader.IsDBNull(6))
								created = reader.GetDateTime(6).ToString();
							if (!reader.IsDBNull(7))
								achievements = reader.GetInt32(7).ToString();
						}
						
						if (name == "" && score == "" && games == "" && created == "" && achievements == "")
							return "{\"success\": \"false\"}";

						return $"{{\"user\": {{\"name\": \"{name}\", \"score\": \"{score}\", \"games\": \"{games}\", \"created\": \"{created}\", \"achievements\": \"{achievements}\", }}, \"success\": \"true\"}}";
					}
				}
			}
			else throw new Exception("DB is not connected");

			return "{\"user\": {\"id\": 1 }}";
        }
		public static string GetUserNoPass(string username)
		{
			string name = "", score = "", games = "", created = "", achievements = "";
			if (Database.DB.IsConnect())
			{
				//suppose col0 and col1 are defined as VARCHAR in the DB
				string query = "SELECT * FROM Users WHERE `Username` = '" + username + "'";
				using (MySqlCommand cmd = new MySqlCommand(query, Database.DB.Connection))
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{

							if (!reader.HasRows)
								return "{\"success\": \"false\"}";

							if (!reader.IsDBNull(3))
								name = reader.GetString(3);
							if (!reader.IsDBNull(4))
								score = reader.GetInt32(4).ToString();
							if (!reader.IsDBNull(5))
								games = reader.GetInt32(5).ToString();
							if (!reader.IsDBNull(6))
								created = reader.GetDateTime(6).ToString();
							if (!reader.IsDBNull(7))
								achievements = reader.GetInt32(7).ToString();
						}

						if (name == "" && score == "" && games == "" && created == "" && achievements == "")
							return "{\"success\": \"false\"}";

						return $"{{\"user\": {{\"name\": \"{name}\", \"score\": \"{score}\", \"games\": \"{games}\", \"created\": \"{created}\", \"achievements\": \"{achievements}\", }}, \"success\": \"true\"}}";
					}
				}
			}
			else throw new Exception("DB is not connected");

			return "{\"user\": {\"id\": 1 }}";
		}
		public static bool CheckUser(string username)
		{
			string name = "", score = "", games = "", created = "", achievements = "";
			if (Database.DB.IsConnect())
			{
				//suppose col0 and col1 are defined as VARCHAR in the DB
				string query = "SELECT * FROM Users WHERE `Username` = '" + username + "'";
				using (MySqlCommand cmd = new MySqlCommand(query, Database.DB.Connection))
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{

							if (!reader.HasRows)
								return false;

							if (!reader.IsDBNull(3))
								name = reader.GetString(3);
							if (!reader.IsDBNull(4))
								score = reader.GetInt32(4).ToString();
							if (!reader.IsDBNull(5))
								games = reader.GetInt32(5).ToString();
							if (!reader.IsDBNull(6))
								created = reader.GetDateTime(6).ToString();
							if (!reader.IsDBNull(7))
								achievements = reader.GetInt32(7).ToString();
						}

						if (name == "" && score == "" && games == "" && created == "" && achievements == "")
							return false;

						return true;
					}
				}
			}
			else throw new Exception("DB is not connected");

			return false;
		}

		public static bool CreateUser(string username, string password)
        {
            //try
            {
				string today = DateTime.Today.Year.ToString() +
					(DateTime.Today.Month.ToString().Length == 1 ? "0" + DateTime.Today.Month.ToString() : DateTime.Today.Month.ToString()) +
					(DateTime.Today.Day.ToString().Length == 1 ? "0" + DateTime.Today.Day.ToString() : DateTime.Today.Day.ToString());

				if (CheckUser(username))
                {
					return false;
                }

				if (Database.DB.IsConnect())
				{
					//suppose col0 and col1 are defined as VARCHAR in the DB
					
					string query = $"INSERT Users (Username, Password, Name, TotalScore, GamesCount, Created, Achievements) VALUES ('{username}', '{password}', '{username}', 0, 0, '{today}', 0)";
					using (MySqlCommand cmd = new MySqlCommand(query, Database.DB.Connection))
					{
						cmd.ExecuteNonQuery();
					}
				}
				else
                {
					return false;
                }

				return true;
            }
			//catch
            {
				return false;
            }
        }
	}
}
