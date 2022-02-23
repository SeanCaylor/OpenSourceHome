using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using OpenSourceHome.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace OpenSourceHome.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private OpenSourceHomeContext db;
        public DownloadController(OpenSourceHomeContext context, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            db = context;
        }
        public FileResult DownloadFile()
        {
            var dltracker = db.SerialNumbers.FirstOrDefault(sn => sn.SerialNumberId == HttpContext.Session.GetInt32("UserSNID"));
            dltracker.Downloads++;
            db.SaveChanges();
            string path = "" + _hostingEnvironment.WebRootPath + "/PH-Plans.pdf";
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", "PH-Plans-DL" + $"{dltracker.Downloads}" + ".pdf");
        }
    }
}