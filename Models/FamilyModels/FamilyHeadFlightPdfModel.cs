namespace TMG.Controllers.FamilyControllers
{
    public class FamilyHeadFlightPdfModel
    {
        public int FamilyHeadFlightId { get; set; }
        public IFormFile TicketPDF { get; set; }
    }
}
