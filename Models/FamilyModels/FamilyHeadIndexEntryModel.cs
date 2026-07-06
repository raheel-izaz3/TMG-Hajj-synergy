namespace TMG.Models.FamilyModels
{
    public record FamilyHeadIndexEntryModel(int SerialNumber, int FamilyHeadId, string BookedBy, string FeedBy, string Company, string Package, int Bookings,
    string Head, int NumberOfMembers, string Status, string EncryptedPassport, bool Tentative, bool IsHead, int BioCount, int VerifiedCount, string FcId,
    int BiometricVerifiedCount, int EHajjVerifiedCount, string HajjiBookingStatus, float? RemaingAmount, string Currency);

    public record DataFormIndexEntryModel(int SerialNumber, int FamilyHeadId, string BookedBy, string FeedBy, string Head, bool IsHead, string GroupNo, string Status, string CNIC, int PAX);

}
