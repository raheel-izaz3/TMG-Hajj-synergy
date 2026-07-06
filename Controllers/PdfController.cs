using TMG.Data;
using TMG.Models.FamilyModels;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;

namespace TMG.Controllers
{
    public class PdfController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _http;

        public PdfController(TMGDbContext dbContext, IWebHostEnvironment host, IHttpContextAccessor http)
        {
            _dbContext = dbContext;
            _host = host;
            _http = http;
        }

        public void AddImageToPdf(string pdfFilePath, string outputFilePath, string base64Image, float x, float y)
        {
            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            using (PdfWriter pdfWriter = new PdfWriter(outputFilePath))
            using (PdfDocument pdfDoc = new PdfDocument(pdfReader, pdfWriter))
            {
                Document document = new Document(pdfDoc);

                PdfPage secondPage = pdfDoc.GetPage(2);
                PdfCanvas canvas = new PdfCanvas(secondPage);

                base64Image = base64Image.Replace("data:image/png;base64,", "");

                byte[] imageBytes = Convert.FromBase64String(base64Image);
                ImageData imageData = ImageDataFactory.Create(imageBytes);
                Image image = new Image(imageData);

                float currentWidth = image.GetImageScaledWidth();
                float currentHeight = image.GetImageScaledHeight();

                float newWidth = currentWidth * 0.6f;
                float newHeight = currentHeight * 0.6f;

                float scaleFactor = 0.55f;
                canvas.SaveState()
                      .ConcatMatrix(scaleFactor, 0, 0, scaleFactor, x, y)
                      .AddImageAt(imageData, 0, 0, false);

            }
        }

        public IActionResult Index()
        {
            FamilyHead familyHead = _dbContext.FamilyHeads.Find(16);

            string file = Path.Combine(_host.WebRootPath, familyHead.MISPdfPath);

            string outputFile = Path.Combine(_host.WebRootPath, "SignedDocument.pdf");

            string base64Path = familyHead.SignatureBase64;

            float x = 125, y = 425;

            AddImageToPdf(file, outputFile, base64Path, x, y);

            return Ok(new
            {
            });
        }
    }
}
