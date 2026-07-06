using TMG.Data;
using TMG.Models.AccountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TMG.Controllers.AccountControllers
{
    public class CategoryController: Controller
    {
        private readonly TMGDbContext _dbContext;

        public CategoryController(TMGDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.TrashCount = _dbContext.Categories.Where(e => e.Trashed).Count();


            //ViewBag.TrashAllowed = PrivilegeManager.CanTrashCategory(_dbContext);
            //ViewBag.CreateAllowed = PrivilegeManager.CanCreateCategory(_dbContext);
            //ViewBag.EditAllowed = PrivilegeManager.CanEditCategory(_dbContext);
            //ViewBag.RestoreAllowed = PrivilegeManager.CanRestoreCategory(_dbContext);

            return View(_dbContext.CategoriesWithoutTrash
                .Include(e => e.SubType)
                .ThenInclude(e => e.Company)
                .Include(e => e.SubType)
                .ThenInclude(e => e.Type));
        }

        public IActionResult Create()
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            return View();
        }


        [HttpPost]
        public IActionResult Create(Category category)
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            if (!ModelState.IsValid) return View();

            _dbContext.Categories.Add(category);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            Category category = _dbContext.Categories
                .Include(e => e.SubType)
                .FirstOrDefault(e => e.CategoryId == id);

            ViewBag.TypeId = category.SubType.TypeId;
            ViewBag.CompanyId = category.SubType.CompanyId;

            return View(category);
        }


        [HttpPost]
        public IActionResult Edit(Category category)
        {
            ViewBag.Types = _dbContext.Types;
            ViewBag.SubTypes = _dbContext.SubTypesWithoutTrash;
            ViewBag.Companies = _dbContext.CompaniesWithoutTrash;

            if (!ModelState.IsValid) return View();

            _dbContext.Entry(category).State = EntityState.Modified;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewTrash()
        {
            try
            {
                var categories = _dbContext.Categories
                    .Include(e => e.SubType)
                        .ThenInclude(e => e.Company)
                    .Include(e => e.SubType)
                        .ThenInclude(e => e.Type)
                    .Where(s => s.Trashed)
                    .ToList();

                return View(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw; 
            }

        }

        public IActionResult Trash(int id)
        {
            Category category = _dbContext.Categories.Find(id);
            if (category == null) return RedirectToAction(nameof(Index));

            category.Trashed = true;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Restore(int id)
        {
            Category category = _dbContext.Categories.Find(id);
            category.Trashed = false;
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
