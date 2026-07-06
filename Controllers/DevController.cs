using TMG.Data;
using TMG.Models.AccountModels;
using TMG.Models.FamilyModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMG.ManagerClasses;
using static System.Net.WebRequestMethods;
using TMG.Models.InvoiceModels;
using TMG.Models.Updated_Accounts;
using TMG.Models;

namespace TMG.Controllers
{
    public class DevController : Controller
    {
        private readonly TMGDbContext _context;
        private readonly IHttpContextAccessor _http;

        public DevController(TMGDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public IActionResult AddReceivableEntries()
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var members = _context.FamilyHeadsWithoutTrash
                    .Where(e => e.IsHead && e.HajjSeasonsId == 2).ToList();

                foreach (var member in members)
                {
                    GeneralLedger generalLedger = new()
                    {
                        Date = member.CreatedAt,
                    };
                    _context.GeneralLedgers.Add(generalLedger);
                    _context.SaveChanges();


                    float packageAmount = PackagePriceManager.GetOverallTotalPrice(_context, member.FamilyHeadId);

                    GeneralLedgerEntry debitEntry = new GeneralLedgerEntry()
                    {
                        Description = $"{member.GivenName} {member.SurName} ({member.FcId}) - Receivable ({Currencies.SAR})",
                        AccountNature = AccountNature.Debit,
                        GeneralLedgerId = generalLedger.GeneralLedgerId,
                        Amount = packageAmount,
                        AccountsId = member.ReceivableAccountsId ?? 0
                    };
                    GeneralLedgerEntry creditEntry = new GeneralLedgerEntry()
                    {
                        Description = $"{member.GivenName} {member.SurName} ({member.FcId}) - Service Revenue ({Currencies.SAR})",
                        AccountNature = AccountNature.Credit,
                        GeneralLedgerId = generalLedger.GeneralLedgerId,
                        Amount = packageAmount,
                        AccountsId = member.RevenueAccountsId ?? 0
                    };

                    _context.GeneralLedgerEntries.Add(debitEntry);
                    _context.GeneralLedgerEntries.Add(creditEntry);
                    _context.SaveChanges();

                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }

            return Ok("Done!");
        }


        public IActionResult AddFamilyHeadCOA()
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var members = _context.FamilyHeadsWithoutTrash
                    .Include(e => e.Package)
                    .Where(e => e.IsHead).ToList();

                foreach (var m in members)
                {
                    int? ReceId = null;
                    int? RevenId = null;

                    var AccGroupRevenue = _context.AccountGroups.Where(g => g.Name == COAManager.ServiceRevenueString).FirstOrDefault();

                    var AccountRevenue = new Accounts()
                    {
                        AccountGroupsId = AccGroupRevenue.AccountGroupId,
                        Name = $"{m.GivenName} {m.SurName} ({m.FcId}) - {AccGroupRevenue.Name} ({Currencies.SAR})",
                        CompanyId = m.Package.CompanyId,

                    };

                    _context.Account.Add(AccountRevenue);
                    _context.SaveChanges();
                    m.RevenueAccountsId = AccountRevenue.AccountsId;



                    var AccGroupReceivable = _context.AccountGroups.Where(g => g.Name == COAManager.CurrentAssetsString).FirstOrDefault();

                    var AccountRec = new Accounts()
                    {
                        AccountGroupsId = AccGroupReceivable.AccountGroupId,
                        Name = $"{m.GivenName} {m.SurName} ({m.FcId}) - Receivable ({Currencies.SAR})",
                        CompanyId = m.Package.CompanyId,
                        IsHajjiAccount = true
                    };

                    _context.Account.Add(AccountRec);
                    _context.SaveChanges();

                    m.ReceivableAccountsId = AccountRec.AccountsId;
                    _context.SaveChanges();


                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Ok("Error!");
            }

            return Ok("Done!");
        }

        public IActionResult AddCompanyBankAccountCOA()
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var accounts = _context.CompanyBankAccountsWithoutTrash.ToList();

