      ALTER TABLE [Packages] ADD [IndividualFacilityDescriptionId] int NULL;
      ALTER TABLE [Packages] ADD [NearBy] nvarchar(max) NULL;
      ALTER TABLE [FamilyHeads] ADD [CountryId] int NULL;
      ALTER TABLE [FamilyHeads] ADD [NationalityId] int NULL;
      ALTER TABLE [FamilyHeads] ADD [PassportTypeId] int NULL;
      ALTER TABLE [FamilyHeadAddresses] ADD [CityId] int NULL;
      ALTER TABLE [FamilyHeadAddresses] ADD [CountryId] int NULL;
      ALTER TABLE [CombinedFlightSummaryDetail] ADD [Honorifics] nvarchar(max) NULL;
      CREATE TABLE [Cities] (
          [CityId] int NOT NULL IDENTITY,
          [Name] nvarchar(max) NULL,
          [CreatedAt] datetime2 NOT NULL,
          [UpdatedAt] datetime2 NOT NULL,
          [Trashed] bit NOT NULL,
          CONSTRAINT [PK_Cities] PRIMARY KEY ([CityId])
      );
      DECLARE @defaultSchema AS sysname;
      SET @defaultSchema = SCHEMA_NAME();
      DECLARE @description AS sql_variant;
      SET @description = N'Entity''s created date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Cities', 'COLUMN', N'CreatedAt';
      SET @description = N'Entity''s last update date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Cities', 'COLUMN', N'UpdatedAt';
      CREATE TABLE [Countries] (
          [CountryId] int NOT NULL IDENTITY,
          [Name] nvarchar(max) NULL,
          [CreatedAt] datetime2 NOT NULL,
          [UpdatedAt] datetime2 NOT NULL,
          [Trashed] bit NOT NULL,
          CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryId])
      );
      DECLARE @defaultSchema AS sysname;
      SET @defaultSchema = SCHEMA_NAME();
      DECLARE @description AS sql_variant;
      SET @description = N'Entity''s created date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Countries', 'COLUMN', N'CreatedAt';
      SET @description = N'Entity''s last update date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Countries', 'COLUMN', N'UpdatedAt';
      CREATE TABLE [Nationalities] (
          [NationalityId] int NOT NULL IDENTITY,
          [Name] nvarchar(max) NULL,
          [CreatedAt] datetime2 NOT NULL,
          [UpdatedAt] datetime2 NOT NULL,
          [Trashed] bit NOT NULL,
          CONSTRAINT [PK_Nationalities] PRIMARY KEY ([NationalityId])
      );
      DECLARE @defaultSchema AS sysname;
      SET @defaultSchema = SCHEMA_NAME();
      DECLARE @description AS sql_variant;
      SET @description = N'Entity''s created date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Nationalities', 'COLUMN', N'CreatedAt';
      SET @description = N'Entity''s last update date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Nationalities', 'COLUMN', N'UpdatedAt';
      CREATE TABLE [PassportTypes] (
          [PassportTypeId] int NOT NULL IDENTITY,
          [PassportTypeName] nvarchar(max) NULL,
          [CreatedAt] datetime2 NOT NULL,
          [UpdatedAt] datetime2 NOT NULL,
          [Trashed] bit NOT NULL,
          CONSTRAINT [PK_PassportTypes] PRIMARY KEY ([PassportTypeId])
      );
      DECLARE @defaultSchema AS sysname;
      SET @defaultSchema = SCHEMA_NAME();
      DECLARE @description AS sql_variant;
      SET @description = N'Entity''s created date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'PassportTypes', 'COLUMN', N'CreatedAt';
      SET @description = N'Entity''s last update date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'PassportTypes', 'COLUMN', N'UpdatedAt';
      CREATE TABLE [Vendors] (
          [VendorId] int NOT NULL IDENTITY,
          [VendorName] nvarchar(max) NULL,
          [CreatedAt] datetime2 NOT NULL,
          [UpdatedAt] datetime2 NOT NULL,
          [Trashed] bit NOT NULL,
          CONSTRAINT [PK_Vendors] PRIMARY KEY ([VendorId])
      );
      DECLARE @defaultSchema AS sysname;
      SET @defaultSchema = SCHEMA_NAME();
      DECLARE @description AS sql_variant;
      SET @description = N'Entity''s created date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Vendors', 'COLUMN', N'CreatedAt';
      SET @description = N'Entity''s last update date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Vendors', 'COLUMN', N'UpdatedAt';
      CREATE TABLE [VendorTypes] (
          [VendorTypeId] int NOT NULL IDENTITY,
          [Type] nvarchar(max) NULL,
          [CreatedAt] datetime2 NOT NULL,
          [UpdatedAt] datetime2 NOT NULL,
          [Trashed] bit NOT NULL,
          CONSTRAINT [PK_VendorTypes] PRIMARY KEY ([VendorTypeId])
      );
      DECLARE @defaultSchema AS sysname;
      SET @defaultSchema = SCHEMA_NAME();
      DECLARE @description AS sql_variant;
      SET @description = N'Entity''s created date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'VendorTypes', 'COLUMN', N'CreatedAt';
      SET @description = N'Entity''s last update date time';
      EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'VendorTypes', 'COLUMN', N'UpdatedAt';

       EXEC sp_rename N'[VendorTypes].[Type]', N'TypeName', N'COLUMN';


