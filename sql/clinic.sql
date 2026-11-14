-- Ручная часть задания. Запустите файл в новой учебной базе clinic.
-- Для просмотра отдельных результатов выделяйте запросы SELECT и запускайте по одному.
-- Пример UPDATE и DELETE запускайте целиком, от BEGIN до ROLLBACK.

BEGIN;

CREATE TABLE patients (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    birth_date DATE NOT NULL,
    phone VARCHAR(20)
);

CREATE TABLE spravka_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price NUMERIC(10, 2) NOT NULL CHECK (price > 0),
    valid_days INT NOT NULL
);

CREATE TABLE spravki (
    id SERIAL PRIMARY KEY,
    patient_id INT NOT NULL REFERENCES patients(id),
    spravka_type_id INT NOT NULL REFERENCES spravka_types(id),
    price NUMERIC(10, 2) NOT NULL,
    issued_at DATE NOT NULL DEFAULT CURRENT_DATE
);

-- Примеры из задания.
INSERT INTO patients (name, birth_date, phone) VALUES
    ('Айбек Асанов', DATE '1998-03-12', '0555123456'),
    ('Нургуль Токтосунова', DATE '2003-07-25', '0700987654'),
    ('Бакыт Жумабаев', DATE '1985-11-02', NULL),
    ('Айпери Садыкова', DATE '2001-01-30', '0772345678'),
    ('Тимур Ким', DATE '2006-09-14', '0509111222');

INSERT INTO spravka_types (name, price, valid_days) VALUES
    ('Справка 083', 100, 365),
    ('Санитарная книжка', 750, 365),
    ('Справка 086', 300, 180),
    ('Справка в бассейн', 200, 90);

INSERT INTO spravki (patient_id, spravka_type_id, price, issued_at) VALUES
    (1, 1, 100, DATE '2026-10-01'),
    (1, 4, 200, DATE '2026-10-15'),
    (2, 2, 750, DATE '2026-10-03'),
    (2, 3, 300, DATE '2026-11-02'),
    (4, 3, 300, DATE '2026-11-05'),
    (5, 4, 200, DATE '2026-11-06'),
    (5, 1, 100, DATE '2026-11-10');

-- Дополнительные вымышленные данные.
INSERT INTO patients (name, birth_date, phone) VALUES
    ('Данияр Касымов', DATE '1997-04-18', NULL),
    ('Аида Байзакова', DATE '2002-08-24', NULL),
    ('Мээрим Айдарова', DATE '2004-11-06', NULL);

INSERT INTO spravka_types (name, price, valid_days) VALUES
    ('Справка для спортивной секции', 250, 180);

INSERT INTO spravki (patient_id, spravka_type_id, price, issued_at) VALUES
    (6, 5, 250, DATE '2026-10-27'),
    (6, 1, 100, DATE '2026-11-07'),
    (7, 3, 300, DATE '2026-11-12'),
    (8, 5, 250, DATE '2026-11-15'),
    (8, 4, 200, DATE '2026-12-01');

COMMIT;

-- Все выданные справки с именем пациента и названием из каталога.
SELECT s.id, p.name AS patient, t.name AS spravka, s.price, s.issued_at
FROM spravki AS s
JOIN patients AS p ON p.id = s.patient_id
JOIN spravka_types AS t ON t.id = s.spravka_type_id
ORDER BY s.issued_at, s.id;

-- Общее количество справок и общая выручка.
SELECT COUNT(*) AS spravki_count, SUM(price) AS total
FROM spravki;

-- 1. Пациенты по алфавиту.
SELECT id, name, birth_date, phone
FROM patients
ORDER BY name, id;

-- 2. Пациенты, родившиеся в 2000 году и позже.
SELECT id, name, birth_date
FROM patients
WHERE birth_date >= DATE '2000-01-01'
ORDER BY birth_date, id;

-- 3. Сравнение с NULL через = даёт неизвестный результат, который WHERE отбрасывает.
SELECT id, name FROM patients WHERE phone = NULL;
SELECT id, name FROM patients WHERE phone IS NULL ORDER BY id;

-- 4. LIKE учитывает регистр. ILIKE ищет без учёта регистра по правилам локали базы.
SELECT id, name FROM patients WHERE name LIKE '%ай%' ORDER BY id;
SELECT id, name FROM patients WHERE name ILIKE '%ай%' ORDER BY id;

-- 5. Карточка пациента с номером 1. Цена берётся из выданной справки.
SELECT t.name AS spravka, s.price, s.issued_at
FROM spravki AS s
JOIN spravka_types AS t ON t.id = s.spravka_type_id
WHERE s.patient_id = 1
ORDER BY s.issued_at, s.id;

