using TMG.Data;
using TMG.Models.AccomodationModels;
using TMG.Models.AirlineModels;
using TMG.Models.CompanyModels;
using TMG.Models.FamilyModels;
using TMG.Models.PackageModels;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers
{
    public class ReportingFormController : Controller
    {
        private readonly TMGDbContext _context;

        public ReportingFormController(TMGDbContext context)
        {
            _context = context;
        }
        public IActionResult ReportingForm(int id)
            {
            List<ReportingFormModel> modellist = new List<ReportingFormModel>();


            var munazzimQuotas =
                from c in _context.Companies
                join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
                where !c.Trashed && !m.Trashed
                group c by m.MunazzimId into g
                select new
                {
                    MunazzimId = g.Key,
                    TotalQuota = g.Sum(x => x.Quota)
                };


            var query1 = from fh in _context.FamilyHeads
                        join p in _context.Packages on fh.PackageId equals p.PackageId
                        join pa in _context.PackageAccomodtions on p.PackageId equals pa.PackageId
                        join c in _context.Companies on p.CompanyId equals c.CompanyId
                        join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
                        join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
                        join pnrm in _context.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
                        join pnr in _context.PNRs on pnrm.PNRId equals pnr.PNRId
                        join f in _context.Flights on pnr.FlightId equals f.FlightId
                        join s in _context.Sectors on f.FlightId equals s.FlightId
                        join cl in _context.CompanyLicenses on c.CompanyId equals cl.CompanyId
                        join mq in munazzimQuotas on m.MunazzimId equals mq.MunazzimId
                        where !fh.Trashed
                              && !c.Trashed
                              && !f.Trashed
                              && !pnr.Trashed
                              && s.ArrivalDateTime == _context.Sectors
                                  .Where(sector => sector.FlightId == f.FlightId && sector.FlightType == (FlightType)1)
                                  .Max(sector => sector.ArrivalDateTime)
                              && f.FlightId == id
                        group new { fh, c, cl, m, p, f, s, pa, mq } by m.MunazzimId into g
                        select new
                        {
                            FamilyHeadCount = g.Select(x => x.fh.FamilyHeadId).Distinct().Count(),
                            CompanyName = g.Max(x => x.c.CompanyName),
                            EnrollmentNo = g.Max(x => x.cl.CompanyLicense),
                            MunazzimName = g.Max(x => x.m.Name),
                            MoraRegistrationNo = g.Max(x => x.m.ClusterNumber),
                            MunazzimNumber = g.Max(x => x.p.PackageNumber),
                            OutboundFlightNumber = g.Max(x => x.f.OutboundFlightNumber),
                            ArrivalDateTime = g.Max(x => x.s.ArrivalDateTime),
                            Destination = g.Max(x => x.s.Destination),
                            PlaceOfDep = g.Max(x => x.fh.POD),
                            MunazzimId = g.Max(x => x.m.MunazzimId),
                            TotalQuota = g.Max(x => x.mq.TotalQuota),
                            AccomodationCategoryId = g.Max(x => x.pa.AccomodationCategoryId)
                        };


            // ====================== JAHANZEB'S QUERY START ======================

            //var query1 = from fh in _context.FamilyHeads
            //             join p in _context.Packages on fh.PackageId equals p.PackageId
            //             join pa in _context.PackageAccomodtions on p.PackageId equals pa.PackageId
            //             join c in _context.Companies on p.CompanyId equals c.CompanyId
            //             join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
            //             join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
            //             join pnrm in _context.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
            //             join pnr in _context.PNRs on pnrm.PNRId equals pnr.PNRId
            //             join f in _context.Flights on pnr.FlightId equals f.FlightId
            //             join s in _context.Sectors on f.FlightId equals s.FlightId
            //             join cl in _context.CompanyLicenses on c.CompanyId equals cl.CompanyId
            //             join mq in munazzimQuotas on m.MunazzimId equals mq.MunazzimId
            //             where !fh.Trashed
            //                   && !c.Trashed
            //                   && !f.Trashed
            //                   && !pnr.Trashed
            //                   && s.ArrivalDateTime == _context.Sectors
            //                       .Where(sector => sector.FlightId == f.FlightId && sector.FlightType == (FlightType)1)
            //                       .Max(sector => sector.ArrivalDateTime)
            //                   && f.FlightId == id
            //             group new { fh, c, cl, m, p, f, s, pa, mq } by new
            //             {
            //                 c.CompanyName,
            //                 cl.CompanyLicense,
            //                 m.Name,
            //                 m.ClusterNumber,
            //                 p.PackageNumber,
            //                 f.OutboundFlightNumber,
            //                 s.Destination,
            //                 fh.POD,
            //                 m.MunazzimId,
            //                 mq.TotalQuota,
            //                 pa.AccomodationCategoryId
            //             } into g
            //             select new
            //             {
            //                 FamilyHeadCount = g.Select(x => x.fh.FamilyHeadId).Distinct().Count(),
            //                 CompanyName = g.Key.CompanyName,
            //                 EnrollmentNo = g.Key.CompanyLicense,
            //                 MunazzimName = g.Key.Name,
            //                 MoraRegistrationNo = g.Key.ClusterNumber,
            //                 MunazzimNumber = g.Key.PackageNumber,
            //                 OutboundFlightNumber = g.Key.OutboundFlightNumber,
            //                 ArrivalDateTime = g.Max(x => x.s.ArrivalDateTime),
            //                 Destination = g.Key.Destination,
            //                 PlaceOfDep = g.Key.POD,
            //                 MunazzimId = g.Key.MunazzimId,
            //                 TotalQuota = g.Key.TotalQuota,
            //                 AccomodationCategoryId = g.Key.AccomodationCategoryId
            //             };


            // ====================== JAHANZEB'S QUERY END ======================

            //var query1 = from fh in _context.FamilyHeads
            //             join p in _context.Packages on fh.PackageId equals p.PackageId
            //             join c in _context.Companies on p.CompanyId equals c.CompanyId
            //             join mc in _context.MunazzimCompanies on c.CompanyId equals mc.CompanyId
            //             join m in _context.Munazzims on mc.MunazzimId equals m.MunazzimId
            //             join pnrm in _context.PNRMembers on fh.FamilyHeadId equals pnrm.FamilyHeadId
            //             join pnr in _context.PNRs on pnrm.PNRId equals pnr.PNRId
            //             join f in _context.Flights on pnr.FlightId equals f.FlightId
            //             join s in _context.Sectors on f.FlightId equals s.FlightId
            //             join cl in _context.CompanyLicenses on c.CompanyId equals cl.CompanyId
            //             where !fh.Trashed
            //                 && !c.Trashed
            //                 && !f.Trashed
            //                 && !pnr.Trashed
            //                 && s.ArrivalDateTime == _context.Sectors
            //                     .Where(sector => sector.FlightId == f.FlightId && sector.FlightType == (FlightType)1)
            //                     .Max(sector => sector.ArrivalDateTime)
            //                 && f.FlightId == id
            //             group new { m.MunazzimId, c.CompanyName, cl.CompanyLicense, m.Name, m.ClusterNumber, p.PackageNumber, f.OutboundFlightNumber, s.ArrivalDateTime, s.Destination, fh.POD } by new
            //             {
            //                 c.CompanyName,
            //                 cl.CompanyLicense,
            //                 m.Name,
            //                 m.ClusterNumber,
            //                 p.PackageNumber,
            //                 f.OutboundFlightNumber,
            //                 s.ArrivalDateTime,
            //                 s.Destination,
            //                 fh.POD,
            //                 m.MunazzimId
            //             } into grouped
            //             select new
            //             {
            //                 CompanyName = grouped.Key.CompanyName,
            //                 EnrollmentNo = grouped.Key.CompanyLicense,
            //                 MunazzimName = grouped.Key.Name,
            //                 MunazzimNumber = grouped.Key.ClusterNumber,
            //                 MoraRegistrationNo = grouped.Key.PackageNumber,
            //                 OutboundFlightNumber = grouped.Key.OutboundFlightNumber,
            //                 ArrivalDateTime = grouped.Key.ArrivalDateTime,
            //                 Destination = grouped.Key.Destination,
            //                 PlaceOfDep = grouped.Key.POD,
            //                 MunazimId = grouped.Key.MunazzimId
            //             };

            var result1 = query1.ToList();



            int munazimid = 0;

            foreach (var item in result1)
            {
                ReportingFormModel model = new ReportingFormModel();
                model.EnrollmentNo = item.EnrollmentNo;
                model.MunazzimName = item.MunazzimName;
                model.MunazzimNo = item.MunazzimNumber;
                model.MoraRegistrationNo = item.MoraRegistrationNo;
                model.OutboundFlightNumber = item.OutboundFlightNumber;
                model.ArrivalDate = item.ArrivalDateTime?.ToString("dd/MMM/yyyy");
                model.ArrivalTime = item.ArrivalDateTime?.ToString("h:mm tt");
                model.PlaceOfDep = item.PlaceOfDep;
                model.ArrivalAt = item.Destination.Contains("MED") ? "MADINAH" : "JEDDAH";
                model.HujajNo = item.FamilyHeadCount;
                model.CompanyName = item.CompanyName;
                model.Quota = item.TotalQuota;
                model.ACcomodationCategory = item.AccomodationCategoryId;
                munazimid = item.MunazzimId;


                //for hotel addresses
                //var query2 = from fh in _context.FamilyHeads
                //             join p in _context.Packages on fh.PackageId equals p.PackageId
                //             join pa in _context.PackageAccomodtions on p.PackageId equals pa.PackageId
                //             join h in _context.Hotels on pa.HotelId equals h.HotelId
                //             where fh.FamilyHeadId == id
                //             orderby h.PlaceId
                //             select new
                //             {
                //                 h.HotelName,
                //                 h.HotelAddress
                //             };

                var query2 = (from f in _context.Flights
                             join pnr in _context.PNRs on f.FlightId equals pnr.FlightId
                             join pnrm in _context.PNRMembers on pnr.PNRId equals pnrm.PNRId
                             join fh in _context.FamilyHeads on pnrm.FamilyHeadId equals fh.FamilyHeadId
                             join p in _context.Packages on fh.PackageId equals p.PackageId
                             join pa in _context.PackageAccomodtions on p.PackageId equals pa.PackageId
                             join h in _context.Hotels on pa.HotelId equals h.HotelId
                             where f.FlightId == id
                             select new { h.HotelName, h.HotelAddress, h.PlaceId, pa.AccomodationName })
                                .Distinct()
                                .OrderBy(h => h.PlaceId)
                                .ToList();

                var result2 = query2.ToList();
                for (int i = 0; i < result2.Count; i++)
                {
                    if ( result2[i].PlaceId == 1) //i == 0 &&
                    {
                        model.HotelNameMakkah = result2[i].HotelName;
                        model.HotelMakkahAddress = result2[i].HotelAddress;
                    }
                    else if ( result2[i].PlaceId == 2) //i == 1 &&
                    {
                        model.HotelNameMedinah = result2[i].HotelName;
                        model.HotelMedinahAddress = result2[i].HotelAddress;
                    }
                    else if (result2[i].PlaceId == 3)
                    {
                        model.HotelNameMina = result2[i].HotelName;
                    }
                    else if (result2[i].PlaceId == 4)
                    {
                        model.HotelNameAzizia = result2[i].HotelName;
                    }

                }
                for (int i = 0;i < result2.Count;i++)
                {
                    if (result2[i].PlaceId == 3)
                    {
                        model.MaktabNo = result2[i].AccomodationName.Contains("-A") ? "1" :"3";
                    }
                }


                //for quota munazim
                //var query3 = from m in _context.Munazzims
                //             join mc in _context.MunazzimCompanies on m.MunazzimId equals mc.MunazzimId
                //             join c in _context.Companies on mc.CompanyId equals c.CompanyId
                //             join p in _context.Packages on c.CompanyId equals p.CompanyId
                //             join fh in _context.FamilyHeads on p.PackageId equals fh.PackageId
                //             where !fh.Trashed && !p.Trashed && !c.Trashed && !m.Trashed && m.MunazzimId == munazimid
                //             group new { CompanyName = m.Name, c.Quota } by new { m.Name } into grouped
                //             select new
                //             {
                //                 CompanyName = grouped.Key.Name,
                //                 Quota = grouped.Sum(g => g.Quota)
                //             };

                //var result3 = query3.ToList();
                //foreach (var item3 in result3)
                //{
                //    model.Quota = item3.Quota;
                //}

                // for CompanyName
                //var query4 = from m in _context.Munazzims
                //             join mc in _context.MunazzimCompanies on m.MunazzimId equals mc.MunazzimId
                //             join c in _context.Companies on mc.CompanyId equals c.CompanyId
                //             join p in _context.Packages on c.CompanyId equals p.CompanyId
                //             join fh in _context.FamilyHeads on p.PackageId equals fh.PackageId
                //             where !fh.Trashed && !p.Trashed && !c.Trashed && !m.Trashed && fh.FamilyHeadId == id
                //             group c by new { c.CompanyName, c.Quota } into grouped
                //             select new
                //             {
                //                 CompanyName = grouped.Key.CompanyName,
                //                 Quota = grouped.Key.Quota
                //             };

                //var result4 = query4.ToList();
                //foreach (var item4 in result4)
                //{
                //    //model.CompanyName = item4.CompanyName;
                //}

                modellist.Add(model);
            }

            return View(modellist);
        }

    }
    public class ReportingFormModel
    {
        public string EnrollmentNo { get; set; }
        public string ArrivalAt { get; set; }
        public string MunazzimName { get; set; }
        public string CompanyName { get; set; }
        public int? MunazzimNo { get; set; }
        public int MoraRegistrationNo { get; set; }
        public string OutboundFlightNumber { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Destination { get; set; }
        public string PlaceOfDep { get; set; }
        public string HotelNameMakkah { get; set; }
        public string HotelMakkahAddress { get; set; }
        public string HotelNameMedinah { get; set; }
        public string HotelMedinahAddress { get; set; }
        public string HotelNameAzizia { get; set; }
        public string HotelNameMina { get; set; }
        public int Quota { get; set; }
        public int HujajNo { get; set; }
        public int? ACcomodationCategory { get; set; }
        public string MaktabNo { get; set; }

    }
}