ALTER TABLE [Vendors] ADD [VendorTypeId] int NOT NULL DEFAULT 0;

-- Create the Cities table
CREATE TABLE Cities (
    CityId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Cities_CreatedAt DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Cities_UpdatedAt DEFAULT GETDATE(),
    Trashed BIT NOT NULL DEFAULT 0
);

-- Create the Countries table
CREATE TABLE Countries (
    CountryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Countries_CreatedAt DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Countries_UpdatedAt DEFAULT GETDATE(),
    Trashed BIT NOT NULL DEFAULT 0
);

-- Create the Nationalities table
CREATE TABLE Nationalities (
    NationalityId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Nationalities_CreatedAt DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Nationalities_UpdatedAt DEFAULT GETDATE(),
    Trashed BIT NOT NULL DEFAULT 0
);

-- Create the PassportTypes table
CREATE TABLE PassportTypes (
    PassportTypeId INT IDENTITY(1,1) PRIMARY KEY,
    PassportTypeName NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PassportTypes_CreatedAt DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_PassportTypes_UpdatedAt DEFAULT GETDATE(),
    Trashed BIT NOT NULL DEFAULT 0
);

-- Add CountryId column to FamilyHeads table
ALTER TABLE FamilyHeads
ADD CountryId INT NULL;

-- Add NationalityId column to FamilyHeads table
ALTER TABLE FamilyHeads
ADD NationalityId INT NULL;

-- Add PassportTypeId column to FamilyHeads table
ALTER TABLE FamilyHeads
ADD PassportTypeId INT NULL;

-- Create index on CountryId in FamilyHeads table
CREATE INDEX IX_FamilyHeads_CountryId
ON FamilyHeads (CountryId);

-- Create index on NationalityId in FamilyHeads table
CREATE INDEX IX_FamilyHeads_NationalityId
ON FamilyHeads (NationalityId);

-- Create index on PassportTypeId in FamilyHeads table
CREATE INDEX IX_FamilyHeads_PassportTypeId
ON FamilyHeads (PassportTypeId);

-- Add foreign key constraint from FamilyHeads.CountryId to Countries.CountryId
ALTER TABLE FamilyHeads
ADD CONSTRAINT FK_FamilyHeads_Countries_CountryId
FOREIGN KEY (CountryId) REFERENCES Countries (CountryId);

