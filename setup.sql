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