using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Pou_Pass_Man.Model
{
    /// <summary>
    /// a class that creates the creates the database
    /// </summary>
    internal class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Authentication> Authentications { get; set; }

        public string DbPath { get; }
        /// <summary>
        /// Constructor that creates the .db file if it's not exist
        /// </summary>
        public DatabaseContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            path += "\\PouPasMan";
            if (!Directory.Exists(path))
            {
                try { Directory.CreateDirectory(path); } catch { }
            }
            DbPath = Path.Join(path, "pouPasMan.db");
        }
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }
}
