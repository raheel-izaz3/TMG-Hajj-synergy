using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using TMG.Models.MiscModels;
using TMG.Models.Updated_Accounts;

namespace TMG.Controllers.AccountControllers
{
    public class VendorContactPersonsController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _host;

        public VendorContactPersonsController(TMGDbContext dbContext, IHttpContextAccessor http, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _http = http;
            _host = host;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.VendorContactPersons.Where(e => e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).Count();

            return View(_dbContext.VendorContactPersons.Include(e => e.Vendor).Include(e => e.Designation).Where(e => !e.Trashed && e.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult EmailIndex()
        {
            return View(_dbContext.EmailVendors.ToList());
        }

        public IActionResult Create()
        {
            var Vendor = _dbContext.Vendors.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
            ViewBag.Vendor = new SelectList(Vendor, "VendorId", "VendorName");
            ViewBag.Country = _dbContext.Countries
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.City = _dbContext.Cities
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.Designation = _dbContext.Designations
                .Where(e => !e.Trashed)
                .ToList();

            return View();
        }


        [HttpPost]
        public IActionResult Create(VendorContactPerson VCP)
        {

            if (!ModelState.IsValid)
            {
                var Vendor = _dbContext.Vendors.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
                ViewBag.Vendor = new SelectList(Vendor, "VendorId", "VendorName");

                return View();
            }

            VCP.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.VendorContactPersons.Add(VCP);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var Vendor = _dbContext.Vendors.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();

            var VCP = _dbContext.VendorContactPersons
                .Include(v => v.Vendor)
                .Include(v => v.Designation)
                .Where(v => v.VendorContactPersonId == id)
                .FirstOrDefault();

            ViewBag.Vendor = new SelectList(Vendor, "VendorId", "VendorName", VCP.VendorId);
            ViewBag.Country = _dbContext.Countries
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.City = _dbContext.Cities
                .Where(e => !e.Trashed)
                .ToList();
            ViewBag.Designation = _dbContext.Designations
                .Where(e => !e.Trashed)
                .ToList();

            return View(VCP);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(VendorContactPerson VCP)
        {

            if (!ModelState.IsValid)
            {
                var Vendor = _dbContext.Vendors.Where(c => !c.Trashed && c.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))).ToList();
                ViewBag.Vendor = new SelectList(Vendor, "VendorId", "VendorName");

                return View();
            }
            VCP.HajjSeasonsId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));
            _dbContext.Entry(VCP).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            return View(_dbContext.VendorContactPersons.Include(e => e.Vendor).Include(v => v.Designation).Where(s => s.Trashed && s.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"))));
        }

        public IActionResult Trash(int id)
        {
            var VCP = _dbContext.VendorContactPersons.Find(id);
            if (VCP == null) return RedirectToAction(nameof(Index));

            VCP.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            var VCP = _dbContext.VendorContactPersons.Find(id);
            VCP.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Certificate(int id)
        {
            ViewBag.ID = id;

            ViewBag.VendorName = _dbContext.VendorContactPersons
                .Where(e => e.VendorContactPersonId == id)
                .Select(e => e.Name)
                .FirstOrDefault();

            ViewBag.VendorHotel = _dbContext.VendorContactPersons
                .Include(e => e.Vendor)
                .Where(e => e.VendorContactPersonId == id)
                .Select(e => e.Vendor.VendorName)
                .FirstOrDefault();

            return View();
        }

        public IActionResult EmailVendor()
        {
            ViewBag.Vendors = _dbContext.VendorContactPersons
                .Include(e => e.Vendor)
                .Where(e => !e.Trashed)
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailVendor(EmailVendor vendor, IFormFile attachment, int vendorId)
        {
            string email = _dbContext.VendorContactPersons
                .Where(e => e.VendorContactPersonId == vendorId)
                .Select(e => e.Email)
                .FirstOrDefault();

            if (email == null)
            {
                ModelState.AddModelError("", "Vendor email not found.");
                return View(vendor);
            }

            vendor.Email = email;

            string attachmentPath = null;

            if (attachment is not null && attachment.Length > 0)
            {
                string fileName = Path.GetFileName(attachment.FileName);
                string folderPath = Path.Combine(_host.WebRootPath, "Images", "VendorEmail");
                Directory.CreateDirectory(folderPath); 

                attachmentPath = Path.Combine(folderPath, fileName);
                if (!string.IsNullOrEmpty(attachmentPath)) { string dir_dt = Path.GetDirectoryName(attachmentPath); if (!Directory.Exists(dir_dt)) Directory.CreateDirectory(dir_dt); }
                using (var stream = new FileStream(attachmentPath, FileMode.Create))
                {
                    await attachment.CopyToAsync(stream);
                }

                vendor.Attachment = @"Images\VendorEmail\" + fileName;
            }

            _dbContext.EmailVendors.Add(vendor);
            _dbContext.SaveChanges();

            try
            {
                using (var message = new MailMessage())
                {
                    message.To.Add(email);
                    message.Subject = vendor.Subject;
                    message.Body = vendor.Message;
                    message.IsBodyHtml = true;

                    message.From = new MailAddress("noreply@tmgpk.com", "Travel Mate - TMG");

                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        message.Attachments.Add(new Attachment(attachmentPath));
                    }

                    using (var smtp = new SmtpClient("smtp.tmgpk.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("noreply@tmgpk.com", "U7H#sT8sLXA^2!8k");
                        smtp.EnableSsl = false;

                        await smtp.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email sending failed: " + ex.Message);
            }

            return RedirectToAction(nameof(EmailIndex));
        }
    }
}
