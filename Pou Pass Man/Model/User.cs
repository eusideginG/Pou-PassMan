using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Pou_Pass_Man.Model
{
    /// <summary>
    /// this class is a model of user that can use the aplication (the a supports multiple users)
    /// </summary>
    [Index(nameof(Username), IsUnique = true)]
    internal class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
        public virtual ICollection<Authentication>
            Authentications
        { get; private set; } = new ObservableCollection<Authentication>();
    }
}
