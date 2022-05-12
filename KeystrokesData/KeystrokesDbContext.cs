using KeystrokesData.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeystrokesData
{
    public class KeystrokesDbContext : DbContext
    {
        public KeystrokesDbContext(DbSet<TrainSample> trainData, DbSet<TestSample> testData)
        {
            TrainData = trainData;
            TestData = testData;
        }

        public KeystrokesDbContext(DbContextOptions<KeystrokesDbContext> options):
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            modelBuilder.Entity<TrainSample>()
                .HasMany(a => a.Probes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestSample>()
                .HasMany(a => a.Probes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KnnGraphEntity>()
                .HasMany(a => a.Edges)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KnnGraphEntity>()
                .HasMany(a => a.Nodes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<TrainSample> TrainData { get; set; }
        public DbSet<TestSample> TestData { get; set; }

        public DbSet<KnnGraphEntity> GraphData { get; set; }

    }
}
