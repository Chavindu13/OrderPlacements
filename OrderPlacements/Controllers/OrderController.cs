using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderPlacements.Models;
using System.Net.Mail;
using System.Data;

namespace OrderPlacements.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index(int id)
        {
            using (DbModels dbModels = new DbModels())
            {
                return View(dbModels.orders.Where(x => x.customer_id == id && x.status == "Pending").ToList());
            }
        }

        [HttpGet]
        public ActionResult Order()
        {
            order orderModel = new order();
            return View(orderModel);
        }

        [HttpPost]
        public ActionResult Order(order orderModel)
        {
            using (DbModels dbModels = new DbModels())
            {
                orderModel.customer_id = (int)Session["userID"];

                var customer = dbModels.customers.Where(x => x.id == orderModel.customer_id)
                                .Select(x => new {
                                    Mail = x.email,
                                }).Single();
                string customermail = customer.Mail;

                if (!orderModel.pack_one)
                {
                    orderModel.pack_one_quantity = 0;
                }
                if (!orderModel.pack_two)
                {
                    orderModel.pack_two_quantity = 0;
                }
                if (!orderModel.pack_three)
                {
                    orderModel.pack_three_quantity = 0;
                }
                orderModel.total = (orderModel.pack_one_quantity * 250) + (orderModel.pack_two_quantity * 350) + (orderModel.pack_three_quantity * 500);
                orderModel.status = "Pending";
                dbModels.orders.Add(orderModel);
                dbModels.SaveChanges();

                DataTable dt = new DataTable("Order");

                DataColumn colPackage = new DataColumn("Package", typeof(System.String));
                DataColumn colQuantity = new DataColumn("Quantity", typeof(System.Decimal));
                DataColumn colTotal = new DataColumn("Price", typeof(System.Decimal));

                dt.Columns.AddRange(new DataColumn[] { colPackage, colQuantity, colTotal });
                decimal totalQuantity = (decimal)(orderModel.pack_one_quantity + orderModel.pack_two_quantity + orderModel.pack_three_quantity);

                dt.Rows.Add("Package 01(LKR 250)", orderModel.pack_one_quantity, orderModel.pack_one_quantity * 250);
                dt.Rows.Add("Package 02(LKR 350)", orderModel.pack_two_quantity, orderModel.pack_two_quantity * 350);
                dt.Rows.Add("Package 03(LKR 500)", orderModel.pack_three_quantity, orderModel.pack_three_quantity * 500);
                dt.Rows.Add("Total", totalQuantity, orderModel.total);

                string htmlString = getHtml(dt);
                Email(htmlString, customermail);
            }
            //return View("Index", new customer());
            return RedirectToAction("Index", "Order", new { id = (int)Session["userID"] });
        }

        public static string getHtml(DataTable dataTable)
        {
            try
            {
                string messageBody = "<font>Your Order Details: </font><br><br>";
                if (dataTable.Rows.Count == 0) return messageBody;
                string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
                string htmlTableEnd = "</table>";
                string htmlHeaderRowStart = "<tr style=\"background-color:#6FA1D2; color:#ffffff;\">";
                string htmlHeaderRowEnd = "</tr>";
                string htmlTrStart = "<tr style=\"color:#555555;\">";
                string htmlTrEnd = "</tr>";
                string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
                string htmlTdEnd = "</td>";
                messageBody += htmlTableStart;
                messageBody += htmlHeaderRowStart;
                messageBody += htmlTdStart + "Package" + htmlTdEnd;
                messageBody += htmlTdStart + "Quantity" + htmlTdEnd;
                messageBody += htmlTdStart + "Sub Total" + htmlTdEnd;
                messageBody += htmlHeaderRowEnd;
                
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    messageBody = messageBody + htmlTrStart;
                    messageBody = messageBody + htmlTdStart + dataTable.Rows[i][0].ToString() + htmlTdEnd;
                    messageBody = messageBody + htmlTdStart + dataTable.Rows[i][1].ToString() + htmlTdEnd;
                    messageBody = messageBody + htmlTdStart + dataTable.Rows[i][2].ToString() + htmlTdEnd;
                    messageBody = messageBody + htmlTrEnd;
                }
                messageBody = messageBody + htmlTableEnd;
                return messageBody;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void Email(string htmlString, string mailAddress)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("chavindu13@gmail.com");
                message.To.Add(new MailAddress(mailAddress));
                message.Subject = "Digico Labs - Order Details";
                message.IsBodyHtml = true;
                message.Body = htmlString;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("chavindu13@gmail.com", "password");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex) {
                
            }
        }
    }
}