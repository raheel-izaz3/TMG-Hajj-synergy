using TMG.Models.MiscModels;

namespace TMG.Models.PackageModels
{
    public class PackagePaymentNotes
    {
        public int PackagePaymentNotesId { get; set; }
        public int PackageId { get; set; }
        public virtual Package Package { get; set; }
        public int PaymentNotesId { get; set; }
        public virtual PaymentNotes PaymentNotes { get; set; }

    }
}
