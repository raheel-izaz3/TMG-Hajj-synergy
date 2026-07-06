using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using TMG.Data;
using TMG.Models.AccomodationModels;
using TMG.Models.FamilyModels;
using TMG.Models.AccommodationReportModels;

namespace TMG.Controllers.AccomodationControllers;
public partial class AccommodationReportController : Controller
{
    private readonly TMGDbContext _dbContext;
    private readonly IHttpContextAccessor _http;

    public AccommodationReportController(TMGDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
    }

    public IActionResult AccomodationReport(string dateType, DateTime? filterDate)
    {
        List<AccommodationReportModel> model = new List<AccommodationReportModel>();

        var familyHeadAccommodations = _dbContext.FamilyHeadAccomodations
            .Include(e => e.IndividualFacilityDescription)
            .ThenInclude(e => e.Room)
            .Include(e => e.Hotel)
            .Include(e => e.FamilyHead)
            .ThenInclude(e => e.Package)
            .ThenInclude(e => e.Company)
            .Where(e => !e.FamilyHead.Trashed
                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            .ToList();


        if (filterDate.HasValue && !string.IsNullOrEmpty(dateType))
        {
            DateTime selectedDate = filterDate.Value.Date;

            if (dateType == "arrival")
            {
                familyHeadAccommodations = familyHeadAccommodations
                    .Where(accomodation => accomodation.CheckIn.HasValue && accomodation.CheckIn.Value.Date == selectedDate)
                    .ToList();
            }
            else if (dateType == "departure")
            {
                familyHeadAccommodations = familyHeadAccommodations
                    .Where(accomodation => accomodation.CheckOut.HasValue && accomodation.CheckOut.Value.Date == selectedDate)
                    .ToList();
            }
        }

        var groupedAccommodations = familyHeadAccommodations
            .GroupBy(e => e.FamilyHeadId)
            .ToList();

        var groupMembers = _dbContext.GroupMembers
            .Include(e => e.Group)
            .Include(e => e.FamilyHead)
            .Where(e => !e.FamilyHead.Trashed
                    && e.FamilyHead.HajjSeasonsId == int.Parse(_http.HttpContext.Session.GetString("ActiveSeasonId")))
            .ToList();

        int i = 0;

        foreach (var group in groupedAccommodations)
        {
            var makkah = group.FirstOrDefault(e => e.Hotel.PlaceId == Place.MAKKAH_ID);
            var medina = group.FirstOrDefault(e => e.Hotel.PlaceId == Place.MEDINA_ID);
            var azizia = group.FirstOrDefault(e => e.Hotel.PlaceId == Place.AZIZIA_ID);
            var mina = group.FirstOrDefault(e => e.Hotel.PlaceId == Place.MINA_ID);

            AccommodationReportModel _ = new()
            {
                SNo = ++i,
                CompanyCode = group.FirstOrDefault()?.FamilyHead.Package.Company.CompanyCode,
                PackageName = group.FirstOrDefault()?.FamilyHead.Package.Name,
                GroupCode = groupMembers.FirstOrDefault(e => e.FamilyHeadId == group.FirstOrDefault()?.FamilyHeadId)?.Group.Code,
                HajiName = group.FirstOrDefault()?.FamilyHead.FullName,
                HbNumber = group.FirstOrDefault()?.FamilyHead.FcId,
                MakkahHotel = new AccommodationReportHotelModel(
                        makkah?.Hotel?.HotelName,
                        $"{makkah?.IndividualFacilityDescription?.Room?.RoomCode} (F#{makkah?.FloorNumber} R#{makkah?.RoomNumber})",
                        makkah?.CheckIn?.ToString("dd/MMM/yyyy"),
                        makkah?.CheckIn?.ToString("HH:mm"),
                        makkah?.CheckOut?.ToString("dd/MMM/yyyy"),
                        makkah?.CheckOut?.ToString("HH:mm")),
                MedinaHotel = new AccommodationReportHotelModel(
                        medina?.Hotel?.HotelName,
                        $"{medina?.IndividualFacilityDescription?.Room?.RoomCode} (F#{medina?.FloorNumber} R#{medina?.RoomNumber})",
                        medina?.CheckIn?.ToString("dd/MMM/yyyy"),
                        medina?.CheckIn?.ToString("HH:mm"),
                        medina?.CheckOut?.ToString("dd/MMM/yyyy"),
                        medina?.CheckOut?.ToString("HH:mm")),
                MinaCamp = new AccommodationReportHotelModel(
                        mina?.Hotel?.HotelName,
                        $"{mina?.IndividualFacilityDescription?.Room?.RoomCode}",
                        mina?.CheckIn?.ToString("dd/MMM/yyyy"),
                        mina?.CheckIn?.ToString("HH:mm"),
                        mina?.CheckOut?.ToString("dd/MMM/yyyy"),
                        mina?.CheckOut?.ToString("HH:mm")),
                AziziaHotel = new AccommodationReportHotelModel(
                        azizia?.Hotel?.HotelName,
                        $"{azizia?.IndividualFacilityDescription?.Room?.RoomCode} (F#{azizia?.FloorNumber} R#{azizia?.RoomNumber})",
                        azizia?.CheckIn?.ToString("dd/MMM/yyyy"),
                        azizia?.CheckIn?.ToString("HH:mm"),
                        azizia?.CheckOut?.ToString("dd/MMM/yyyy"),
                        azizia?.CheckOut?.ToString("HH:mm"))
            };

            model.Add(_);
        }

        return View(model);
    }

}
