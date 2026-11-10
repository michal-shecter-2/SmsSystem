-- Optional synthetic demo data. Safe to run again in the local demo database.
USE SmsDb;
GO

INSERT INTO dbo.Employees (FirstName, LastName, Address)
SELECT demo.FirstName, demo.LastName, demo.Address
FROM (VALUES
    (N'נועה', N'כהן', N'רחוב הדוגמה 1, חיפה'),
    (N'אורי', N'לוי', N'רחוב הדוגמה 2, ירושלים'),
    (N'נועה', N'כהן', N'רחוב הדוגמה 3, תל אביב')
) AS demo(FirstName, LastName, Address)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Employees AS e
    WHERE e.FirstName = demo.FirstName
      AND e.LastName = demo.LastName
      AND e.Address = demo.Address
);
GO

-- A number may belong to more than one employee. There is no UNIQUE on it.
INSERT INTO dbo.EmployeePhones (EmployeeId, PhoneNumber)
SELECT e.Id, demo.PhoneNumber
FROM (VALUES
    (N'נועה', N'כהן', N'רחוב הדוגמה 1, חיפה', '0501234567'),
    (N'נועה', N'כהן', N'רחוב הדוגמה 1, חיפה', '+972521234567'),
    (N'אורי', N'לוי', N'רחוב הדוגמה 2, ירושלים', '0501234567')
) AS demo(FirstName, LastName, Address, PhoneNumber)
JOIN dbo.Employees AS e
    ON e.FirstName = demo.FirstName
   AND e.LastName = demo.LastName
   AND e.Address = demo.Address
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EmployeePhones AS p
    WHERE p.EmployeeId = e.Id AND p.PhoneNumber = demo.PhoneNumber
);
GO