                foreach (CompanyBankAccount acc in accounts)
                {

                    int? AccAssetId = null;
                    int? AccExpId = null;

                    var AccGroupRev = _context.AccountGroups.Where(g => g.Name == COAManager.CurrentAssetsString).FirstOrDefault();
                    var AccGroupBankExp = _context.AccountGroups.Where(g => g.Name == COAManager.BankExpenseString).FirstOrDefault();
                    var BankName = _context.BanksWithoutTrash.Where(e => e.BankId == acc.BankId).FirstOrDefault();

                    if (AccGroupRev != null)
                    {
                        var AccountRev = new Accounts()
                        {
                            AccountGroupsId = AccGroupRev.AccountGroupId,
                            Name = $" {BankName.Name} - {acc.Title} ({acc.AccountCurrency})",
                            CompanyId = acc.CompanyId,
                            IsBank = true
                        };

                        _context.Account.Add(AccountRev);
                        _context.SaveChanges();

                        AccAssetId = AccountRev.AccountsId;
                    }

                    if (AccGroupBankExp != null)
                    {
                        var AccountExp = new Accounts()
                        {
                            AccountGroupsId = AccGroupBankExp.AccountGroupId,
                            Name = $" {BankName.Name} - {acc.Title} ({acc.AccountCurrency})",
                            CompanyId = acc.CompanyId
                        };

                        _context.Account.Add(AccountExp);
                        _context.SaveChanges();

                        AccExpId = AccountExp.AccountsId;
                    }

                    acc.BankAssetAccountId = AccAssetId;
                    acc.BankExpenseAccountId = AccExpId;
                    
                    _context.SaveChanges();

                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Ok("Error!");
            }

