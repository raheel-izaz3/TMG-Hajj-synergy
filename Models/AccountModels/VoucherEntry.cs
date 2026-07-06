namespace TMG.Models.AccountModels
{
    public class VoucherEntry
    {
        public int VoucherEntryId { get; set; }
        
        public int PaymentVoucherId { get; set; }
        public PaymentVoucher PaymentVoucher { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }

        public float Debit { get; set; }
        public float Credit { get; set; }
        public ShowAccountDebitOrCreditSide DebitOrCreditSide { get; set; }
    }
}
