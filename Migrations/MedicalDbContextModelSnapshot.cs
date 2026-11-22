using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MedicalApi.Migrations;

[DbContext(typeof(MedicalDbContext))]
public sealed class MedicalDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.4");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);
        modelBuilder.UseIdentityByDefaultColumns();

        modelBuilder.Entity(typeof(Patient).FullName!, patient =>
        {
            patient.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer")
                .UseIdentityByDefaultColumn();
            patient.Property<string>("Pin").HasMaxLength(14).HasColumnType("character varying(14)");
            patient.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            patient.Property<DateTime>("BirthDate").HasColumnType("date");
            patient.Property<string>("Phone").HasColumnType("text");
            patient.Property<string>("Email").HasColumnType("text");
            patient.Property<string>("BloodType").HasMaxLength(10).HasColumnType("character varying(10)");
            patient.HasKey("Id");
            patient.HasIndex("Pin").IsUnique();
            patient.ToTable("Patients");
        });
    }
}