-- Add foreign key constraint from FamilyHeads.NationalityId to Nationalities.NationalityId
ALTER TABLE FamilyHeads
ADD CONSTRAINT FK_FamilyHeads_Nationalities_NationalityId
FOREIGN KEY (NationalityId) REFERENCES Nationalities (NationalityId);

-- Add foreign key constraint from FamilyHeads.PassportTypeId to PassportTypes.PassportTypeId
ALTER TABLE FamilyHeads
ADD CONSTRAINT FK_FamilyHeads_PassportTypes_PassportTypeId
FOREIGN KEY (PassportTypeId) REFERENCES PassportTypes (PassportTypeId);

-- Add CityId column to FamilyHeadAddresses table
ALTER TABLE FamilyHeadAddresses
ADD CityId INT NULL;

-- Create index on CityId in FamilyHeadAddresses table
CREATE INDEX IX_FamilyHeadAddresses_CityId
ON FamilyHeadAddresses (CityId);

-- Add foreign key constraint from FamilyHeadAddresses.CityId to Cities.CityId
ALTER TABLE FamilyHeadAddresses
ADD CONSTRAINT FK_FamilyHeadAddresses_Cities_CityId
FOREIGN KEY (CityId) REFERENCES Cities (CityId)
ON DELETE CASCADE;

-- Drop the existing foreign key constraint on CityId in FamilyHeadAddresses
ALTER TABLE FamilyHeadAddresses
DROP CONSTRAINT FK_FamilyHeadAddresses_Cities_CityId;

-- Alter CityId column to make it nullable
ALTER TABLE FamilyHeadAddresses
ALTER COLUMN CityId INT NULL;

-- Add CountryId column to FamilyHeadAddresses table
ALTER TABLE FamilyHeadAddresses
ADD CountryId INT NULL;

-- Create index on CountryId in FamilyHeadAddresses table
CREATE INDEX IX_FamilyHeadAddresses_CountryId
ON FamilyHeadAddresses (CountryId);

-- Add the foreign key constraint back for CityId in FamilyHeadAddresses
ALTER TABLE FamilyHeadAddresses
ADD CONSTRAINT FK_FamilyHeadAddresses_Cities_CityId
FOREIGN KEY (CityId) REFERENCES Cities (CityId);

-- Add foreign key constraint from FamilyHeadAddresses.CountryId to Countries.CountryId
ALTER TABLE FamilyHeadAddresses
ADD CONSTRAINT FK_FamilyHeadAddresses_Countries_CountryId
FOREIGN KEY (CountryId) REFERENCES Countries (CountryId);

-- Add CountryCodeId column to FamilyHeads table
ALTER TABLE FamilyHeads
ADD CountryCodeId INT NULL;

-- Create CountryCodes table with primary key and Code column
CREATE TABLE CountryCodes (
    CountryCodeId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(MAX) NULL
);

-- Create index on CountryCodeId in FamilyHeads table
CREATE INDEX IX_FamilyHeads_CountryCodeId
ON FamilyHeads (CountryCodeId);

-- Add foreign key constraint from FamilyHeads.CountryCodeId to CountryCodes.CountryCodeId
ALTER TABLE FamilyHeads
ADD CONSTRAINT FK_FamilyHeads_CountryCodes_CountryCodeId
FOREIGN KEY (CountryCodeId) REFERENCES CountryCodes (CountryCodeId);

-- Add CreatedAt column to CountryCodes table with default value and comment
ALTER TABLE CountryCodes
ADD CreatedAt DATETIME2 NOT NULL
    DEFAULT '0001-01-01T00:00:00.0000000';

-- Add Trashed column to CountryCodes table with default value
ALTER TABLE CountryCodes
ADD Trashed BIT NOT NULL
    DEFAULT 0;

-- Add UpdatedAt column to CountryCodes table with default value and comment
ALTER TABLE CountryCodes
ADD UpdatedAt DATETIME2 NOT NULL
    DEFAULT '0001-01-01T00:00:00.0000000';
