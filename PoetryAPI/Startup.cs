using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoetryAPI
{
	public class Startup
	{
		public static RussianDict Dict = new RussianDict();
		public static SpellChecker Spell = new SpellChecker();
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;

#if DEBUG
			Database.DB = new DBConnection()
			{
				Server = "localhost",
				DatabaseName = "poetrydb",
				Username = "root",
				Password = "cool_0789"
			};
#else
			Database.DB = new DBConnection()
			{
				Server = "/var/run/mysqld/mysqld.sock",
				DatabaseName = "poetrydb",
				Username = "poetry",
				Password = "3WskCJ_0789"
			};
#endif

			TestSql();

			//Spell.LoadSpellChecker();
			//Dict.LoadDictionary(1);
		}
		private void TestSql()
		{
			if (Database.DB.IsConnect())
			{
				string query = "SELECT * FROM WordsDictionary LIMIT 2";
				using (MySqlCommand cmd = new MySqlCommand(query, Database.DB.Connection))
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							for (int i = 0; i < 8; i++)
							{
								object col = reader.GetValue(i);
								//System.Diagnostics.Debug.WriteLine(col);
								Console.WriteLine(col);
							}
						}
					}
				}
			}
			else
            {
				Console.WriteLine("DB Connection failed");
            }
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "PoetryAPI", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PoetryAPI v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
