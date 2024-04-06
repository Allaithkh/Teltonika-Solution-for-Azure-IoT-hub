using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TeltonikaModels;

namespace TeltonikaRepository
{
    public class TeltonikaContext : DbContext
    {
        public TeltonikaContext(DbContextOptions<TeltonikaContext> options)
            : base(options)
        {

        }

        public DbSet<DeviceTransmission> DeviceTransmissions { get; set; }
        public DbSet<DecodedDeviceTransmission> DecodedDeviceTransmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DeviceTransmission>().HasKey(c => c.Id);
            modelBuilder.Entity<DecodedDeviceTransmission>().HasKey(c => c.Id);
            modelBuilder.Entity<DeviceTransmission>().Property(c => c.Id).UseIdentityColumn();
            modelBuilder.Entity<DecodedDeviceTransmission>().Property(c => c.Id).UseIdentityColumn();
        }
    }
}
