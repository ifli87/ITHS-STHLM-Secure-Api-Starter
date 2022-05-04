using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace secure_api.Data
{
    // identitydbcontext är en härledd klass från dbcontext
    // med stöd för inloggning ocg rollhantering via automatiska tabeller 
    // som vi får via IdentityDbContext
    public class ApplicationContext : IdentityDbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {
        }
    }
}