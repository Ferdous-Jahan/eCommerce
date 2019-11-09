using Khichuri.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Khichuri.Controllers
{
    public class CustomerController : Controller
    {
        ShopEntities db = new ShopEntities();

        // GET: Customer
        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Password,Address,Email,PhoneNo")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(customer);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Customer customer)
        {
            using (ShopEntities db = new ShopEntities())
            {
                var customerDetail = db.Customers.Where(x => x.Name == customer.Name && x.Password == customer.Password).FirstOrDefault();
                if (customerDetail == null)
                {
                    var adminDetail = db.Admins.Where(x => x.Name == customer.Name && x.Password == customer.Password).FirstOrDefault();
                    if (adminDetail == null)
                    {
                        return View("Login");
                    }
                    else
                    {
                        Session["adminId"] = customer.Id;
                        Session["adminName"] = customer.Name;
                        return RedirectToAction("Index", "Admin");
                    }
                }
                else
                {
                    Session["userId"] = customer.Id;
                    Session["userName"] = customer.Name;
                    return RedirectToAction("Products");
                }
            }
        }

        public ActionResult Products()
        {
            return View(db.Products.ToList());
        }

        [HttpPost]
        public ActionResult Details(Product product)
        {
            PurchaseHistory pH = new PurchaseHistory();

            pH.ProductId = product.Id;
            pH.CustomerId = Convert.ToInt32(Session["userId"]);
            pH.Status = "On process";
            pH.Quantity = Convert.ToInt32(product.Quantity);

            db.PurchaseHistories.Add(pH);
            db.SaveChanges();

            return RedirectToAction("Products");
        }

        public ActionResult Logout()
        {
            int userId = (int)Session["userId"];
            Session.Abandon();
            return RedirectToAction("Index");
        }

        public ActionResult Cart()
        {
            List<PurchaseHistory> pl = new List<PurchaseHistory>();
            using (ShopEntities db = new ShopEntities())
            {
                var pH = db.PurchaseHistories.Where(x => x.CustomerId == (int)Session["userId"]).FirstOrDefault();
                pl.Add(pH);
            }
            return View(pl);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}



