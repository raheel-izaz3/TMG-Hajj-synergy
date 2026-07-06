using Microsoft.EntityFrameworkCore;
using TMG.Data;
using TMG.Models;
using TMG.Models.FamilyModels;
using TMG.Models.InvoiceModels;
using TMG.Models.PackageModels;

namespace TMG.ManagerClasses;

public static class PackageManager
{

    public static float TotalPackageAmount(TMGDbContext dbContext, int packageId, int familyHeadId)
    {
        var members = dbContext.FamilyMembers
               .Include(e => e.Head)
               .Include(e => e.FamilyHead)
               .ThenInclude(e => e.FamilyHeadRelation)
               .Include(e => e.FamilyHead)
               .ThenInclude(e => e.Package)
               .ThenInclude(e => e.Company)
               .Include(e => e.FamilyHead)
               .ThenInclude(e => e.SPO)
               .Where(e => e.HeadId == familyHeadId && !e.FamilyHead.Trashed)
               .ToList();

        var head = members.FirstOrDefault(e => e.IsHead);

        var familyHeadTransports = dbContext.FamilyHeadTransports
                                            .Where(e => e.VehicleId != null)
                                            .Select(e => new
                                            {
                                                e.Vehicle.Price,
                                                e.FamilyHead.PackageId,
                                                e.FamilyHead.Trashed,
                                                e.FamilyHeadId,
                                                e.FamilyHead.RouteAsPerPackage
                                            })
                                            .Where(e => !e.Trashed && members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                                            .ToList();

        var familyHeadFlights = dbContext.FamilyHeadFlight
                                    .Select(e => new
                                    {
                                        e.FamilyHeadId,
                                        e.FamilyHead.PackageId,
                                        e.FamilyHead.FlightAsPerPackage,
                                        e.FamilyHead.Trashed,
                                        e.Taxes,
                                        e.Commission,
                                        e.FareAmount, // amount assumed as SAR
                                        e.TicketStatus,
                                        e.Total
                                    })
                                    .Where(e => !e.Trashed && members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                                    .Where(e => !e.FlightAsPerPackage && e.TicketStatus.HasValue && e.TicketStatus == TicketStatus.Active)
                                    .ToList();

        var familyHeadTrains = dbContext.FamilyHeadTrain
                                    .Select(e => new
                                    {
                                        e.FamilyHeadId,
                                        e.FamilyHead.PackageId,
                                        e.FamilyHead.TrainAsPerPackage,
                                        e.FamilyHead.Trashed,
                                        e.Taxes,
                                        e.Commission,
                                        e.FareAmount, // amount assumed as SAR
                                        e.TicketStatus,
                                        e.Total
                                    })
                                    .Where(e => !e.Trashed && members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                                    .Where(e => !e.TrainAsPerPackage && e.TicketStatus.HasValue && e.TicketStatus == TicketStatus.Active)
                                    .ToList();

        var familyHeadAccommodations = dbContext.FamilyHeadAccomodations
                                        .Select(e => new
                                        {
                                            e.FamilyHeadId,
                                            e.FamilyHead.PackageId,
                                            e.FamilyHead.Trashed,
                                            e.IndividualFacilityDescription.Price
                                        })
                                        .Where(e => !e.Trashed && members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                                        .ToList();


        var eiadfs = dbContext.FamilyHeadExtras
                .Where(e => members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                .Select(e => new
                {
                    e.FamilyHeadId,
                    //e.ExtraFacilityDescription.Price,
                    e.Discount,
                })
                .ToList();


        var invoices = dbContext.NewInvoicesWithoutTrash.ToList();

        float totalPaymentReceivedSar = 0;

        float paymentInTrasitSar = 0;

        float basicPackageAmount = 0f, eiadfAmount = 0f, discountAmount = 0f;


        float totalPackageAmount = basicPackageAmount + eiadfAmount - discountAmount;


        float balanceBeforeTrasit = totalPackageAmount - totalPaymentReceivedSar;

        float balanceAfterTrasit = totalPackageAmount - (totalPaymentReceivedSar + paymentInTrasitSar);

        double percOfPaymentReceived = Math.Round((totalPaymentReceivedSar / totalPackageAmount) * 100, 2);

        var packageRooms = dbContext.PackageCategoryFacilities
            .Include(e => e.IndividualFacilityDescription)
            .Where(e => e.PackageId == head.FamilyHead.PackageId);

        var packageFlights = dbContext.PackageFlights
                                .Where(e => e.PackageId == head.FamilyHead.PackageId)
                                .ToList();

        var packageTrains = dbContext.PackageTrains
                                .Where(e => e.PackageId == head.FamilyHead.PackageId)
                                .ToList();

        float newTotal = 0f;
        float newBasic = 0f;
        float newFlightTicket = 0f;
        float newTrainTicket = 0f;
        float newRoomTotal = 0f;
        float newGeneralDiscount = 0f;


        float additionalTrasnportSar = familyHeadTransports.Sum(e => e.Price);


        //var extraPriceSar = eiadfs.Sum(e => e.Price);
        //extraPriceSar -= eiadfs.Sum(e => e.Discount) ?? 0f;

        //return Ok(headMembersEIadfs);

        foreach (var m in members)
        {
            var currency = m.FamilyHead.Currencies ?? Currencies.SAR;
            // Basic Package Amount Calculation
            newBasic += CalculateBasicAmount(m.FamilyHead, head.FamilyHead);

            // Flight Ticket Calculation
            if (m.FamilyHead.FlightAsPerPackage && m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
            {
                newFlightTicket += CalculateTicketAmount(packageFlights.Sum(e => e.TicketAmount) ?? 0f, currency, head.FamilyHead.Package);
            }
            else if (!m.FamilyHead.FlightAsPerPackage && m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
            {
                //newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                //    .Sum(e => e.Commission ?? 0f + e.FareAmount ?? 0f + e.Taxes ?? 0f);

                if (head.FamilyHead.Currencies == Currencies.SAR)
                {
                    newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                        .Sum(e => e.Total ?? 0f);
                }
                else if (head.FamilyHead.Currencies == Currencies.PKR)
                {
                    newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                       .Sum(e => e.Total ?? 0f) * head.FamilyHead.Package.PKRROE;
                }
                else if (head.FamilyHead.Currencies == Currencies.USD)
                {
                    newFlightTicket += familyHeadFlights.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                       .Sum(e => e.Total ?? 0f) * head.FamilyHead.Package.USDROE;
                }
            }

            // Train Ticket Calculation
            if (m.FamilyHead.TrainAsPerPackage)
            {
                newTrainTicket += CalculateTicketAmount(packageTrains.Sum(e => e.TicketAmount) ?? 0f, currency, head.FamilyHead.Package);
            }
            else if (!m.FamilyHead.TrainAsPerPackage)
            {

                if (head.FamilyHead.Currencies == Currencies.SAR)
                {
                    newTrainTicket += familyHeadTrains.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                        .Sum(e => e.Total ?? 0f);
                }
                else if (head.FamilyHead.Currencies == Currencies.PKR)
                {
                    newTrainTicket += familyHeadTrains.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                       .Sum(e => e.Total ?? 0f) * head.FamilyHead.Package.PKRROE;
                }
                else if (head.FamilyHead.Currencies == Currencies.USD)
                {
                    newTrainTicket += familyHeadTrains.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                       .Sum(e => e.Total ?? 0f) * head.FamilyHead.Package.USDROE;
                }
            }

            if (m.FamilyHead.AccomodationAsPerPackage)
            {
                // Room Total Calculation
                newRoomTotal += CalculateTicketAmount(packageRooms.Sum(e => e.IndividualFacilityDescription.Price), currency, head.FamilyHead.Package);
            }
            else
            {

                if (head.FamilyHead.Currencies == Currencies.SAR)
                {
                    newRoomTotal += familyHeadAccommodations.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                        .Sum(e => e.Price);
                }
                else if (head.FamilyHead.Currencies == Currencies.PKR)
                {
                    newRoomTotal += familyHeadAccommodations.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                       .Sum(e => e.Price) * head.FamilyHead.Package.PKRROE;
                }
                else if (head.FamilyHead.Currencies == Currencies.USD)
                {
                    newRoomTotal += familyHeadAccommodations.Where(e => e.FamilyHeadId == m.FamilyHeadId)
                       .Sum(e => e.Price) * head.FamilyHead.Package.USDROE;
                }
            }


            // General Discount will be as per base currency
            newGeneralDiscount += m.FamilyHead.GeneralDiscount ?? 0f;
        }


        newTotal += newBasic + newFlightTicket + newTrainTicket + newRoomTotal - newGeneralDiscount + additionalTrasnportSar;


        return newTotal;
    }

    // Helper Methods
    static float CalculateBasicAmount(FamilyHead member, FamilyHead head)
    {
        return head.Currencies switch
        {
            Currencies.PKR => member.IsAdult ? member.Package.AdultPkr :
                             member.IsChild ? member.Package.ChildPkr :
                             member.Package.InfantPkr,
            Currencies.SAR => member.IsAdult ? member.Package.AdultSar :
                             member.IsChild ? member.Package.ChildSar :
                             member.Package.InfantSar,
            Currencies.USD => member.IsAdult ? member.Package.AdultUsd :
                             member.IsChild ? member.Package.ChildUsd :
                             member.Package.InfantUsd,
            _ => 0f
        };
    }

    static float CalculateTicketAmount(float ticketTotal, Currencies currency, Package package)
    {
        return currency switch
        {
            Currencies.PKR => ticketTotal * package.SARROE,
            Currencies.SAR => ticketTotal,
            Currencies.USD => ticketTotal * package.USDROE,
            _ => 0f
        };
    }

    public static float GetTotalPackageAmountInSarForEntireFamily(TMGDbContext dbContext, int packageId, int familyHeadId)
    {
        float totalPackageAmount = 0;

        Package package = dbContext.Packages
            .AsNoTracking()
            .FirstOrDefault(e => e.PackageId == packageId);

        int headId = dbContext.FamilyMembers
                        .FirstOrDefault(e => e.FamilyHeadId == familyHeadId)
                        .HeadId;

        var members = dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.HeadId == headId && !e.FamilyHead.Trashed)
                .AsNoTracking()
                .ToList();

        var extras = dbContext.FamilyHeadExtras
                    .Where(e => members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                    .ToList();

        totalPackageAmount += extras.Sum(e => e.PriceSar ?? 0f);

        foreach (var item in members)
        {
            if (item.FamilyHead.IsInfant)
            {
                totalPackageAmount += package.InfantSar;
            }
            else if (item.FamilyHead.IsChild)
            {
                totalPackageAmount += package.ChildSar;
            }
            else if (item.FamilyHead.IsAdult)
            {
                totalPackageAmount += package.AdultSar;
            }

            if (!item.FamilyHead.IsTicketExcluded)
            {
                totalPackageAmount += package.FlightTicketPriceSar;
            }
            if (item.FamilyHead.IsTicketIncludedWithDifference)
            {
                totalPackageAmount += item.FamilyHead.FlightTicketDifferencesSar ?? 0f;
            }

            totalPackageAmount -= item.FamilyHead.SarDiscountAmount;
        }

        return totalPackageAmount;
    }

    public static float GetTotalPackageAmountInSarForIndividual(TMGDbContext dbContext, int packageId, int familyHeadId)
    {
        float totalPackageAmount = 0;

        Package package = dbContext.Packages
            .AsNoTracking()
            .FirstOrDefault(e => e.PackageId == packageId);

        int headId = dbContext.FamilyMembers
                        .FirstOrDefault(e => e.FamilyHeadId == familyHeadId)
                        .HeadId;

        var member = dbContext.FamilyHeads
                .FirstOrDefault(e => e.FamilyHeadId == headId);

        var extras = dbContext.FamilyHeadExtras
                    .Where(e => member.FamilyHeadId == headId)
                    .ToList();

        totalPackageAmount += extras.Sum(e => e.PriceSar ?? 0f);

        if (member.IsInfant)
        {
            totalPackageAmount += package.InfantSar;
        }
        else if (member.IsChild)
        {
            totalPackageAmount += package.ChildSar;
        }
        else if (member.IsAdult)
        {
            totalPackageAmount += package.AdultSar;
        }

        if (!member.IsTicketExcluded)
        {
            totalPackageAmount += package.FlightTicketPriceSar;
        }
        if (member.IsTicketIncludedWithDifference)
        {
            totalPackageAmount += member.FlightTicketDifferencesSar ?? 0f;
        }

        totalPackageAmount -= member.SarDiscountAmount;

        return totalPackageAmount;
    }

    public static float GetTotalPackageAmountInSarForIndividualWithoutTicket(TMGDbContext dbContext, int packageId, int familyHeadId)
    {
        float totalPackageAmount = 0;

        Package package = dbContext.Packages
            .AsNoTracking()
            .FirstOrDefault(e => e.PackageId == packageId);


        int headId = dbContext.FamilyMembers
                        .FirstOrDefault(e => e.FamilyHeadId == familyHeadId)
                        .HeadId;

        var member = dbContext.FamilyHeads
                .FirstOrDefault(e => e.FamilyHeadId == headId);

        var extras = dbContext.FamilyHeadExtras
                    .Where(e => member.FamilyHeadId == headId)
                    .ToList();

        totalPackageAmount += extras.Sum(e => e.PriceSar ?? 0f);

        if (member.IsInfant)
        {
            totalPackageAmount += package.InfantSar;
        }
        else if (member.IsChild)
        {
            totalPackageAmount += package.ChildSar;
        }
        else if (member.IsAdult)
        {
            totalPackageAmount += package.AdultSar;
        }

        totalPackageAmount -= member.SarDiscountAmount;

        return totalPackageAmount;
    }

    public static float GetTotalPackageAmountInSarForEntireFamilyWithoutTicket(TMGDbContext dbContext, int packageId, int familyHeadId)
    {
        float totalPackageAmount = 0;

        Package package = dbContext.Packages
            .AsNoTracking()
            .FirstOrDefault(e => e.PackageId == packageId);

        int headId = dbContext.FamilyMembers
                        .FirstOrDefault(e => e.FamilyHeadId == familyHeadId)
                        .HeadId;

        var members = dbContext.FamilyMembers
                .Include(e => e.FamilyHead)
                .Where(e => e.HeadId == headId && !e.FamilyHead.Trashed)
                .AsNoTracking()
                .ToList();

        var extras = dbContext.FamilyHeadExtras
                    .Where(e => members.Select(e => e.FamilyHeadId).Contains(e.FamilyHeadId))
                    .ToList();

        totalPackageAmount += extras.Sum(e => e.PriceSar ?? 0f);

        foreach (var item in members)
        {
            if (item.FamilyHead.IsInfant)
            {
                totalPackageAmount += package.InfantSar;
            }
            else if (item.FamilyHead.IsChild)
            {
                totalPackageAmount += package.ChildSar;
            }
            else if (item.FamilyHead.IsAdult)
            {
                totalPackageAmount += package.AdultSar;
            }

            totalPackageAmount -= item.FamilyHead.SarDiscountAmount;
        }

        return totalPackageAmount;
    }
}
