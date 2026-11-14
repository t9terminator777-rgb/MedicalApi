-- Выполнить в базе medical после применения миграций.
INSERT INTO "SpravkaTypes" ("Name", "Price", "ValidDays")
SELECT seed.name, seed.price, seed.valid_days
FROM (VALUES
    ('Справка 083', 100.00, 365),
    ('Санитарная книжка', 750.00, 365),
    ('Справка 086', 300.00, 180),
    ('Справка в бассейн', 200.00, 90)
) AS seed(name, price, valid_days)
WHERE NOT EXISTS (
    SELECT 1 FROM "SpravkaTypes" AS current_type WHERE current_type."Name" = seed.name
);
