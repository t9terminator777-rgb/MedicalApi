using MedicalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalApi.Data;

public sealed class MedicalDbContext(DbContextOptions<MedicalDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>().HasIndex(patient => patient.Pin).IsUnique();
        modelBuilder.Entity<Patient>().Property(patient => patient.Pin).HasMaxLength(14);
        modelBuilder.Entity<Patient>().Property(patient => patient.BirthDate).HasColumnType("date");
    }
}
