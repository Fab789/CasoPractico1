/* ============================================================
   Caso Práctico 1 - Programación Avanzada Web (SC-701)
   Script de creación de la base de datos EarthquakeDb
   (Opcional: el API también crea la BD con EF EnsureCreated)
   ============================================================ */
IF DB_ID(N'EarthquakeDb') IS NULL
    CREATE DATABASE EarthquakeDb;
GO

USE EarthquakeDb;
GO

IF OBJECT_ID(N'dbo.Earthquakes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Earthquakes
    (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Earthquakes PRIMARY KEY,
        UsgsId       NVARCHAR(50)  NOT NULL,   -- id del evento en USGS (único)
        Magnitude    FLOAT         NULL,
        Place        NVARCHAR(500) NOT NULL,   -- ej. "10 km SW of Iquique, Chile"
        Country      NVARCHAR(150) NOT NULL,   -- derivado de Place
        TimeUtc      DATETIME2     NOT NULL,
        Latitude     FLOAT         NOT NULL,
        Longitude    FLOAT         NOT NULL,
        DepthKm      FLOAT         NULL,
        Url          NVARCHAR(500) NULL,
        Title        NVARCHAR(500) NULL,
        [Type]       NVARCHAR(50)  NULL,
        CreatedAtUtc DATETIME2     NOT NULL CONSTRAINT DF_Earthquakes_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX IX_Earthquakes_UsgsId ON dbo.Earthquakes (UsgsId);
    CREATE INDEX IX_Earthquakes_Country ON dbo.Earthquakes (Country);
    CREATE INDEX IX_Earthquakes_TimeUtc ON dbo.Earthquakes (TimeUtc);
END
GO
