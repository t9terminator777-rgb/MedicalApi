using MedicalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalApi.Data;

public class MedicalDbContext(DbContextOptions<MedicalDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Spravka> Spravki => Set<Spravka>();
    public DbSet<SpravkaType> SpravkaTypes => Set<SpravkaType>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Spravka>()
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(spravka => spravka.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Spravka>()
            .HasOne<SpravkaType>()
            .WithMany()
            .HasForeignKey(spravka => spravka.SpravkaTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PatientId);

        modelBuilder.Entity<Spravka>().Property(spravka => spravka.Price).HasPrecision(10, 2);
        modelBuilder.Entity<SpravkaType>().Property(type => type.Price).HasPrecision(10, 2);
        modelBuilder.Entity<SpravkaType>().ToTable(table =>
            table.HasCheckConstraint("CK_SpravkaTypes_Price", "\"Price\" > 0"));
    }
}
