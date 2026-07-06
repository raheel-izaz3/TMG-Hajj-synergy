using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.CompanyModels;
using TMG.Models.POModels;
using TMG.Models.ReservationModels;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.POControllers;

public class POVisaController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public POVisaController(TMGDbContext dbContext, IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _http = contextAccessor;
    }

    public IActionResult Index()
    {
        ViewBag.Trashed = _dbContext.POVisas.IgnoreQueryFilters()
            .Where(e => e.Trashed)
            .Count();

        return View(_dbContext.POVisas
            .Include(e => e.VendorType)
            .Include(e => e.Vendor)
            .Where(e => e.HajjSeasonsId ==
                        int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            );
    }

    public IActionResult Create()
    {
        ViewBag.Companies = _dbContext.UserCompanies
        .Include(e => e.Company)
            .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);

        return View();
    }

    [HttpPost]
    public IActionResult Create(POVisaFormModel request)
    {
        ViewBag.Companies = _dbContext.UserCompanies
               .Include(e => e.Company)
                   .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);


        var transaction = _dbContext.Database.BeginTransaction();

        int hajjSeasonId = int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId"));


        var poNumber = _dbContext.POVisas
            .Where(p => p.HajjSeasonsId == hajjSeasonId)
            .OrderByDescending(p => p.PONumber)
            .Select(p => p.PONumber)
            .FirstOrDefault();

        int nextpoNumber = 1;

        if (!string.IsNullOrEmpty(poNumber) && poNumber.StartsWith("PO-VIS-"))
        {
            var numericPart = poNumber.Substring(7);
            nextpoNumber = int.Parse(numericPart) + 1;

        }

        string FormatedNumber = $"PO-VIS-{nextpoNumber:D3}";

        try
        {
            var newPONumber = GeneratePONumber(hajjSeasonId);

            POVisa po = new POVisa
            {
                CompanyId = request.CompanyId,
                Date = request.Date,
                Description = request.Description,
                HajjSeasonsId = hajjSeasonId,
                PONumber = FormatedNumber,
                Terms = request.Terms,
                VatPercentage = request.VatPercentage,
                Amount = request.Amount,
                MunicipilityPercentage = request.MunicipilityPercentage,
                MunicipilityTax = request.MunicipilityTax,
                OtherTaxAmount = request.OtherTaxAmount,
                OtherTaxes = request.OtherTaxes,
                Subtotal = request.Subtotal,
                VAT = request.VAT,
                VendorId = request.VendorId,
                VendorTypeId = request.VendorTypeId,
                Total = request.Total,
            };

            _dbContext.POVisas.Add(po);

            _dbContext.SaveChanges();

            for (int i = 0; i < request.DueDates.Count; i++)
            {
                _dbContext.POVisaInstallments.Add(new POVisaInstallment
                {
                    DueDate = request.DueDates[i],
                    Percentage = request.Percentages[i],
                    Amount = request.Amounts[i],
                    POVisaId = po.POVisaId
                });
            }
            _dbContext.SaveChanges();

            transaction.Commit();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return View(request);
        }

    }

    public IActionResult Edit(int id)
    {
        ViewBag.Companies = _dbContext.UserCompanies
        .Include(e => e.Company)
            .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendors = _dbContext.Vendors.Where(e => !e.Trashed);

        POVisa po = _dbContext.POVisas.Find(id);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }

        ViewBag.POVisaInstallments = _dbContext.POVisaInstallments
            .Include(e => e.POVisa)
            .Where(e => e.POVisaId == po.POVisaId)
            .ToList();

        POVisaEditFormModel model = new POVisaEditFormModel
        {
            POVisaId = po.POVisaId,
            CompanyId = po.CompanyId,
            Date = po.Date,
            PONumber = po.PONumber,
            VatPercentage = po.VatPercentage,
            MunicipilityPercentage = po.MunicipilityPercentage,
            OtherTaxAmount = po.OtherTaxAmount,
            Description = po.Description,
            Amount = po.Amount,
            Subtotal = po.Subtotal,
            VAT = po.VAT,
            MunicipilityTax = po.MunicipilityTax,
            OtherTaxes = po.OtherTaxes,
            Total = po.Total,
            VendorId = po.VendorId,
            VendorTypeId = po.VendorTypeId,
            Terms = po.Terms,
        };



        return View(model);
    }

    [HttpPost]
    public IActionResult Edit(POVisaEditFormModel request)
    {
        ViewBag.Companies = _dbContext.UserCompanies
       .Include(e => e.Company)
           .Where(e => e.UserId == Convert.ToInt32(_http.HttpContext.Session.GetString("UserId")) && !e.Company.Trashed);

        ViewBag.VendorTypes = _dbContext.VendorTypes.Where(e => !e.Trashed);
        ViewBag.Vendors = _dbContext.Vendors.Where(e => !e.Trashed);

        var po = _dbContext.POVisas
            .Include(e => e.VendorType)
            .Include(e => e.Vendor)
            .Include(e => e.Company)
            .Include(e => e.POVisaInstallments)
            .FirstOrDefault(e => e.POVisaId == request.POVisaId);

        if (po == null)
        {
            return Json(new { msg = "Not Found" });
        }

        po.POVisaId = request.POVisaId;
        po.CompanyId = request.CompanyId;
        po.Date = request.Date;
        po.VatPercentage = request.VatPercentage;
        po.MunicipilityPercentage = request.MunicipilityPercentage;
        po.OtherTaxAmount = request.OtherTaxAmount;
        po.Description = request.Description;
        po.Amount = request.Amount;
        po.Subtotal = request.Subtotal;
        po.VAT = request.VAT;
        po.MunicipilityTax = request.MunicipilityTax;
        po.OtherTaxes = request.OtherTaxes;
        po.Total = request.Total;
        po.VendorId = request.VendorId;
        po.VendorTypeId = request.VendorTypeId;
        po.Terms = request.Terms;

        _dbContext.SaveChanges();

        _dbContext.POVisaInstallments.RemoveRange(po.POVisaInstallments.Where(P => P.POVisaId == request.POVisaId));
        for (int i = 0; i < request.DueDates.Count; i++)
        {
            _dbContext.POVisaInstallments.Add(new POVisaInstallment
            {
                DueDate = request.DueDates[i],
                Percentage = request.Percentages[i],
                Amount = request.Amounts[i],
                POVisaId = po.POVisaId
            });
        }
        _dbContext.SaveChanges();



        return RedirectToAction(nameof(Index));
    }

    public IActionResult Print(int id)
    {
        ViewBag.Installments = _dbContext.POVisaInstallments
                            .Where(e => e.POVisaId == id);

        POVisa po = _dbContext.POVisas
                        .Include(e => e.Company)
                        .Include(e => e.Vendor)
                        .FirstOrDefault(e => e.POVisaId == id);

        CompanyAddresses ca = _dbContext.CompanyAddresses
            .FirstOrDefault(e => e.CompanyId == po.CompanyId && e.Preferred);

        CompanyEmails ce = _dbContext.CompanyEmails
            .FirstOrDefault(e => e.CompanyId == po.CompanyId && e.Preferred);

        CompanyContacts cc = _dbContext.CompanyContacts
            .FirstOrDefault(e => e.CompanyId == po.CompanyId && e.Preferred);

        ViewBag.CompanyAddress = ca.Address;
        ViewBag.City = $"{ca.City}, {ca.Country}";
        ViewBag.Email = ce.CompanyEmail;
        ViewBag.Contact = cc.CompanyContact;


        return View(po);
    }

    private string GeneratePONumber(int hajjSeasonId)
    {
        // Get the latest PO number for the given HajjSeasonId
        var latestPO = _dbContext.POVisas
            .Where(po => po.HajjSeasonsId == hajjSeasonId)
            .OrderByDescending(po => po.PONumber)
            .FirstOrDefault();

        int nextNumber = 1;

        // If a PO exists, extract and increment the sequence number
        if (latestPO != null && !string.IsNullOrEmpty(latestPO.PONumber))
        {
            // Assuming PONumber format is "PO-XXXX"
            var poParts = latestPO.PONumber.Split('-');
            if (poParts.Length == 2 && int.TryParse(poParts[1], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        // Format the PO number as "PO-XXXX" (e.g., "PO-0001")
        string newPONumber = $"PO-{nextNumber:D4}";
        return newPONumber;
    }

}