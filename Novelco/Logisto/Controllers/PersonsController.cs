using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MicroOrm.Dapper.Repositories.DbContext;
using Logisto.Models;
using Logisto.Data;

namespace Logisto.Controllers
{
	public class PersonsController : Controller
	{
		public ActionResult Index()
		{
			// http://www.codeproject.com/Articles/510725/How-to-add-lazy-loading-to-Dapper

			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
			var rep = new PersonRepository(conn, new MicroOrm.Dapper.Repositories.SqlGenerator.SqlGenerator<Person>());
			var list = rep.FindAll(w => w.ID > 0);

			return View("ListPersons", list);
		}

		public ActionResult CreateBank()
		{
			return View();
		}

		[HttpPost]
		public ActionResult CreateBank(Bank bank)
		{
			if (ModelState.IsValid)
			{
				var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
				var rep = new BanksRFRepository(conn, new MicroOrm.Dapper.Repositories.SqlGenerator.SqlGenerator<Bank>());
				rep.Insert(bank);
				return RedirectToAction("Index");
			}

			return View();
		}


		public ActionResult EditBank(int id)
		{
			var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
			var rep = new BanksRFRepository(conn, new MicroOrm.Dapper.Repositories.SqlGenerator.SqlGenerator<Bank>());
			var qq = rep.Find(w => w.ID == id);
			return View(qq);
		}

		[HttpPost]
		public ActionResult EditBank(Bank bank)
		{
			if (ModelState.IsValid)
			{
				var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
				var rep = new BanksRFRepository(conn, new MicroOrm.Dapper.Repositories.SqlGenerator.SqlGenerator<Bank>());
				rep.Update(bank);
				return RedirectToAction("Index");
			}

			return View();
		}

	}
}