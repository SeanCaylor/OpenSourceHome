using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenSourceHome.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace OpenSourceHome.Controllers
{
    public class HomeController : Controller
    {
        // private readonly ILogger<HomeController> _logger;
        private OpenSourceHomeContext db;
        private readonly SmtpClient _smtpClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string apiKey;
        private readonly string mailFrom;
        private readonly string mailFromName;
        //Below are parts of the admin view.
        // private readonly string adminView;
        // private readonly string adminPass;
        public HomeController(OpenSourceHomeContext context, IConfiguration settings, IWebHostEnvironment hostingEnvironment)
        {
            apiKey = settings["APIKey:GoogleStreet"];
            db = context;
            SmtpClient smtpClient = new SmtpClient()
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Port = int.Parse(settings["MailSettings:MailServerPort"]),
                Host = settings["MailSettings:MailServerAddress"],
                Credentials = new NetworkCredential(settings["MailSettings:UserName"], settings["MailSettings:UserPassword"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
            mailFromName = settings["MailSettings:FromName"];
            mailFrom = settings["MailSettings:FromAddress"];
            _smtpClient = smtpClient;
            _hostingEnvironment = hostingEnvironment;
            // adminView = settings["AdminView:AdminView"];
            // adminPass = settings["AdminView:AViewPass"];
        }
        private int? uid
        {
            get
            {
                return HttpContext.Session.GetInt32("UserId");
            }
        }
        private bool isLoggedIn
        {
            get
            {
                return uid != null;
            }
        }
        [HttpGet("")]
        public IActionResult Homepage()
        {
            HttpContext.Session.SetString("ApiLink", "https://maps.googleapis.com/maps/api/js?sensor=false&libraries=places&key=" + apiKey);
            // TempData.Remove("AdminView");
            return View("Index");
        }
        [HttpGet("/home")]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("ApiLink") == null)
            {
                HttpContext.Session.SetString("ApiLink", "https://maps.googleapis.com/maps/api/js?sensor=false&libraries=places&key=" + apiKey);
            }
            return View("Index");
        }
        [HttpPost("/home/sngate")]
        public IActionResult Gatekeeper(Keymaster userInput)
        {
            // See below, re: admin mode
            // if (userInput.UserSNInput.ToString() == adminView)
            // {
            //     TempData.Add("AdminView", true);
            //     return View("AdminPass");
            // }
            var vinzeClortho = db.SerialNumbers.FirstOrDefault(s => s.HashedSN == userInput.UserSNInput);
            if (ModelState.IsValid)
            {
                if (vinzeClortho != null)
                {
                    HttpContext.Session.SetString("SNValid", "yes");
                    HttpContext.Session.SetString("UserSN", userInput.UserSNInput);
                    HttpContext.Session.SetInt32("UserSNID", vinzeClortho.SerialNumberId);
                    return Redirect("/home/downloads");
                }
                ModelState.AddModelError("UserSNInput", "doesn't appear to be in our system, please register or try again.");
            }
            return View("Index");
        }
        [HttpGet("/home/downloads")]
        public IActionResult DownloadSplash()
        {
            if (HttpContext.Session.GetString("SNValid") == "yes")
            {
                SerialNumber userSerial = db.SerialNumbers.FirstOrDefault(sn => sn.SerialNumberId == HttpContext.Session.GetInt32("UserSNID"));
                if (userSerial == null)
                {
                    return Redirect("/");
                }
                return View("Download", userSerial);
            }
            return Redirect("/");
        }
        [HttpGet("/login")]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("ApiLink") == null)
            {
                HttpContext.Session.SetString("ApiLink", "https://maps.googleapis.com/maps/api/js?sensor=false&libraries=places&key=" + apiKey);
            }
            return View("LoginRegister");
        }
        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            ViewBag.Message = "Address: " + form["txtAddress"];
            ViewBag.Message = "\\nLatitude: " + form["txtLatitude"];
            ViewBag.Message = "\\nLongitude: " + form["txtLongitude"];
            return View();
        }
        [HttpPost("/login/register")]
        public IActionResult Process(User newUser)
        {
            if (ModelState.IsValid)
            {
                Random rand = new Random();
                SerialNumber newSN = new SerialNumber();
                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMinimum8Chars = new Regex(@".{8,}");
                var validUsername = new Regex("[^A-Za-z0-9]");
                var passValid = false;
                var usernameValid = true;
                if (db.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "is taken.");
                }
                if (db.Users.Any(u => u.UserName == newUser.UserName))
                {
                    ModelState.AddModelError("Username", "is taken.");
                }
                if (validUsername.IsMatch(newUser.UserName))
                {
                    usernameValid = false;
                }
                if (!usernameValid)
                {
                    ModelState.AddModelError("Username", "is not valid, only alphanumeric characters");
                }
                if (hasNumber.IsMatch(newUser.Password) && hasUpperChar.IsMatch(newUser.Password) && hasMinimum8Chars.IsMatch(newUser.Password))
                {
                    passValid = true;
                }
                if (!passValid)
                {
                    ModelState.AddModelError("PasswordConfirm", "Password must contain one upper, one lower, and one number");
                }
                if (newUser.Password != newUser.PasswordConfirm)
                {
                    ModelState.AddModelError("PasswordConfirm", "doesn't match");
                }
                if (newUser.Location == "")
                {
                    ModelState.AddModelError("Location", "shouldn't be empty, or Null Island.");
                }
                if (newUser.nLatitude == 0 && newUser.nLongitude == 0)
                {
                    ModelState.AddModelError("Location", "shouldn't be empty, or Null Island.");
                }
                if (!ModelState.IsValid)
                {
                    return View("LoginRegister");
                }
                int lat = Math.Abs((int)Math.Round(newUser.nLatitude));
                while (lat >= 10)
                {
                    lat /= 10;
                }
                int lon = Math.Abs((int)Math.Round(newUser.nLongitude));
                while (lon >= 10)
                {
                    lon /= 10;
                }
                int latlon = Math.Abs((lat * rand.Next(100, 1000)) * (lon * rand.Next(100, 1000)));
                while (latlon >= 10)
                {
                    latlon /= 10;
                }
                newSN.HashedSN = "PMD" + rand.Next(100, 1000) + lat.ToString() + lon.ToString() + latlon.ToString() + rand.Next(100, 1000);
                newSN.Downloads = 0;
                db.SerialNumbers.Add(newSN);
                db.SaveChanges();
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(mailFrom, mailFromName);
                msg.To.Add(new MailAddress(newUser.Email));
                msg.Subject = "Thank you for registering!";
                string plainBody = $"Thank you for registering with PowerMouse Designs’ Open Source Home Project!\nWe thank you for being a part of the zero emission home revolution. Along with the below serial number, used to access the plans download page, but you now also have access to the forums.\nThe below serial number can also be used multiple times, so please retain this email for your records.\nFeel free to reach out to the administrator at opensourcehomeproject@gmail.com\n\nYOUR SERIAL NUMBER\n************************\n*** {newSN.HashedSN} ***" + "\n************************";
                string htmlBody = "<!doctype html><html><head><meta name='viewport' content='width=device-width, initial-scale=1.0'><meta http-equiv='Content-Type' content='text/html; charset=UTF-8'><title>Registration Email</title><style>@media only screen and (max-width: 620px) {table.body h1 {font-size: 28px !important;margin-bottom: 10px !important;}table.body p,table.body ul,table.body ol,table.body td,table.body span,table.body a {font-size: 16px !important;}table.body .wrapper,table.body .article {padding: 10px !important;}table.body .content {padding: 0 !important;}table.body .container {padding: 0 !important;width: 100% !important;}table.body .main {border-left-width: 0 !important;border-radius: 0 !important;border-right-width: 0 !important;}table.body .btn table {width: 100% !important;}table.body .btn a {width: 100% !important;}table.body .img-responsive {height: auto !important;max-width: 100% !important;width: auto !important;}}@media all {.ExternalClass {width: 100%;}.ExternalClass,.ExternalClass p,.ExternalClass span,.ExternalClass font,.ExternalClass td,.ExternalClass div {line-height: 100%;}.apple-link a {color: inherit !important;font-family: inherit !important;font-size: inherit !important;font-weight: inherit !important;line-height: inherit !important;text-decoration: none !important;}#MessageViewBody a {color: inherit;text-decoration: none;font-size: inherit;font-family: inherit;font-weight: inherit;line-height: inherit;}.btn-primary table td:hover {background-color: #34495e !important;}.btn-primary a:hover {background-color: #34495e !important;border-color: #34495e !important;}}</style></head><body style='background-color: #f6f6f6; font-family: sans-serif; -webkit-font-smoothing: antialiased; font-size: 14px; line-height: 1.4; margin: 0; padding: 0; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%;'><span class='preheader' style='color: transparent; display: none; height: 0; max-height: 0; max-width: 0; opacity: 0; overflow: hidden; mso-hide: all; visibility: hidden; width: 0;'>Thank you so much for registering with the Open Source Home Project</span><table role='presentation' border='0' cellpadding='0' cellspacing='0' class='body' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f6f6f6; width: 100%;' width='100%' bgcolor='#f6f6f6'><tr><td style='font-family: sans-serif; font-size: 14px; vertical-align: top;' valign='top'>&nbsp;</td><td class='container' style='font-family: sans-serif; font-size: 14px; vertical-align: top; display: block; max-width: 580px; padding: 10px; width: 580px; margin: 0 auto;' width='580' valign='top'><div class='content' style='box-sizing: border-box; display: block; margin: 0 auto; max-width: 580px; padding: 10px;'><table role='presentation' class='main' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; background: #ffffff; border-radius: 3px; width: 100%;' width='100%'><tr><td class='wrapper' style='font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;' valign='top'><table role='presentation' border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;' width='100%'><tr><td style='font-family: sans-serif; font-size: 14px; vertical-align: top;' valign='top'><p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; margin-bottom: 15px;'>Thank you, " + $"{newUser.UserName}!" + "</p><p style='font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; margin-bottom: 15px;'>You’ve registered an account with the zero emission home revolution! Along with the below serial number, which is used to access the plans download page, you now have access to the forums! The below serial number can be used multiple times, so please retain this email for your records. If you need any assistance, email us at <a href='mailto: opensourcehomeproject@gmail.com'>opensourcehomeproject@gmail.com</a></p><table role='presentation' border='0' cellpadding='0' cellspacing='0' class='btn btn-primary' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; box-sizing: border-box; width: 100%;' width='100%'><tbody><tr><td align='left' style='font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;' valign='top'><table role='presentation' border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;'><tbody><tr><td style='font-family: sans-serif; font-size: 24px; vertical-align: top; border-radius: 5px; text-align: center; background-color: #3498db;' valign='top' align='center' bgcolor='#3498db'>" + $"{newSN.HashedSN}" + "</td></tr></tbody></table></td></tr></tbody></table></td></tr></table></td></tr></table>";
                AlternateView alternateViewPlainText = AlternateView.CreateAlternateViewFromString(plainBody, null, MediaTypeNames.Text.Plain);
                AlternateView alternateViewHtmlText = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                msg.IsBodyHtml = true;
                msg.AlternateViews.Add(alternateViewHtmlText);
                msg.AlternateViews.Add(alternateViewPlainText);
                msg.Priority = MailPriority.Normal;
                _smtpClient.Send(msg);
                newUser.SerialNumberId = newSN.SerialNumberId;
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                db.Users.Add(newUser);
                db.SaveChanges();
                HttpContext.Session.SetInt32("UserId", newUser.UserId);
                HttpContext.Session.SetString("UserName", newUser.UserName);
                HttpContext.Session.SetString("UserSN", newSN.HashedSN);
                TempData.Add("Registered", newUser.UserName);
                return Redirect("/");
            }
            return View("LoginRegister");
        }
        [HttpPost("/login/process")]
        public IActionResult LoginProcess(LoginUser loginUser)
        {
            var loginCheck = new Regex("[^A-Za-z0-9]");
            if (!ModelState.IsValid)
            {
                return View("LoginRegister");
            }
            else if (loginCheck.IsMatch(loginUser.LoginUsername))
            {
                ModelState.AddModelError("LoginUsername", "please enter a valid username");
                return View("LoginRegister");
            }
            User dbUser = db.Users.FirstOrDefault(u => u.UserName == loginUser.LoginUsername);
            SerialNumber dbSN = db.SerialNumbers.FirstOrDefault(s => s.SerialNumberId == dbUser.SerialNumberId);
            if (dbUser == null)
            {
                ModelState.AddModelError("LoginUsername", "Username/Password combination not found");
                return View("LoginRegister");
            }
            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
            PasswordVerificationResult pwCompareResult = hasher.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.LoginPassword);
            if (pwCompareResult == 0)
            {
                ModelState.AddModelError("LoginUsername", "Username/Password combination not found");
                return View("LoginRegister");
            }
            HttpContext.Session.SetInt32("UserId", dbUser.UserId);
            HttpContext.Session.SetString("UserName", dbUser.UserName);
            HttpContext.Session.SetString("UserSN", dbSN.HashedSN);
            TempData.Add("LoggedIn", dbUser.UserName);
            return Redirect("/home");
        }
        //The admin page, which needs its own controller, commenting out for now, will add in update

        // [HttpPost("/admin/process")]
        // public IActionResult AdminLoginProcess(IFormCollection input)
        // {
        //     if (input["APassInput"] == adminPass)
        //     {
        //         return RedirectToAction("AdminPortal");
        //     }
        //     return View("Index");
        // }
        // [HttpGet("/admin/page")]
        // public IActionResult AdminPortal()
        // {
        //     if (!(bool)TempData["AdminView"])
        //     {
        //         return RedirectToAction("Index");
        //     }
        //     TempData.Remove("AdminView");
        //     return RedirectToAction("AdminPortal");
        // }
        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}