using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.PackageModels;

namespace TMG.ManagerClasses;

public static class PackagePriceManager
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="familyHeadId"></param>
    /// <returns></returns>
    public static float GetTotalBasePrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        float convertedAsPerBaseCurrency = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            price += member.FamilyHead.IsAdult ?
                p.AdultSar : member.FamilyHead.IsChild ?
                p.ChildSar : p.InfantSar;

        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalBasePriceForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        float price = 0;
        float convertedAsPerBaseCurrency = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            price += member.FamilyHead.IsAdult ?
                p.AdultSar : member.FamilyHead.IsChild ?
                p.ChildSar : p.InfantSar;

        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalFlightPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        float convertedAsPerBaseCurrency = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            if (member.FamilyHead.FlightAsPerPackage)
            {
                price += dbContext.PackageFlights
                                .Where(e => e.PackageId == p.PackageId)
                                .Sum(e => e.TicketAmount) ?? 0f;
            }
            else
            {

                price += dbContext.FamilyHeadFlight
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.TicketStatus == TicketStatus.Active && e.FlightClass != FlightClass.Self)
                            .Sum(e => e.Total) ?? 0f;
            }
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalFlightPriceForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        float price = 0;
        float convertedAsPerBaseCurrency = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            if (member.FamilyHead.FlightAsPerPackage)
            {
                price += dbContext.PackageFlights
                                .Where(e => e.PackageId == p.PackageId)
                                .Sum(e => e.TicketAmount) ?? 0f;
            }
            else
            {

                price += dbContext.FamilyHeadFlight
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.TicketStatus == TicketStatus.Active && e.FlightClass != FlightClass.Self)
                            .Sum(e => e.Total) ?? 0f;
            }
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalTrainPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            if (member.FamilyHead.TrainAsPerPackage)
            {
                price += dbContext.PackageTrains
                                .Where(e => e.PackageId == p.PackageId)
                                .Sum(e => e.TicketAmount) ?? 0f;
            }
            else
            {

                price += dbContext.FamilyHeadTrain
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                            .Sum(e => e.Total) ?? 0f;
            }
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalTrainPriceForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        float price = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            if (member.FamilyHead.TrainAsPerPackage)
            {
                price += dbContext.PackageTrains
                                .Where(e => e.PackageId == p.PackageId)
                                .Sum(e => e.TicketAmount) ?? 0f;
            }
            else
            {

                price += dbContext.FamilyHeadTrain
                            .Where(e => e.FamilyHeadId == member.FamilyHeadId && e.TicketStatus == TicketStatus.Active)
                            .Sum(e => e.Total) ?? 0f;
            }
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalRoomPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        if (head == null || head.FamilyHead == null)
            return 0;

        Package p = dbContext.Packages
            .FirstOrDefault(e => e.PackageId == head.FamilyHead.PackageId);

        if (p == null)
            return 0;

        foreach (var member in members)
        {
            var facilitiesFromPackage = dbContext.PackageCategoryFacilities
                .Include(e => e.IndividualFacilityDescription)
                .Where(e => e.PackageId == p.PackageId)
                .ToList();

            var facilitiesFromCustom = dbContext.FamilyHeadAccomodations
                .Include(e => e.IndividualFacilityDescription)
                .Where(e => e.FamilyHeadId == member.FamilyHeadId)
                .ToList();

            bool hasSharing = facilitiesFromCustom.Any(f => f.IndividualFacilityDescription?.IsSharing == true);

            if (member.FamilyHead.AccomodationAsPerPackage || hasSharing)
            {
                price += facilitiesFromPackage.Sum(e => e.IndividualFacilityDescription?.Price ?? 0);
            }
            else
            {
                price += facilitiesFromCustom.Sum(e => e.IndividualFacilityDescription?.Price ?? 0);
            }
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalRoomPriceForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        float price = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages
            .FirstOrDefault(e => e.PackageId == head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            var facilitiesFromPackage = dbContext.PackageCategoryFacilities
                .Include(e => e.IndividualFacilityDescription)
                .Where(e => e.PackageId == p.PackageId)
                .ToList();

            var facilitiesFromCustom = dbContext.FamilyHeadAccomodations
                .Include(e => e.IndividualFacilityDescription)
                .Where(e => e.FamilyHeadId == member.FamilyHeadId)
                .ToList();

            bool hasSharing = facilitiesFromCustom.Any(f => f.IndividualFacilityDescription?.IsSharing == true);

            if (member.FamilyHead.AccomodationAsPerPackage || hasSharing)
            {
                price += facilitiesFromPackage.Sum(e => e.IndividualFacilityDescription?.Price ?? 0);
            }
            else
            {
                price += facilitiesFromCustom.Sum(e => e.IndividualFacilityDescription?.Price ?? 0);
            }
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalAddonsPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            price += dbContext.FamilyHeadExtras
                .Where(e => e.FamilyHeadId == member.FamilyHeadId)
                .Sum(e => e.Price) ?? 0f;
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalAddonsPriceForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        float price = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        Package p = dbContext.Packages.Find(head.FamilyHead.PackageId);

        foreach (var member in members)
        {
            price += dbContext.FamilyHeadExtras
                .Where(e => e.FamilyHeadId == member.FamilyHeadId)
                .Sum(e => e.Price) ?? 0f;
        }

        return Convert(head.FamilyHead.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetTotalAddonsDiscount(TMGDbContext dbContext, int familyHeadId)
    {
        float totalDiscount = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        foreach (var member in members)
        {
            var addonPrices = dbContext.FamilyHeadExtras
                .Where(e => e.FamilyHeadId == member.FamilyHeadId)
                .ToList();

            foreach (var addon in addonPrices)
            {
                totalDiscount += addon.Discount ?? 0f;
            }
        }

        return totalDiscount;
    }

    public static float GetTotalAddonsDiscountForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        float totalDiscount = 0;

        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        foreach (var member in members)
        {
            var addonPrices = dbContext.FamilyHeadExtras
                .Where(e => e.FamilyHeadId == member.FamilyHeadId)
                .ToList();

            foreach (var addon in addonPrices)
            {
                totalDiscount += addon.Discount ?? 0f;
            }
        }

        return totalDiscount;
    }

    public static float GetOverallTotalPrice(TMGDbContext dbContext, int familyHeadId)
    {
        return GetTotalBasePrice(dbContext, familyHeadId)
            + GetTotalFlightPrice(dbContext, familyHeadId)
            + GetTotalTrainPrice(dbContext, familyHeadId)
            + GetTotalRoomPrice(dbContext, familyHeadId)
            + GetTotalAddonsPrice(dbContext, familyHeadId)
            - GetTotalAddonsDiscount(dbContext, familyHeadId)
            - GetTotalDiscount(dbContext, familyHeadId);
    }

    public static float GetOverallTotalPriceForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        return GetTotalBasePriceForAll(dbContext, familyHeadId)
            + GetTotalFlightPriceForAll(dbContext, familyHeadId)
            + GetTotalTrainPriceForAll(dbContext, familyHeadId)
            + GetTotalRoomPriceForAll(dbContext, familyHeadId)
            + GetTotalAddonsPriceForAll(dbContext, familyHeadId)
            - GetTotalAddonsDiscountForAll(dbContext, familyHeadId)
            - GetTotalDiscountForAll(dbContext, familyHeadId);
    }

    public static float GetTotalDiscount(TMGDbContext dbContext, int familyHeadId)
    {
        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed && e.FamilyHead.HajjiBookingStatus != HajjiBookingStatus.Cancelled)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        float discount = members.Sum(e => e.FamilyHead.GeneralDiscount) ?? 0f;

        return discount;
    }

    public static float GetTotalDiscountForAll(TMGDbContext dbContext, List<int> familyHeadId)
    {
        var members = dbContext.FamilyMembers
            .Include(e => e.FamilyHead)
            .Where(e => familyHeadId.Contains(e.HeadId) && !e.FamilyHead.Trashed)
            .ToList();

        var head = members.ElementAtOrDefault(0);

        float discount = members.Sum(e => e.FamilyHead.GeneralDiscount) ?? 0f;

        return discount;
    }

    public static float Convert(Currencies baseCurrency, float price, Package package)
    {
        switch (baseCurrency)
        {
            case Currencies.PKR:
                return price * package.PKRROE;
            case Currencies.SAR:
                return price;
            case Currencies.USD:
                return price / package.USDROE;
        }

        return price;
    }

    #region Individual Updated

    public static float GetIndividualBasePrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        var member = dbContext.FamilyHeads
            .Include(e => e.Package)
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        Package p = dbContext.Packages.Find(member.Package.PackageId);

        if (member != null)
        {
            price = member.IsAdult ? p.AdultSar :
                    member.IsChild ? p.ChildSar : p.InfantSar;
        }

        return Convert(member.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetIndividualRoomPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        var head = dbContext.FamilyHeads
            .Include(e => e.Package)
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        Package p = dbContext.Packages
            .FirstOrDefault(e => e.PackageId == head.PackageId);

        if (head != null)
        {

            if (head.AccomodationAsPerPackage)
            {
                price = dbContext.PackageCategoryFacilities
                    .Include(e => e.IndividualFacilityDescription)
                    .Where(e => e.PackageId == p.PackageId)
                    .Sum(e => e.IndividualFacilityDescription.Price);
            }
            else
            {
                price = dbContext.FamilyHeadAccomodations
                    .Include(e => e.IndividualFacilityDescription)
                    .Where(e => e.FamilyHeadId == familyHeadId)
                    .Sum(e => e.IndividualFacilityDescription.Price);
            }
        }

        return Convert(head.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetIndividualFlightPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        var head = dbContext.FamilyHeads
            .Include(e => e.Package)
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        Package p = dbContext.Packages.Find(head.PackageId);

        if (head != null)
        {

            if (head.FlightAsPerPackage)
            {
                price = dbContext.PackageFlights
                    .Where(e => e.PackageId == p.PackageId)
                    .Sum(e => e.TicketAmount) ?? 0f;
            }
            else
            {
                price = dbContext.FamilyHeadFlight
                    .Where(e => e.FamilyHeadId == familyHeadId && e.TicketStatus == TicketStatus.Active && e.FlightClass != FlightClass.Self)
                    .Sum(e => e.Total) ?? 0f;
            }
        }

        return Convert(head.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetIndividualTrainPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        var head = dbContext.FamilyHeads
            .Include(e => e.Package)
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        Package p = dbContext.Packages.Find(head.PackageId);

        if (head != null)
        {

            if (head.TrainAsPerPackage)
            {
                price = dbContext.PackageTrains
                    .Where(e => e.PackageId == p.PackageId)
                    .Sum(e => e.TicketAmount) ?? 0f;
            }
            else
            {
                price = dbContext.FamilyHeadTrain
                    .Where(e => e.FamilyHeadId == familyHeadId && e.TicketStatus == TicketStatus.Active)
                    .Sum(e => e.Total) ?? 0f;
            }
        }

        return Convert(head.Currencies ?? Currencies.SAR, price, p);
    }

    public static float GetIndividualAddonsPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float price = 0;
        var head = dbContext.FamilyHeads
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        if (head != null)
        {
            price = dbContext.FamilyHeadExtras
                .Where(e => e.FamilyHeadId == familyHeadId)
                .Sum(e => e.Price) ?? 0f;
        }

        return Convert(head.Currencies ?? Currencies.SAR, price, dbContext.Packages.Find(head.PackageId));
    }
    
    public static float GetIndividualAddonsDiscount(TMGDbContext dbContext, int familyHeadId)
    {
        float totalDiscount = 0;

        var head = dbContext.FamilyHeads
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        if (head != null)
        {
            var addonPrices = dbContext.FamilyHeadExtras
                .Where(e => e.FamilyHeadId == familyHeadId)
                .ToList();

            foreach (var addon in addonPrices)
            {
                totalDiscount += addon.Discount ?? 0f;
            }
        }

        return totalDiscount;
    }
    public static float GetIndividualDiscount(TMGDbContext dbContext, int familyHeadId)
    {
        var head = dbContext.FamilyHeads
            .FirstOrDefault(e => e.FamilyHeadId == familyHeadId);

        return head?.GeneralDiscount ?? 0f;
    }

    public static float GetIndividualTotalPrice(TMGDbContext dbContext, int familyHeadId)
    {
        float basePrice = GetIndividualBasePrice(dbContext, familyHeadId);
        float flightPrice = GetIndividualFlightPrice(dbContext, familyHeadId);
        float trainPrice = GetIndividualTrainPrice(dbContext, familyHeadId);
        float roomPrice = GetIndividualRoomPrice(dbContext, familyHeadId);
        float addonsPrice = GetIndividualAddonsPrice(dbContext, familyHeadId);
        float addonsDiscount = GetIndividualAddonsDiscount(dbContext, familyHeadId);
        float discount = GetIndividualDiscount(dbContext, familyHeadId);

        float totalPrice = basePrice + flightPrice + trainPrice + roomPrice + addonsPrice - addonsDiscount - discount;

        return totalPrice;
    }


    #endregion
}
