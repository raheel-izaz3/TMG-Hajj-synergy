namespace TMG.Models.Updated_Accounts
{
    public class JVAccounts
    {
        public int JVAccountsId { get; set; }
        public int? CreditId { get; set; }
        public int? DebitId { get; set; }

        public Currencies? CurrencyD { get; set; }
        public Currencies? CurrencyC { get; set; }
        
        public int JVId { get; set; }
        public JV JV { get; set; }
    }
}