            return Ok("Done!");
        }

        //public IActionResult AddNewReceipts()
        //{
        //    var oldInvoices = _context.Invoices
        //        .Where(e => e.FamilyHeadId != null && e.BankId != null)
        //        .ToList();

        //    var transaction = _context.Database.BeginTransaction();

        //    try
        //    {
        //        foreach (var invoice in oldInvoices)
        //        {
        //            NewInvoice newInvoice = new NewInvoice
        //            {
        //                FamilyHeadId = (int)invoice.FamilyHeadId,
        //                //BankId = (int)invoice.BankId,
        //                AmountRecieved = invoice.AmountRecived,
        //                Remarks = "",
        //                ClearanceDate = invoice.ClearanceDate,
        //                DepositDate = invoice.DepositDate,  
        //                HajjSeasonsId = invoice.HajjSeasonsId,
        //                ImageBase64 = invoice.ImageBase64,
        //                ImagePath = invoice.ImagePath,
        //                InstrumentNo = invoice.Instrument,
        //                ProformaNumber = invoice.ProformaNumber,
        //                Trashed = invoice.Trashed,
        //                Date = invoice.Date,
        //                InvoiceStatus = invoice.InvoiceStatus,
        //                ModeOfPayment = invoice.ModeOfPayment,
        //            };

        //            //switch (invoice.Currency)
        //            //{
        //            //    case Models.Currencies.PKR:
        //            //        newInvoice.CurrencyId = 2;
        //            //        newInvoice.ROE = invoice.PKRROE;
        //            //        break;
        //            //    case Models.Currencies.SAR:
        //            //        newInvoice.CurrencyId = 1;
        //            //        newInvoice.ROE = invoice.SARROE;
        //            //        break;
        //            //    case Models.Currencies.USD:
        //            //        newInvoice.CurrencyId = 3;
        //            //        newInvoice.ROE = invoice.USDROE;
        //            //        break;
        //            //}

        //            _context.NewInvoice.Add(newInvoice);
        //        }
        //        _context.SaveChanges();

        //        transaction.Commit();
        //        return Ok("Data Inserted");
        //    }
        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        return BadRequest("Error");
        //    }
        //}

        /// <summary>
        /// Update all untrashed families' Individual Ids
        /// </summary>
        /// <returns></returns>
        public IActionResult IndIdUpdate()
        {
            var familyheads = _context.FamilyHeads.ToList();

            for (int i = 0; i < familyheads.Count; i++)
            {
                familyheads.ElementAt(i).IndId = (i + 1).ToString();
            }

            _context.SaveChanges();
            _context.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault().IndividualCount = familyheads.Count;
            _context.SaveChanges();


            return Ok(new
            {
                msg = "Updated!"
            });
        }

        /// <summary>
        /// Update all untrashed families' FC IDs
        /// </summary>
        /// <returns></returns>
        public IActionResult FamilyHeadIdUpdate()
        {
            var heads = _context.FamilyHeads
            .OrderBy(e => e.FamilyHeadId)
            .Where(e => e.IsHead).ToList();

            for (int i = 0; i < heads.Count; i++)
            {
                heads.ElementAt(i).FcId = $"HB-{i + 1}";
            }

            _context.SaveChanges();

            _context.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault().FamilyCount = heads.Count;
            _context.SaveChanges();


            return Ok(new
            {
                msg = "Updated!"
            });
        }

        /// <summary>
        /// Updates All Family Heads Account with the updated FcIds.
        /// Run this function after FamilyHeadIdUpdate Function
        /// </summary>
        /// <returns></returns>
        public IActionResult UpdateAccountNames()
        {
            var accounts = _context.Accounts
                .Where(e => e.IsReceivable)
                .ToList();

            var fh = _context.FamilyHeads.Where(e => e.AccountId != null).ToList();

            foreach (var a in accounts)
            {
                var currFh = fh.FirstOrDefault(e => e.AccountId == a.AccountId);
                a.Name = $"{currFh.FullName} ({currFh.FcId})";
            }

            _context.SaveChanges();


            return Ok(new
            {
                msg = "Updated!"
            });
        }

        public IActionResult UpdateMemberFcIds()
        {

            var members = _context.FamilyMembers.ToList();
            var families = _context.FamilyHeads.ToList();

            for (int i = 0; i < members.Count; i++)
            {
                if (!members.ElementAt(i).IsHead)
                    members.ElementAt(i).FamilyHead.FcId =
                    members.ElementAt(i).Head.FcId;
            }
            _context.SaveChanges();

            return Ok(new
            {
                msg = "Updated!"
            });
        }

        //public IActionResult UpdateAccountReceivables()
        //{
        //    var accountIds = _context
        //                    .FamilyHeads
        //                    .Where(e => e.AccountId != null)
        //                    .Select(e => e.AccountId)
        //                    .ToList();

        //    _context.Accounts
        //        .Where(e => accountIds.Contains(e.AccountId))
        //        .ExecuteUpdate(e => e.SetProperty(prop => prop.IsReceivable, true));

        //    return Ok(new
        //    {
        //        msg = "Updated!"
        //    });
        //}

        public IActionResult UpdateProformaNumber()
        {
            var a = _context.Invoices.ToList();
            int i = 1;
            foreach (var _ in a)
            {
                _.ProformaNumber = i;
                i++;
            }
            _context.SaveChanges();
            return Ok("Updated");
        }

        /// <summary>
        /// Update The Amount Receivable of All untrashed Family Heads
        /// </summary>
        /// <returns></returns>
        public IActionResult UpdateReceivables()
        {
            var heads = _context.FamilyMembers
                    .Include(e => e.FamilyHead)
                    .Where(e => e.IsHead && !e.FamilyHead.Trashed)
                    .ToList();

            foreach (var h in heads)
            {
                var members = _context.FamilyMembers
                    .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.Package)
                    .Where(e => e.HeadId == h.HeadId)
                    .ToList();

                float packageAmount = 0;
                foreach (var m in members)
                {
                    var addFacs = _context.FamilyHeadAdditionalFacilities
                        .Include(e => e.AdditionalFacilities)
                        .Where(e => e.FamilyHeadId == m.FamilyHeadId).ToList();
                    if (m.FamilyHead.Age < 2)
                    {
                        packageAmount += m.FamilyHead.Package.InfantPkr;
                        packageAmount += addFacs.Sum(e => e.AdditionalFacilities.InfantPkr) ?? 0;
                    }
                    else if (m.FamilyHead.Age < 12)
                    {
                        packageAmount += m.FamilyHead.Package.ChildPkr;
                        packageAmount += addFacs.Sum(e => e.AdditionalFacilities.ChildPkr) ?? 0;

                    }
                    else if (m.FamilyHead.Age >= 12)
                    {
                        packageAmount += m.FamilyHead.Package.AdultPkr;
                        packageAmount += addFacs.Sum(e => e.AdditionalFacilities.AdultPkr) ?? 0;

                    }
                    packageAmount -= m.FamilyHead.PkrDiscountAmount;

                    if (m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
                    {
                        packageAmount += m.FamilyHead.Package.FlightTicketPricePkr;

                        if (m.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
                        {
                            packageAmount += m.FamilyHead.FlightTicketDifferencesPkr ?? 0;
                        }
                    }

                    packageAmount += _context.FamilyHeadExtras.Where(e => e.FamilyHeadId == m.FamilyHeadId).Sum(e => e.PricePkr) ?? 0;
                }

                int? accountId = h.FamilyHead.AccountId;

                packageAmount -= 1;

                PaymentVoucher voucher = _context.PaymentVouchersWithoutTrash
                            .Where(e => e.AccountId == accountId)
                            .OrderBy(e => e.PaymentVoucherId)
                            .FirstOrDefault();

                voucher.Amount = packageAmount;
                _context.SaveChanges();

                var voucherDebitEntry = _context.VoucherEntries
                                    .OrderBy(e => e.PaymentVoucherId)
                                    .FirstOrDefault(e => e.PaymentVoucherId == voucher.PaymentVoucherId
                                    && e.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Debit);

                var voucherCreditEntry = _context.VoucherEntries
                                    .OrderBy(e => e.PaymentVoucherId)
                                    .FirstOrDefault(e => e.PaymentVoucherId == voucher.PaymentVoucherId
                                    && e.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Credit);

                voucherDebitEntry.Debit = packageAmount;
                voucherDebitEntry.Credit = packageAmount;

                _context.SaveChanges();

            }

            return Ok("Updated");
        }


        //public IActionResult UpdateReceivables(int id)
        //{
        //    var heads = _context.FamilyMembers
        //            .Include(e => e.FamilyHead)
        //            .Where(e => e.IsHead && !e.FamilyHead.Trashed && e.FamilyHeadId == id)
        //            .ToList();

        //    foreach (var h in heads)
        //    {
        //        var members = _context.FamilyMembers
        //            .Include(e => e.FamilyHead)
        //            .ThenInclude(e => e.Package)
        //            .Where(e => e.HeadId == h.HeadId)
        //            .ToList();

        //        float packageAmount = 0;
        //        foreach (var m in members)
        //        {
        //            var addFacs = _context.FamilyHeadAdditionalFacilities
        //                .Include(e => e.AdditionalFacilities)
        //                .Where(e => e.FamilyHeadId == m.FamilyHeadId).ToList();
        //            if (m.FamilyHead.Age < 2)
        //            {
        //                packageAmount += m.FamilyHead.Package.InfantPkr;
        //                packageAmount += addFacs.Sum(e => e.AdditionalFacilities.InfantPkr) ?? 0;
        //            }
        //            else if (m.FamilyHead.Age < 12)
        //            {
        //                packageAmount += m.FamilyHead.Package.ChildPkr;
        //                packageAmount += addFacs.Sum(e => e.AdditionalFacilities.ChildPkr) ?? 0;

        //            }
        //            else if (m.FamilyHead.Age >= 12)
        //            {
        //                packageAmount += m.FamilyHead.Package.AdultPkr;
        //                packageAmount += addFacs.Sum(e => e.AdditionalFacilities.AdultPkr) ?? 0;

        //            }
        //            packageAmount -= m.FamilyHead.PkrDiscountAmount;

        //            if (m.FamilyHead.FlightTicketType != FlightTicketType.Excluded)
        //            {
        //                packageAmount += m.FamilyHead.Package.FlightTicketPricePkr ?? 0;

        //                if (m.FamilyHead.FlightTicketType == FlightTicketType.IncludedWithDifferences)
        //                {
        //                    packageAmount += m.FamilyHead.FlightTicketDifferencesPkr ?? 0;
        //                }
        //            }

        //            packageAmount += _context.FamilyHeadExtras.Where(e => e.FamilyHeadId == m.FamilyHeadId).Sum(e => e.Price) ?? 0;
        //        }

        //        int? accountId = h.FamilyHead.AccountId;

        //        packageAmount -= 1;

        //        PaymentVoucher voucher = _context.PaymentVouchersWithoutTrash
        //                    .Where(e => e.AccountId == accountId)
        //                    .OrderBy(e => e.PaymentVoucherId)
        //                    .FirstOrDefault();

        //        voucher.Amount = packageAmount;
        //        _context.SaveChanges();

        //        var voucherDebitEntry = _context.VoucherEntries
        //                            .OrderBy(e => e.PaymentVoucherId)
        //                            .FirstOrDefault(e => e.PaymentVoucherId == voucher.PaymentVoucherId
        //                            && e.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Debit);

        //        var voucherCreditEntry = _context.VoucherEntries
        //                            .OrderBy(e => e.PaymentVoucherId)
        //                            .FirstOrDefault(e => e.PaymentVoucherId == voucher.PaymentVoucherId
        //                            && e.DebitOrCreditSide == ShowAccountDebitOrCreditSide.Credit);

        //        voucherDebitEntry.Debit = packageAmount;
        //        voucherDebitEntry.Credit = packageAmount;

        //        _context.SaveChanges();

        //    }

        //    return Ok("Updated");
        //}

    }
}
