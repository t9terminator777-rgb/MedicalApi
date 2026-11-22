using MedicalApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MedicalApi.Migrations;

[DbContext(typeof(MedicalDbContext))]
[Migration("20261122000100_CreatePatients")]
public sealed class CreatePatients : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Patients",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Pin = table.Column<string>(type: "character varying(14)", nullable: true),
                Name = table.Column<string>(type: "character varying(100)", nullable: false),
                BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                Phone = table.Column<string>(type: "text", nullable: true),
                Email = table.Column<string>(type: "text", nullable: true),
                BloodType = table.Column<string>(type: "character varying(10)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Patients", row => row.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Patients_Pin", table: "Patients", column: "Pin", unique: true);

        migrationBuilder.Sql("""
            INSERT INTO "Patients" ("Id", "Name", "BirthDate", "Phone", "Email", "BloodType") VALUES
                (1, 'Алиса Иванова', DATE '2000-05-15', '+996111222333', 'alice@example.com', 'O+'),
                (2, 'Бек Садыков', DATE '1998-09-03', '+996555123456', 'bek@example.com', 'A+');
            ALTER TABLE "Patients" ALTER COLUMN "Id" RESTART WITH 3;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("Patients");
}
