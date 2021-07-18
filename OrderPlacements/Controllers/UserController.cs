using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderPlacements.Models;

namespace OrderPlacements.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            customer customerModel = new customer();
            return View(customerModel);
        }

        [HttpPost]
        public ActionResult Index(customer customerModel)
        {
            using (DbModels dbModels = new DbModels())
            {
                if (dbModels.customers.Any(x => x.username == customerModel.username))
                {
                    ViewBag.DuplicateMessage = "Username already exist.";
                    return View("Index", customerModel);
                }
                dbModels.customers.Add(customerModel);
                dbModels.SaveChanges();
            }
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(customer customerModel)
        {
            using (DbModels dbModels = new DbModels())
            {
                if (dbModels.customers.Any(x => x.username == customerModel.username && x.password == customerModel.password))
                {
                    var customer = dbModels.customers.Where(x => x.username == customerModel.username)
                                .Select(x => new {
                                    ID = x.id,
                                }).Single();
                    Session["userID"] = customer.ID;

                    return RedirectToAction("Index", "Order", new { id = (int)customer.ID });
                }
                else
                {
                    return RedirectToAction("Login", "User");
                }
            }
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login", "User");
        }
    }
}