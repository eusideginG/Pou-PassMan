using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Pou_Pass_Man.Model
{
    /// <summary>
    /// This is a class tha models the authentication (login crendentials)
    /// </summary>
    [Index(nameof(Name), IsUnique = true)]
    public class Authentication
    {
        [Key]
        public int AuthId { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? UsernameOrEmail { get; set; }
        public string? Password { get; set; }
        public string? Website { get; set; }
        public string? Date { get; set; }
    }
}