-- 6. Справки, выписанные в ноябре 2026 года.
SELECT id, patient_id, spravka_type_id, price, issued_at
FROM spravki
WHERE issued_at >= DATE '2026-11-01'
  AND issued_at < DATE '2026-12-01'
ORDER BY issued_at, id;

-- 7. Самый дорогой вид справки в каталоге.
SELECT id, name, price
FROM spravka_types
ORDER BY price DESC, id
LIMIT 1;

-- 8. Количество справок и выручка за ноябрь 2026 года.
SELECT COUNT(*) AS spravki_count, SUM(price) AS total
FROM spravki
WHERE issued_at >= DATE '2026-11-01'
  AND issued_at < DATE '2026-12-01';

-- 9. Изменяем цену в каталоге и проверяем цены уже выданных справок.
-- 10. Удаляем одну справку. ROLLBACK возвращает исходные учебные данные.
BEGIN;

UPDATE spravka_types
SET price = 120
WHERE id = 1
RETURNING id, name, price AS new_catalog_price;

SELECT s.id, s.price AS issued_price, t.price AS catalog_price
FROM spravki AS s
JOIN spravka_types AS t ON t.id = s.spravka_type_id
WHERE t.id = 1
ORDER BY s.id;

DELETE FROM spravki
WHERE id = 7
RETURNING id, patient_id, spravka_type_id, price, issued_at;

ROLLBACK;

-- 11. Количество справок у каждого пациента, включая пациентов без справок.
SELECT p.id, p.name, COUNT(s.id) AS spravki_count
FROM patients AS p
LEFT JOIN spravki AS s ON s.patient_id = p.id
GROUP BY p.id, p.name
ORDER BY p.name, p.id;

-- Проверки ограничений. Каждый блок перехватывает только ожидаемую ошибку
-- и показывает её в сообщениях pgAdmin. Отрицательные id оставляют счётчики SERIAL без изменений.
DO $$
DECLARE
    constraint_name_found TEXT;
BEGIN
    INSERT INTO spravki (id, patient_id, spravka_type_id, price)
    VALUES (-1, 999, 1, 100);
    RAISE EXCEPTION 'Ожидался отказ: пациент 999 отсутствует';
EXCEPTION WHEN foreign_key_violation THEN
    GET STACKED DIAGNOSTICS constraint_name_found = CONSTRAINT_NAME;
    IF constraint_name_found IS DISTINCT FROM 'spravki_patient_id_fkey' THEN
        RAISE;
    END IF;
    RAISE NOTICE '[%] %', SQLSTATE, SQLERRM;
END;
$$;

DO $$
DECLARE
    constraint_name_found TEXT;
BEGIN
    INSERT INTO spravka_types (id, name, price, valid_days)
    VALUES (-1, 'Справка в лагерь', -500, 30);
    RAISE EXCEPTION 'Ожидался отказ: цена должна быть положительной';
EXCEPTION WHEN check_violation THEN
    GET STACKED DIAGNOSTICS constraint_name_found = CONSTRAINT_NAME;
    IF constraint_name_found IS DISTINCT FROM 'spravka_types_price_check' THEN
        RAISE;
    END IF;
    RAISE NOTICE '[%] %', SQLSTATE, SQLERRM;
END;
$$;

DO $$
DECLARE
    column_name_found TEXT;
BEGIN
    INSERT INTO patients (id, birth_date)
    VALUES (-1, DATE '2000-01-01');
    RAISE EXCEPTION 'Ожидался отказ: имя пациента обязательно';
EXCEPTION WHEN not_null_violation THEN
    GET STACKED DIAGNOSTICS column_name_found = COLUMN_NAME;
    IF column_name_found IS DISTINCT FROM 'name' THEN
        RAISE;
    END IF;
    RAISE NOTICE '[%] %', SQLSTATE, SQLERRM;
END;
$$;

DO $$
DECLARE
    constraint_name_found TEXT;
BEGIN
    DELETE FROM patients WHERE id = 1;
    RAISE EXCEPTION 'Ожидался отказ: у пациента 1 есть выданные справки';
EXCEPTION WHEN foreign_key_violation THEN
    GET STACKED DIAGNOSTICS constraint_name_found = CONSTRAINT_NAME;
    IF constraint_name_found IS DISTINCT FROM 'spravki_patient_id_fkey' THEN
        RAISE;
    END IF;
    RAISE NOTICE '[%] %', SQLSTATE, SQLERRM;
END;
$$;
