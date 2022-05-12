using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeystrokesData
{
    public class KeystrokesContextFactory : IDesignTimeDbContextFactory<KeystrokesDbContext>
    {
        public KeystrokesDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KeystrokesDbContext>();
            optionsBuilder.UseNpgsql("Server=localhost;Database=keystrokes_dev;Port=5432;User Id=postgres;Password=12345");

            return new KeystrokesDbContext(optionsBuilder.Options);
        }
    }
}
