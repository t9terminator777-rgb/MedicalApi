-- База medical. Ниже SQL из журнала Microsoft.EntityFrameworkCore.Database.Command
-- при запросах к API на PostgreSQL 18.6. Команды с @параметрами приведены в
-- комментариях, чтобы файл можно было безопасно открыть и выполнить в pgAdmin.
-- EF Core скрывает значения параметров в журнале, показывая '?'.

-- GET /api/patients: список с сортировкой по имени, затем по идентификатору.
SELECT p."Id", p."BirthDate", p."BloodType", p."Email", p."Name", p."Phone"
FROM "Patients" AS p
ORDER BY p."Name", p."Id";

-- GET /api/patients/{id}: WHERE ограничивает выборку заданным Id, LIMIT 1
-- ограничивает результат одной строкой. Этот же SELECT предваряет PUT и DELETE.
-- SELECT p."Id", p."BirthDate", p."BloodType", p."Email", p."Name", p."Phone"
-- FROM "Patients" AS p
-- WHERE p."Id" = @id
-- LIMIT 1

-- GET /api/patients?name=ай: поиск по имени без учёта регистра.
-- SELECT p."Id", p."BirthDate", p."BloodType", p."Email", p."Name", p."Phone"
-- FROM "Patients" AS p
-- WHERE p."Name" ILIKE @pattern ESCAPE '\'
-- ORDER BY p."Name", p."Id"

-- POST /api/patients: PostgreSQL генерирует Id и передаёт его через RETURNING.
-- INSERT INTO "Patients" ("BirthDate", "BloodType", "Email", "Name", "Phone")
-- VALUES (@p0, @p1, @p2, @p3, @p4)
-- RETURNING "Id";

-- PUT /api/patients/{id}: после SELECT EF Core меняет только столбец Phone.
-- Повторный PUT с теми же данными отправил SELECT. UPDATE в журнале отсутствует.
-- UPDATE "Patients" SET "Phone" = @p0
-- WHERE "Id" = @p1;

-- DELETE /api/patients/{id}: после SELECT проверяется наличие справок.
-- SELECT EXISTS (
--     SELECT 1
--     FROM "Spravki" AS s
--     WHERE s."PatientId" = @id)
-- При наличии справок API отвечает 400. Следующий DELETE выполняется,
-- когда у пациента нет выданных справок.
-- DELETE FROM "Patients"
-- WHERE "Id" = @p0;

-- GET /api/patients/{id}/spravki: сначала проверяется существование пациента.
-- SELECT EXISTS (
--     SELECT 1
--     FROM "Patients" AS p
--     WHERE p."Id" = @id)
-- SELECT s."Id", s."IssuedAt", s."Name", s."PatientId", s."Price", s."SpravkaTypeId"
-- FROM "Spravki" AS s
-- WHERE s."PatientId" = @id
-- ORDER BY s."Id"

-- GET /api/spravki?patientId=7&minPrice=300&sort=price: сравните с тем же
-- запросом без minPrice. Условие по цене вычисляется на сервере PostgreSQL.
-- SELECT s."Id", s."IssuedAt", s."Name", s."PatientId", s."Price", s."SpravkaTypeId"
-- FROM "Spravki" AS s
-- WHERE s."PatientId" = @patientId_Value AND s."Price" >= @minPrice_Value
-- ORDER BY s."Price", s."Id"

-- GET /api/stats выполняет три SQL-команды.
SELECT count(*)::int
FROM "Patients" AS p;

SELECT count(*)::int
FROM "Spravki" AS s;

SELECT COALESCE(sum(s."Price"), 0.0)
FROM "Spravki" AS s;

-- Демонстрация связи PostgreSQL и API. Подставьте Id из ответа POST Swagger.
-- SELECT "Id", "Name", "Phone" FROM "Patients" WHERE "Id" = 7;
-- UPDATE "Patients" SET "Phone" = '0555000002' WHERE "Id" = 7;
-- Повторите GET /api/patients/7, чтобы увидеть обновлённый телефон.

SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
