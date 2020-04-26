using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Entities
{
    public class BiblioEntities : DbContext
    {


        public BiblioEntities(DbContextOptions<BiblioEntities> options) :
        base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
