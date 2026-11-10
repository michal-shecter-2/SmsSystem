USE master;
GO

IF DB_ID(N'SmsDb') IS NULL
BEGIN
    CREATE DATABASE SmsDb;
END;
GO

USE SmsDb;
GO

IF OBJECT_ID(N'dbo.Messages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Messages
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Messages PRIMARY KEY,

        [To] VARCHAR(16) NOT NULL,

        Body NVARCHAR(160) NOT NULL,

        Status VARCHAR(16) NOT NULL
            CONSTRAINT DF_Messages_Status DEFAULT 'pending',

        ProviderId NVARCHAR(200) NULL,

        Error VARCHAR(100) NULL,

        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Messages_CreatedAt
            DEFAULT SYSUTCDATETIME(),

        UpdatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Messages_UpdatedAt
            DEFAULT SYSUTCDATETIME(),

        CONSTRAINT CK_Messages_Status CHECK
        (
            Status IN
            (
                'pending',
                'accepted',
                'delivered',
                'failed',
                'unknown'
            )
        )
    );
END;
GO
-- Employee identity is internal; the three business fields are unique together.
IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Address NVARCHAR(200) NOT NULL,
        CONSTRAINT UQ_Employees_Identity UNIQUE (FirstName, LastName, Address)
    );
END;
GO

IF OBJECT_ID(N'dbo.EmployeePhones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeePhones
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeePhones PRIMARY KEY,
        EmployeeId INT NOT NULL,
        PhoneNumber VARCHAR(16) NOT NULL,
        CONSTRAINT FK_EmployeePhones_Employees
            FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id)
    );

    CREATE INDEX IX_EmployeePhones_EmployeeId
        ON dbo.EmployeePhones(EmployeeId);
END;
GO
