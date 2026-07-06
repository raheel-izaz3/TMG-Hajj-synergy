using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TMG.ManagerClasses
{
    public static class ApplicationManager
    {
        public const string AESKey = "2b7e151628aed2a6abf7158809cf4f3c";

        public const string ImageDirPath = "Images/";
        public const string BASE_URL = "https://hajj.tmgpk.com/";
        //public const string LOCAL_URL = "https://hajj.tmgpk.com/";

        public static string NomineeCnicPath
        {
            get => ImageDirPath + "NomineeCnic/";
        }
        public static string DocumentImagePath
        {
            get => ImageDirPath + "Documents/";
        }
        public static string PackageImagesPath
        {
            get => ImageDirPath + "PackageImages/";
        }
        public static string PackageItineraryPath
        {
            get => ImageDirPath + "PackageItinerary/";
        }
        public static string CompanyLogosPath
        {
            get => ImageDirPath + "CompanyLogos/";
        }

        public static string MemberImagePath
        {
            get => ImageDirPath + "MemberImages/";
        }

        public static string MemberVaccImagePath
        {
            get => ImageDirPath + "MemberVaccImages/";
        }

        public static string PassportImagePath
        {
            get => ImageDirPath + "PassportImages/";
        }

        public static string VisaImagePath
        {
            get => ImageDirPath + "VisaImages/";
        }
        
        public static string PaymentVoucherImagesPath
        {
            get => ImageDirPath + "PaymentVoucherImages/";
        }
        
        public static string UserSignaturesPath
        {
            get => ImageDirPath + "UserSignatures/";
        }
        
        public static string BankTransferProofPath
        {
            get => ImageDirPath + "BankTransferImages/";
        }

        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(AESKey);
                aesAlg.IV = new byte[16]; // Initialization Vector, you might want to generate a random IV in a real scenario
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText);

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(AESKey);
                aesAlg.IV = new byte[16]; // Initialization Vector, should be the same as used during encryption
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    return srDecrypt.ReadToEnd();
            }
        }
    }
}
