using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace TMG.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User : BaseEntity
    {
        [Comment("Primary Key of Users")]
        public int UserId { get; set; }

        [MaxLength(256)]
        [Comment("Username of the User")]
        public string Username { get; set; }

        [MaxLength(256)]
        [Comment("Email of the user of authentication")]
        public string Email { get; set; }

        [Comment("Generated Hash of the user's password string")]
        public byte[] PasswordHash { get; set; } = new byte[32];

        [Comment("Salt of the password hash")]
        public byte[] PasswordSalt { get; set; } = new byte[32];

        [Comment("Foreign Key from Role")]
        public int? RoleId { get; set; }
        
        public string? UserSignature { get; set; }
        public int? PinCode { get; set; }

        // Navigation Properties
        public virtual Role? Role { get; set; }

        public const string EmailKey = nameof(Email);

        /// <summary>
        /// Generates Password Hash and Salt
        /// </summary>
        /// <param name="passwordString">Password string entered by user</param>
        /// <param name="passwordHash">Generated hash of the password string</param>
        /// <param name="passwordSalt">Salt of the hash</param>
        public static void GenerateHash(string passwordString, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (HMACSHA512 hmac = new HMACSHA512())
            {
                byte[] _computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordString));
                
                passwordHash = _computedHash;
                passwordSalt = hmac.Key;
            }
        }

        /// <summary>
        /// Verifies if the password string is equal to hash
        /// </summary>
        /// <param name="passwordString">Password string entered by user</param>
        /// <param name="passwordHash">Password hash of the user</param>
        /// <param name="passwordSalt">Salt of the hash</param>
        /// <returns></returns>
        public static bool VerifyPassword(string passwordString, byte[] passwordHash, byte[] passwordSalt)
        {
            using (HMACSHA512 hmac = new HMACSHA512(passwordSalt))
            {
                byte[] _computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordString));
                return _computedHash.SequenceEqual(passwordHash);
            }
        }

        public static bool IsDuplicateEmail(DbUpdateException ex)
        {
            return ex.InnerException is SqlException || ex.InnerException?.InnerException is SqlException;
        }
    }
}
