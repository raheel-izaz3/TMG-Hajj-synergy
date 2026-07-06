using TMG.Data;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers.AccountControllers
{
    public class VoucherController : Controller
    {
        private readonly TMGDbContext _dbContext;

        public VoucherController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

       

        
    }
}
