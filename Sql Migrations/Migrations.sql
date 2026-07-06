--done 
ALTER TABLE Roles DROP COLUMN AccomodationModuleAllowed;
ALTER TABLE Roles DROP COLUMN InvoiceModuleAllowed;
ALTER TABLE Roles DROP COLUMN LogisticsModuleAllowed;

EXEC sp_rename 'Roles.ReportsModuleAllowed', 'ReceiptModuleAllowed', 'COLUMN';
EXEC sp_rename 'Roles.RegistrationModuleAllowed', 'DemandModuleAllowed', 'COLUMN';
EXEC sp_rename 'Roles.MemberModuleAllowed', 'AccommodationModuleAllowed', 'COLUMN';


ALTER TABLE MehramRelations ALTER COLUMN Code nvarchar(max) NULL;

ALTER TABLE Invoices ALTER COLUMN InvoiceType int NULL;

CREATE TABLE RolePrivileges (
    RolePrivilegesId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId INT NOT NULL,
    Privilege NVARCHAR(MAX) NULL
);

ALTER TABLE FamilyHeadTransports
ADD TransportType INT NOT NULL DEFAULT 0;


--done
ALTER TABLE [FamilyHeads] ADD [HajjiBookingStatus] int NOT NULL DEFAULT 0;

ALTER TABLE RateOfExchanges
ADD AEDROE REAL NOT NULL DEFAULT 0;

ALTER TABLE RateOfExchanges
ADD EUROROE REAL NOT NULL DEFAULT 0;

ALTER TABLE RateOfExchanges
ADD GBPROE REAL NOT NULL DEFAULT 0;


-- KAN 43 Package 
-- pending
ALTER TABLE IndividualFacilityDescriptions
ADD AccomodationId INT NULL;

ALTER TABLE IndividualFacilityDescriptions
ADD PlaceId INT NULL;

ALTER TABLE IndividualFacilityDescriptions
ADD RoomId INT NULL;


-- Add foreign key constraints
ALTER TABLE IndividualFacilityDescriptions
ADD CONSTRAINT FK_IndividualFacilityDescriptions_Accomodations_AccomodationId
FOREIGN KEY (AccomodationId) REFERENCES Accomodations(AccomodationId);

ALTER TABLE IndividualFacilityDescriptions
ADD CONSTRAINT FK_IndividualFacilityDescriptions_Places_PlaceId
FOREIGN KEY (PlaceId) REFERENCES Places(PlaceId);

ALTER TABLE IndividualFacilityDescriptions
ADD CONSTRAINT FK_IndividualFacilityDescriptions_Rooms_RoomId
FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId);


-- Add the RoomId column to the Packages table
ALTER TABLE Packages
ADD RoomId INT NULL;

-- Add the foreign key constraint to the RoomId column
ALTER TABLE Packages
ADD CONSTRAINT FK_Packages_Rooms_RoomId
FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId);




-- Create PackageCategoryFacilities table
CREATE TABLE PackageCategoryFacilities (
    PackageCategoryFacilitiesId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PackageId INT NOT NULL,
    IndividualFacilityDescriptionId INT NOT NULL,
    CONSTRAINT FK_PackageCategoryFacilities_IndividualFacilityDescriptions FOREIGN KEY (IndividualFacilityDescriptionId)
        REFERENCES IndividualFacilityDescriptions (IndividualFacilityDescriptionId)
        ON DELETE CASCADE,
    CONSTRAINT FK_PackageCategoryFacilities_Packages FOREIGN KEY (PackageId)
        REFERENCES Packages (PackageId)
        ON DELETE CASCADE
);




select * from Packages

select *
from PackageAccomodtions

update packages set roomid = 1

-- ALTER QUERY FOR LOAD ROOM/WASHROOM
alter table PackageAccomodtions
drop column loadperroom

alter table PackageAccomodtions
drop column loadperwashroom


--pending Currency Create 
CREATE TABLE Currencies (
    CurrencyId INT IDENTITY(1,1) PRIMARY KEY,
    CurrencyName NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    Trashed BIT NOT NULL
);

CREATE TABLE FamilyHeadFlight (
    FamilyHeadFlightId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    FlightType INT NOT NULL,
    AirlineId INT NOT NULL,
    FlightNo NVARCHAR(MAX),
    Origin NVARCHAR(MAX),
    Destination NVARCHAR(MAX),
    DepartureDateTime DATETIME2 NOT NULL,
    ArrivalDateTime DATETIME2 NOT NULL,
    PNRNo NVARCHAR(MAX),
    TicketNo NVARCHAR(MAX),
    TicketPDF NVARCHAR(MAX),
    CurrencyId INT NOT NULL,
    FareAmount NVARCHAR(MAX),
    Taxes NVARCHAR(MAX),
    Commission NVARCHAR(MAX),
    Total NVARCHAR(MAX),
    TicketStatus INT NOT NULL,
    CONSTRAINT FK_FamilyHeadFlight_Airlines FOREIGN KEY (AirlineId) REFERENCES Airlines(AirlineId) ON DELETE CASCADE,
    CONSTRAINT FK_FamilyHeadFlight_Currencies FOREIGN KEY (CurrencyId) REFERENCES Currencies(CurrencyId) ON DELETE CASCADE
);

-- Drop foreign keys
ALTER TABLE RouteVehiclePrices
DROP CONSTRAINT FK_RouteVehiclePrices_Routes_RouteId;

ALTER TABLE RouteVehiclePrices
DROP CONSTRAINT FK_RouteVehiclePrices_Vehicles_VehicleId;

-- Drop primary key
ALTER TABLE RouteVehiclePrices
DROP CONSTRAINT PK_RouteVehiclePrices;

-- Drop columns from specified tables
ALTER TABLE TravelRouteStops
DROP COLUMN SARPrice;

ALTER TABLE TravelRoutes
DROP COLUMN ROE;

ALTER TABLE TravelRoutes
DROP COLUMN RouteNumber;

ALTER TABLE TravelRoutes
DROP COLUMN StandardPrice;

ALTER TABLE FamilyHeadTransports
DROP COLUMN TimeIn;

ALTER TABLE FamilyHeadTransports
DROP COLUMN TimeOut;

-- Rename table
EXEC sp_rename 'RouteVehiclePrices', 'RouteVehiclePrice';

-- Rename index
EXEC sp_rename 'RouteVehiclePrices.IX_RouteVehiclePrices_VehicleId', 'IX_RouteVehiclePrice_VehicleId', 'INDEX';

-- Add primary key
ALTER TABLE RouteVehiclePrice
ADD CONSTRAINT PK_RouteVehiclePrice PRIMARY KEY (RouteId, VehicleId);

DROP TABLE RouteVehiclePrice;


CREATE TABLE FamilyHeadAccomodations (
    FamilyHeadAccomodationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HotelId INT NOT NULL,
    IndividualFacilityDescriptionId INT NOT NULL,
    CheckIn DATETIME2 NOT NULL,
    CheckOut DATETIME2 NOT NULL,
    FloorNumber INT NOT NULL,
    RoomNumber INT NOT NULL,
    GenderSegregated BIT NOT NULL,
    CONSTRAINT FK_FamilyHeadAccomodations_Hotels FOREIGN KEY (HotelId) 
        REFERENCES Hotels(HotelId) ON DELETE CASCADE,
    CONSTRAINT FK_FamilyHeadAccomodations_IndividualFacilityDescriptions FOREIGN KEY (IndividualFacilityDescriptionId) 
        REFERENCES IndividualFacilityDescriptions(IndividualFacilityDescriptionId) ON DELETE CASCADE
);

-- Drop Foreign Keys
ALTER TABLE PackageAccomodtions DROP CONSTRAINT FK_PackageAccomodtions_AccomodationCategories_AccomodationCategoryId;
ALTER TABLE PackageAccomodtions DROP CONSTRAINT FK_PackageAccomodtions_Accomodations_AccomodationId;
ALTER TABLE PackageAccomodtions DROP CONSTRAINT FK_PackageAccomodtions_FoodPackages_FoodPackageId;
ALTER TABLE PackageAccomodtions DROP CONSTRAINT FK_PackageAccomodtions_Places_PlaceId;
ALTER TABLE PackageAccomodtions DROP CONSTRAINT FK_PackageAccomodtions_Rooms_RoomId;
ALTER TABLE PackageCategoryFacilities DROP CONSTRAINT FK_PackageCategoryFacilities_IndividualFacilityDescriptions_IndividualFacilityDescriptionId;
ALTER TABLE Packages DROP CONSTRAINT FK_Packages_Rooms_RoomId;
ALTER TABLE PackageTrainings DROP CONSTRAINT FK_PackageTrainings_Trainings_TrainingId;

-- Alter columns
ALTER TABLE PackageTrainings ALTER COLUMN TrainingId INT NULL;
ALTER TABLE Packages ALTER COLUMN RoomId INT NULL;
ALTER TABLE Packages ALTER COLUMN PackageStatus INT NULL;
ALTER TABLE Packages ALTER COLUMN PackageState INT NULL;
ALTER TABLE Packages ALTER COLUMN PackageNumber INT NULL;
--ALTER TABLE Packages ALTER COLUMN FlightTicketPriceUsd REAL NOT NULL DEFAULT 0;
--ALTER TABLE Packages ALTER COLUMN FlightTicketPriceSar REAL NOT NULL DEFAULT 0;
--ALTER TABLE Packages ALTER COLUMN FlightTicketPricePkr REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageCategoryFacilities ALTER COLUMN IndividualFacilityDescriptionId INT NULL;
ALTER TABLE PackageAccomodtions ALTER COLUMN RoomId INT NULL;
ALTER TABLE PackageAccomodtions ALTER COLUMN PlaceId INT NULL;
ALTER TABLE PackageAccomodtions ALTER COLUMN FoodPackageId INT NULL;
ALTER TABLE PackageAccomodtions ALTER COLUMN AccomodationId INT NULL;
ALTER TABLE PackageAccomodtions ALTER COLUMN AccomodationCategoryId INT NULL;

-- Add Foreign Keys back
ALTER TABLE PackageAccomodtions
ADD CONSTRAINT FK_PackageAccomodtions_AccomodationCategories_AccomodationCategoryId
FOREIGN KEY (AccomodationCategoryId) REFERENCES AccomodationCategories(AccomodationCategoryId);

ALTER TABLE PackageAccomodtions
ADD CONSTRAINT FK_PackageAccomodtions_Accomodations_AccomodationId
FOREIGN KEY (AccomodationId) REFERENCES Accomodations(AccomodationId);

ALTER TABLE PackageAccomodtions
ADD CONSTRAINT FK_PackageAccomodtions_FoodPackages_FoodPackageId
FOREIGN KEY (FoodPackageId) REFERENCES FoodPackages(FoodPackageId);

ALTER TABLE PackageAccomodtions
ADD CONSTRAINT FK_PackageAccomodtions_Places_PlaceId
FOREIGN KEY (PlaceId) REFERENCES Places(PlaceId);

ALTER TABLE PackageAccomodtions
ADD CONSTRAINT FK_PackageAccomodtions_Rooms_RoomId
FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId);

ALTER TABLE PackageCategoryFacilities
ADD CONSTRAINT FK_PackageCategoryFacilities_IndividualFacilityDescriptions_IndividualFacilityDescriptionId
FOREIGN KEY (IndividualFacilityDescriptionId) REFERENCES IndividualFacilityDescriptions(IndividualFacilityDescriptionId);

ALTER TABLE Packages
ADD CONSTRAINT FK_Packages_Rooms_RoomId
FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId);

ALTER TABLE PackageTrainings
ADD CONSTRAINT FK_PackageTrainings_Trainings_TrainingId
FOREIGN KEY (TrainingId) REFERENCES Trainings(TrainingId);

-- Alter columns in FamilyHeadAccomodations table
ALTER TABLE FamilyHeadAccomodations
ALTER COLUMN RoomNumber NVARCHAR(MAX) NULL;

ALTER TABLE FamilyHeadAccomodations
ALTER COLUMN FloorNumber NVARCHAR(MAX) NULL;

ALTER TABLE FamilyHeadAccomodations
ALTER COLUMN CheckOut DATETIME2 NULL;

ALTER TABLE FamilyHeadAccomodations
ALTER COLUMN CheckIn DATETIME2 NULL;

-- Alter column in Airlines table
ALTER TABLE Airlines
ALTER COLUMN AirlineLogo NVARCHAR(MAX) NULL;




-- done
-- Add 'FamilyHeadId' column to 'FamilyHeadAccomodations' table
ALTER TABLE FamilyHeadAccomodations
ADD FamilyHeadId INT NOT NULL DEFAULT 0;

-- Create an index on 'FamilyHeadId' column in 'FamilyHeadAccomodations' table
CREATE INDEX IX_FamilyHeadAccomodations_FamilyHeadId
ON FamilyHeadAccomodations(FamilyHeadId);

-- Add a foreign key constraint between 'FamilyHeadAccomodations.FamilyHeadId' and 'FamilyHeads.FamilyHeadId'
ALTER TABLE FamilyHeadAccomodations
ADD CONSTRAINT FK_FamilyHeadAccomodations_FamilyHeads_FamilyHeadId
FOREIGN KEY (FamilyHeadId) REFERENCES FamilyHeads(FamilyHeadId)
ON DELETE CASCADE;

-- Drop 'GenderSegregated' column from 'FamilyHeadAccomodations' table
ALTER TABLE FamilyHeadAccomodations
DROP COLUMN GenderSegregated;


--  pending
CREATE TABLE [AuthorizationAgents] (
    [id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Contact] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [Trashed] bit NOT NULL,
    CONSTRAINT [PK_AuthorizationAgents] PRIMARY KEY ([id])
);

ALTER TABLE FamilyHeads
DROP COLUMN MISImage;







--new pending migrations
EXEC sp_rename 'FamilyHeadExtras.Price', 'PricePkr', 'COLUMN';


-- Add Price column to Vehicles table
ALTER TABLE [Vehicles] 
ADD [Price] REAL NOT NULL DEFAULT 0;

-- Add Price column to IndividualFacilityDescriptions table
ALTER TABLE [IndividualFacilityDescriptions] 
ADD [Price] REAL NOT NULL DEFAULT 0;

-- Add Price column to ExtraFacilityDescriptions table
ALTER TABLE [ExtraFacilityDescriptions] 
ADD [Price] REAL NOT NULL DEFAULT 0;



-- Drop foreign key constraint
ALTER TABLE [Packages] DROP CONSTRAINT [FK_Packages_Packages_OrignalPackageId];

-- Drop index on OrignalPackageId column
DROP INDEX [IX_Packages_OrignalPackageId] ON [Packages];

-- Drop columns from Packages table
ALTER TABLE [Packages] DROP COLUMN [CustomerAddress];
ALTER TABLE [Packages] DROP COLUMN [CustomerEmail];
ALTER TABLE [Packages] DROP COLUMN [CustomerName];
ALTER TABLE [Packages] DROP COLUMN [CustomerPhone];
ALTER TABLE [Packages] DROP COLUMN [NumberOfAdults];
ALTER TABLE [Packages] DROP COLUMN [NumberOfChilds];
ALTER TABLE [Packages] DROP COLUMN [NumberOfInfants];
ALTER TABLE [Packages] DROP COLUMN [OrignalPackageId];
ALTER TABLE [Packages] DROP COLUMN [ValidTill];


ALTER TABLE CompanyBankAccounts
ADD BankId INT NULL;

-- Establish a foreign key relationship with the Bank table
ALTER TABLE CompanyBankAccounts
ADD CONSTRAINT FK_CompanyBankAccounts_Bank
    FOREIGN KEY (BankId)              -- BankId in CompanyBankAccounts refers to
    REFERENCES Banks(BankId)           -- BankId in the Bank table
    ON DELETE SET NULL;               -- If a bank is deleted, set BankId to NULL
    
-- Drop foreign key constraint
ALTER TABLE [PackageAccomodtions] DROP CONSTRAINT [FK_PackageAccomodtions_Rooms_RoomId];

-- Drop index on RoomId column
DROP INDEX [IX_PackageAccomodtions_RoomId] ON [PackageAccomodtions];

-- Drop RoomId column from PackageAccomodtions table
ALTER TABLE [PackageAccomodtions] DROP COLUMN [RoomId];


CREATE TABLE [PackageFlights] (
    [PackageFlightsId] int NOT NULL IDENTITY,
    [FlightType] int NOT NULL,
    [AirlineId] int NULL,
    [FlightNumber] nvarchar(max) NULL,
    [FlightClass] int NOT NULL,
    [Origin] nvarchar(max) NULL,
    [Destination] nvarchar(max) NULL,
    [DepartureDateTime] datetime2 NULL,
    [ArrivalDateTime] datetime2 NULL,
    [PNRNumber] nvarchar(max) NULL,
    [TicketAmount] real NOT NULL,
    CONSTRAINT [PK_PackageFlights] PRIMARY KEY ([PackageFlightsId]),
    CONSTRAINT [FK_PackageFlights_Airlines_AirlineId] FOREIGN KEY ([AirlineId]) REFERENCES [Airlines] ([AirlineId])
);

CREATE TABLE [PackageTrains] (
    [PackageTrainsId] int NOT NULL IDENTITY,
    [RailwayId] int NULL,
    [TrainNumber] nvarchar(max) NULL,
    [TrainClass] int NOT NULL,
    [Origin] nvarchar(max) NULL,
    [Destination] nvarchar(max) NULL,
    [DepartureDateTime] datetime2 NULL,
    [ArrivalDateTime] datetime2 NULL,
    [PNRNumber] nvarchar(max) NULL,
    [TicketAmount] real NOT NULL,
    CONSTRAINT [PK_PackageTrains] PRIMARY KEY ([PackageTrainsId]),
    CONSTRAINT [FK_PackageTrains_Railways_RailwayId] FOREIGN KEY ([RailwayId]) REFERENCES [Railways] ([RailwayId])
);


-- new pending migration 
-- Alter the TrainClass column in the PackageTrains table to allow NULL values
ALTER TABLE PackageTrains
ALTER COLUMN TrainClass INT NULL;

-- Alter the TicketAmount column in the PackageTrains table to allow NULL values
ALTER TABLE PackageTrains
ALTER COLUMN TicketAmount REAL NULL;

-- Add the PackageId column to the PackageTrains table
ALTER TABLE PackageTrains
ADD PackageId INT NOT NULL DEFAULT 0;

-- Add the PackageId column to the PackageFlights table
ALTER TABLE PackageFlights
ADD PackageId INT NOT NULL DEFAULT 0;

-- Create an index for the PackageId column in the PackageTrains table
CREATE INDEX IX_PackageTrains_PackageId
ON PackageTrains (PackageId);

-- Create an index for the PackageId column in the PackageFlights table
CREATE INDEX IX_PackageFlights_PackageId
ON PackageFlights (PackageId);

-- Add a foreign key constraint on PackageId in the PackageFlights table, referencing the Packages table
ALTER TABLE PackageFlights
ADD CONSTRAINT FK_PackageFlights_Packages_PackageId
FOREIGN KEY (PackageId) REFERENCES Packages (PackageId)
ON DELETE CASCADE;

-- Add a foreign key constraint on PackageId in the PackageTrains table, referencing the Packages table
ALTER TABLE PackageTrains
ADD CONSTRAINT FK_PackageTrains_Packages_PackageId
FOREIGN KEY (PackageId) REFERENCES Packages (PackageId)
ON DELETE CASCADE;



--new TravelRouteStops migration pending

ALTER TABLE TravelRouteStops
ADD Price REAL NULL DEFAULT 0;


-- migration pending for new build 

-- Create CustomerPortfolios table
CREATE TABLE CustomerPortfolios (
    CustomerPortfolioId INT IDENTITY(1, 1) PRIMARY KEY,
    ApplicantType INT NOT NULL,
    IsHead BIT NOT NULL,
    SPOId INT NULL,
    FeedBy NVARCHAR(MAX) NULL,
    HonorificsId INT NULL,
    SurName NVARCHAR(MAX) NULL,
    GivenName NVARCHAR(MAX) NULL,
    FatherHusbandName NVARCHAR(MAX) NULL,
    CNIC NVARCHAR(MAX) NULL,
    DateOfBirth DATETIME2 NOT NULL,
    PlaceOfBirth NVARCHAR(MAX) NULL,
    Nationality NVARCHAR(MAX) NULL,
    Gender INT NOT NULL,
    BloodGroup NVARCHAR(MAX) NULL,
    FiqahId INT NULL,
    IsMarried BIT NOT NULL,
    IsEmployed BIT NOT NULL,
    EmployedAt NVARCHAR(MAX) NULL,
    Designation NVARCHAR(MAX) NULL,
    Residency NVARCHAR(MAX) NULL,
    PassportType NVARCHAR(MAX) NULL,
    CountryCode NVARCHAR(MAX) NULL,
    PassportNumber NVARCHAR(MAX) NULL,
    PassportDateOfIssue DATETIME2 NULL,
    PassportDateOfExpiry DATETIME2 NULL,
    ContactPerson BIT NOT NULL,
    MemberImage NVARCHAR(MAX) NULL,
    PassportImage NVARCHAR(MAX) NULL,
    PassportTokenImage NVARCHAR(MAX) NULL,
    TokenRemarks NVARCHAR(MAX) NULL,
    VisaImage NVARCHAR(MAX) NULL,
    FamilyHeadRelationId INT NULL,
    Tentative BIT NOT NULL,
    HajjSeasonsId INT NULL,
    CreatedAt DATETIME2 NOT NULL ,
    UpdatedAt DATETIME2 NOT NULL,
    Trashed BIT NOT NULL,
    FOREIGN KEY (FamilyHeadRelationId) REFERENCES FamilyHeadRelations(FamilyHeadRelationId),
    FOREIGN KEY (FiqahId) REFERENCES Fiqahs(FiqahId),
    FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId),
    FOREIGN KEY (HonorificsId) REFERENCES Honorifics(HonorificsId),
    FOREIGN KEY (SPOId) REFERENCES SPOs(SPOId)
);

-- Create CustomerPortfolioAddress table
CREATE TABLE CustomerPortfolioAddress (
    CustomerPortfolioAddressId INT IDENTITY(1, 1) PRIMARY KEY,
    CustomerPortfolioId INT NOT NULL,
    MemberAddressTypeId INT NULL,
    Address NVARCHAR(MAX) NULL,
    Country NVARCHAR(MAX) NULL,
    City NVARCHAR(MAX) NULL,
    ZipCode NVARCHAR(MAX) NULL,
    PoBox NVARCHAR(MAX) NULL,
    TehsilId INT NULL,
    Preferred BIT NOT NULL,
    FOREIGN KEY (CustomerPortfolioId) REFERENCES CustomerPortfolios(CustomerPortfolioId) ON DELETE CASCADE,
    FOREIGN KEY (MemberAddressTypeId) REFERENCES MemberAddressTypes(MemberAddressTypeId),
    FOREIGN KEY (TehsilId) REFERENCES Tehsils(TehsilId)
);

-- Create CustomerPortfolioContacts table
CREATE TABLE CustomerPortfolioContacts (
    CustomerPortfolioContactId INT IDENTITY(1, 1) PRIMARY KEY,
    CustomerPortfolioId INT NOT NULL,
    MemberContactTypeId INT NULL,
    Contact NVARCHAR(MAX) NULL,
    Preferred BIT NOT NULL,
    FOREIGN KEY (CustomerPortfolioId) REFERENCES CustomerPortfolios(CustomerPortfolioId) ON DELETE CASCADE,
    FOREIGN KEY (MemberContactTypeId) REFERENCES MemberContactTypes(MemberContactTypeId)
);

-- Create CustomerPortfolioEmails table
CREATE TABLE CustomerPortfolioEmails (
    CustomerPortfolioEmailId INT IDENTITY(1, 1) PRIMARY KEY,
    CustomerPortfolioId INT NOT NULL,
    MemberEmailTypeId INT NULL,
    Email NVARCHAR(MAX) NULL,
    Preferred BIT NOT NULL,
    FOREIGN KEY (CustomerPortfolioId) REFERENCES CustomerPortfolios(CustomerPortfolioId) ON DELETE CASCADE,
    FOREIGN KEY (MemberEmailTypeId) REFERENCES MemberEmailTypes(MemberEmailTypeId)
);

CREATE TABLE CustomerPortfolioDocuments (
    CustomerPortfolioDocumentsId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerPortfolioId INT NOT NULL,
    RequiredDocumentId INT NOT NULL,
    Recieved BIT NOT NULL,
    Remarks NVARCHAR(MAX),
    CONSTRAINT FK_CustomerPortfolioDocuments_CustomerPortfolios FOREIGN KEY (CustomerPortfolioId)
        REFERENCES CustomerPortfolios(CustomerPortfolioId) ON DELETE CASCADE,
    CONSTRAINT FK_CustomerPortfolioDocuments_RequiredDocuments FOREIGN KEY (RequiredDocumentId)
        REFERENCES RequiredDocuments(RequiredDocumentId) ON DELETE CASCADE
);

CREATE TABLE CustomerPortfoliosMembers (
    CustomerPortfolioMembersId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerPortfolioId INT NOT NULL,
    HeadId INT NOT NULL,
    IsHead BIT NOT NULL,
    CONSTRAINT FK_CustomerPortfoliosMembers_CustomerPortfolios FOREIGN KEY (CustomerPortfolioId)
        REFERENCES CustomerPortfolios(CustomerPortfolioId) ON DELETE CASCADE,
    CONSTRAINT FK_CustomerPortfoliosMembers_Head_CustomerPortfolios FOREIGN KEY (HeadId)
        REFERENCES CustomerPortfolios(CustomerPortfolioId) ON DELETE CASCADE
);

ALTER TABLE FamilyHeadTransports
ADD Price REAL NULL;

-- Drop the 'Code' column from the 'FamilyHeadExtras' table
ALTER TABLE FamilyHeadExtras
DROP COLUMN Code;

-- Add the 'Discount' column of type REAL to the 'FamilyHeadExtras' table
ALTER TABLE FamilyHeadExtras
ADD Discount REAL NULL;

-- new migrations pending 02-Oct-2024

ALTER TABLE FamilyHeads
DROP COLUMN BaseAmount;

EXEC sp_rename 'FamilyHeads.CurrencyId', 'Currencies', 'COLUMN';

ALTER TABLE Packages
ADD PKRROE real NOT NULL DEFAULT 0;

ALTER TABLE FamilyHeads
ADD GeneralDiscount real NULL;

ALTER TABLE FamilyHeadExtras
ADD Price real NULL;

EXEC sp_rename 'NewInvoice.CurrencyId', 'Currencies', 'COLUMN';

ALTER TABLE FamilyHeads
ADD AccomodationAsPerPackage bit NOT NULL DEFAULT 0;

ALTER TABLE ExtraFacilityDescriptions
ADD HajjSeasonId int NOT NULL DEFAULT 0;

ALTER TABLE ExtraFacilityDescriptions
ADD CONSTRAINT FK_ExtraFacilityDescriptions_HajjSeason
FOREIGN KEY (HajjSeasonId) REFERENCES HajjSeason(HajjSeasonId);


-- Rename the column in the 'ExtraFacilityDescriptions' table
EXEC sp_rename 'ExtraFacilityDescriptions.HajjSeasonId', 'HajjSeasonsId', 'COLUMN';

-- Add a foreign key constraint to the 'ExtraFacilityDescriptions' table
ALTER TABLE ExtraFacilityDescriptions
ADD CONSTRAINT FK_ExtraFacilityDescriptions_HajjSeasons_HajjSeasonsId
FOREIGN KEY (HajjSeasonsId)
REFERENCES HajjSeasons(HajjSeasonsId)
ON DELETE CASCADE;




--new migration pending 16-10-2024

ALTER TABLE HajjSeasons
ADD Hijri NVARCHAR(MAX) NULL;

ALTER TABLE NewInvoice
ADD CompanyBankAccountId INT NOT NULL DEFAULT 0;

CREATE INDEX IX_NewInvoice_CompanyBankAccountId
ON NewInvoice (CompanyBankAccountId);

ALTER TABLE NewInvoice
ADD CONSTRAINT FK_NewInvoice_CompanyBankAccounts_CompanyBankAccountId
FOREIGN KEY (CompanyBankAccountId) REFERENCES CompanyBankAccounts(CompanyBankAccountId)
ON DELETE NO ACTION;

ALTER TABLE NewInvoice
DROP CONSTRAINT FK_NewInvoice_Banks;

ALTER TABLE NewInvoice
DROP COLUMN BankId;

ALTER TABLE NewInvoice
DROP CONSTRAINT FK_NewInvoice_CompanyBankAccounts_CompanyBankAccountId;

ALTER TABLE NewInvoice
ALTER COLUMN CompanyBankAccountId INT NULL;

ALTER TABLE NewInvoice
ADD CONSTRAINT FK_NewInvoice_CompanyBankAccounts_CompanyBankAccountId
FOREIGN KEY (CompanyBankAccountId) REFERENCES CompanyBankAccounts(CompanyBankAccountId);

-- new pending migration for build starting from 21-oct-2024

ALTER TABLE ExtraFacilityDescriptions
DROP COLUMN Price;

-- Add the HajjSeasonsId column to the Rooms table
ALTER TABLE Rooms
ADD HajjSeasonsId INT NULL;

-- Create an index on the HajjSeasonsId column in the Rooms table
CREATE INDEX IX_Rooms_HajjSeasonsId
ON Rooms (HajjSeasonsId);

-- Add the foreign key constraint between Rooms and HajjSeasons tables
ALTER TABLE Rooms
ADD CONSTRAINT FK_Rooms_HajjSeasons_HajjSeasonsId
FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);

-- Add HajjSeasonsId column to each table
ALTER TABLE Vehicles ADD HajjSeasonsId INT NULL;
ALTER TABLE Vaccines ADD HajjSeasonsId INT NULL;
ALTER TABLE TravelRouteStops ADD HajjSeasonsId INT NULL;
ALTER TABLE TravelRoutes ADD HajjSeasonsId INT NULL;
ALTER TABLE Trainings ADD HajjSeasonsId INT NULL;
ALTER TABLE Routes ADD HajjSeasonsId INT NULL;
ALTER TABLE IssuingAgents ADD HajjSeasonsId INT NULL;
ALTER TABLE IndividualFacilityDescriptions ADD HajjSeasonsId INT NULL;
ALTER TABLE Hotels ADD HajjSeasonsId INT NULL;
ALTER TABLE Honorifics ADD HajjSeasonsId INT NULL;
ALTER TABLE Gifts ADD HajjSeasonsId INT NULL;
ALTER TABLE Fiqahs ADD HajjSeasonsId INT NULL;
ALTER TABLE Doctors ADD HajjSeasonsId INT NULL;
ALTER TABLE CompanyBankAccounts ADD HajjSeasonsId INT NULL;
ALTER TABLE Banks ADD HajjSeasonsId INT NULL;

-- Create indexes for HajjSeasonsId column in each table
CREATE INDEX IX_Vehicles_HajjSeasonsId ON Vehicles (HajjSeasonsId);
CREATE INDEX IX_Vaccines_HajjSeasonsId ON Vaccines (HajjSeasonsId);
CREATE INDEX IX_TravelRouteStops_HajjSeasonsId ON TravelRouteStops (HajjSeasonsId);
CREATE INDEX IX_TravelRoutes_HajjSeasonsId ON TravelRoutes (HajjSeasonsId);
CREATE INDEX IX_Trainings_HajjSeasonsId ON Trainings (HajjSeasonsId);
CREATE INDEX IX_Routes_HajjSeasonsId ON Routes (HajjSeasonsId);
CREATE INDEX IX_IssuingAgents_HajjSeasonsId ON IssuingAgents (HajjSeasonsId);
CREATE INDEX IX_IndividualFacilityDescriptions_HajjSeasonsId ON IndividualFacilityDescriptions (HajjSeasonsId);
CREATE INDEX IX_Hotels_HajjSeasonsId ON Hotels (HajjSeasonsId);
CREATE INDEX IX_Honorifics_HajjSeasonsId ON Honorifics (HajjSeasonsId);
CREATE INDEX IX_Gifts_HajjSeasonsId ON Gifts (HajjSeasonsId);
CREATE INDEX IX_Fiqahs_HajjSeasonsId ON Fiqahs (HajjSeasonsId);
CREATE INDEX IX_Doctors_HajjSeasonsId ON Doctors (HajjSeasonsId);
CREATE INDEX IX_CompanyBankAccounts_HajjSeasonsId ON CompanyBankAccounts (HajjSeasonsId);
CREATE INDEX IX_Banks_HajjSeasonsId ON Banks (HajjSeasonsId);

-- Add foreign key constraints for HajjSeasonsId in each table
ALTER TABLE Banks ADD CONSTRAINT FK_Banks_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE CompanyBankAccounts ADD CONSTRAINT FK_CompanyBankAccounts_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Doctors ADD CONSTRAINT FK_Doctors_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Fiqahs ADD CONSTRAINT FK_Fiqahs_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Gifts ADD CONSTRAINT FK_Gifts_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Honorifics ADD CONSTRAINT FK_Honorifics_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId) ON DELETE CASCADE;
ALTER TABLE Hotels ADD CONSTRAINT FK_Hotels_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE IndividualFacilityDescriptions ADD CONSTRAINT FK_IndividualFacilityDescriptions_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE IssuingAgents ADD CONSTRAINT FK_IssuingAgents_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Routes ADD CONSTRAINT FK_Routes_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Trainings ADD CONSTRAINT FK_Trainings_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE TravelRoutes ADD CONSTRAINT FK_TravelRoutes_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE TravelRouteStops ADD CONSTRAINT FK_TravelRouteStops_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Vaccines ADD CONSTRAINT FK_Vaccines_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);
ALTER TABLE Vehicles ADD CONSTRAINT FK_Vehicles_HajjSeasons_HajjSeasonsId FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons(HajjSeasonsId);

UPDATE Vehicles SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Vaccines SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE TravelRouteStops SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE TravelRoutes SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Trainings SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Routes SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE IssuingAgents SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE IndividualFacilityDescriptions SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Hotels SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Honorifics SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Gifts SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Fiqahs SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Doctors SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE CompanyBankAccounts SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Banks SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;
UPDATE Rooms SET HajjSeasonsId = 1 WHERE HajjSeasonsId IS NULL;

ALTER TABLE PackageTermsConditions
ADD HajjSeasonsId INT NULL;

CREATE INDEX IX_PackageTermsConditions_HajjSeasonsId
ON PackageTermsConditions (HajjSeasonsId);

ALTER TABLE PackageTermsConditions
ADD CONSTRAINT FK_PackageTermsConditions_HajjSeasons_HajjSeasonsId
FOREIGN KEY (HajjSeasonsId)
REFERENCES HajjSeasons (HajjSeasonsId);


-- Step 1: Drop the foreign key constraint
ALTER TABLE ExtraFacilityDescriptions
DROP CONSTRAINT FK_ExtraFacilityDescriptions_HajjSeasons_HajjSeasonsId;

-- Step 2: Drop the index on the HajjSeasonsId column
DROP INDEX IX_ExtraFacilityDescriptions_HajjSeasonsId
ON ExtraFacilityDescriptions;

-- Step 3: Drop the HajjSeasonsId column
ALTER TABLE ExtraFacilityDescriptions
DROP COLUMN HajjSeasonsId;


-- new migrations 23-oct-2024

-- Add the new column 'CompanyDepartmentId' to the 'DepartmentFilings' table
ALTER TABLE DepartmentFilings
ADD CompanyDepartmentId INT NOT NULL DEFAULT 0;

-- Create an index on the 'CompanyDepartmentId' column
CREATE INDEX IX_DepartmentFilings_CompanyDepartmentId
ON DepartmentFilings (CompanyDepartmentId);

-- Add the foreign key constraint on 'CompanyDepartmentId' that references 'CompanyDepartments'
ALTER TABLE DepartmentFilings
ADD CONSTRAINT FK_DepartmentFilings_CompanyDepartments_CompanyDepartmentId
FOREIGN KEY (CompanyDepartmentId)
REFERENCES CompanyDepartments (CompanyDepartmentId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;

-- Add the new column 'CompanyId' to the 'DepartmentFilings' table
ALTER TABLE DepartmentFilings
ADD CompanyId INT NOT NULL DEFAULT 0;

-- Create an index on the 'CompanyId' column
CREATE INDEX IX_DepartmentFilings_CompanyId
ON DepartmentFilings (CompanyId);

-- Add the foreign key constraint on 'CompanyId' that references 'Companies'
ALTER TABLE DepartmentFilings
ADD CONSTRAINT FK_DepartmentFilings_Companies_CompanyId
FOREIGN KEY (CompanyId)
REFERENCES Companies (CompanyId)
ON DELETE CASCADE;

-- new migration pending 26-oct-2024

ALTER TABLE PackageFlights
ADD IsPreffered BIT NOT NULL DEFAULT 0;

-- Rename columns
EXEC sp_rename 'PackageFlights.DepartureDateTime', 'DepartureDate', 'COLUMN';
EXEC sp_rename 'PackageFlights.ArrivalDateTime', 'ArrivalDate', 'COLUMN';

-- Add new columns
ALTER TABLE PackageFlights
ADD ArrivalTime TIME Null;

ALTER TABLE PackageFlights
ADD DepartureTime TIME NULL;

-- Rename columns
EXEC sp_rename 'PackageTrains.DepartureDateTime', 'DepartureDate', 'COLUMN';
EXEC sp_rename 'PackageTrains.ArrivalDateTime', 'ArrivalDate', 'COLUMN';

-- Add new columns
ALTER TABLE PackageTrains
ADD ArrivalTime TIME NULL;

ALTER TABLE PackageTrains
ADD DepartureTime TIME NULL;

-- Add new columns to FamilyHeadTrain
ALTER TABLE FamilyHeadTrain
ADD ArrivalTime DATETIME2 NULL;

ALTER TABLE FamilyHeadTrain
ADD DepartureTime DATETIME2 NULL;

-- Add new columns to FamilyHeadFlight
ALTER TABLE FamilyHeadFlight
ADD ArrivalTime DATETIME2 NULL;

ALTER TABLE FamilyHeadFlight
ADD DepartureTime DATETIME2 NULL;

ALTER TABLE Packages
ADD TermsnCondition NVARCHAR(MAX) NULL;


select *
from FamilyHeadFlight

select *
from FamilyMembers

INSERT INTO Places (PlaceName, Trashed, CreatedAt, UpdatedAt)
VALUES ('Arafat', 0, GETDATE(), GETDATE());


    select * from Places

    update Places set PlaceName = 'Mina' where PlaceId = 3

    select *
    from Packages


    select *
    from FamilyHeads
    where FamilyHeadId = 319


select *
from Packages 
where HajjSeasonsId = 2
and Trashed = 0

select *
from IndividualFacilityDescriptions
where IndividualFacilityDescriptionId = 13


select * 
from CustomerPortfolios
where HajjSeasonsId = 2

select *
from CustomerRemarksLogs

select *
from EVideoFilings

select *
from TravelRouteStops

select *
from PackageCategoryFacilities

select QurbaniIncluded
from Packages
where PackageId = 143

select *
from Packages

SELECT * 
FROM FamilyHeads
WHERE FamilyHeadId = 343


SET IDENTITY_INSERT AccountGroups ON;

INSERT INTO AccountGroups (AccountGroupId, Name, AccountTypesId, CreatedAt, UpdatedAt, Trashed) VALUES
(1, 'Current Asstes', 4, GETDATE(),GETDATE(), 0),
(2, 'Non Current Asstes', 4, GETDATE(),GETDATE(), 0),
(3, 'Service Revenue', 6, GETDATE(),GETDATE(), 0),
(4, 'Bank Expense', 7, GETDATE(),GETDATE(), 0);

SET IDENTITY_INSERT AccountGroups OFF;

-- AccountTypes Table Seed Data
INSERT INTO AccountTypes (Name, CreatedAt, UpdatedAt, Trashed, AccountNature)
VALUES 
    ('Asset', GETDATE(),GETDATE(), 0, 0),
    ('Expense', GETDATE(),GETDATE(), 0, 0),
    ('Revenue', GETDATE(),GETDATE(), 0, 1),
    ('Liability', GETDATE(),GETDATE(), 0, 1),
    ('Capital', GETDATE(),GETDATE(), 0, 1);

select *
from AccountTypes

select *
from Accounts

select *
from CompanyBankAccounts


select *
from FamilyHeads
where FamilyHeadId = 354

-- Add the 'IsQuery' column to the 'CustomerPortfolios' table
ALTER TABLE CustomerPortfolios 
ADD IsQuery BIT NULL;

-- Drop the 'IndividualFacilityDescriptionId' column from the 'Packages' table
ALTER TABLE Packages 
DROP COLUMN IndividualFacilityDescriptionId;

update CustomerPortfolios
set IsQuery = 0

ALTER TABLE CompanyBankAccounts
ADD AccountBalance REAL NOT NULL DEFAULT 0;

ALTER TABLE Account
ADD OpeningBalance REAL NULL;


CREATE TABLE [dbo].[BankTransfers] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [TransferDate] DATETIME2 NOT NULL,
    [ReferenceNumber] NVARCHAR(50) NOT NULL,
    [BTNumber] NVARCHAR(50) NOT NULL,
    [ROE] DECIMAL(18,4) NOT NULL,
    [ProofOfPaymentFile] NVARCHAR(MAX) NULL,
    [DebitAccountId] INT NOT NULL,
    [DebitBankCharges] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DebitOPAPTax] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DebitRemittanceTax] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DebitThirdPartyCharges] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CreditAccountId] INT NOT NULL,
    [CreditBankCharges] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CreditOPAPTax] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CreditRemittanceTax] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CreditThirdPartyCharges] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_BankTransfers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BankTransfers_CompanyBankAccounts_DebitAccountId] 
        FOREIGN KEY ([DebitAccountId]) REFERENCES [CompanyBankAccounts] ([Id]),
    CONSTRAINT [FK_BankTransfers_CompanyBankAccounts_CreditAccountId] 
        FOREIGN KEY ([CreditAccountId]) REFERENCES [CompanyBankAccounts] ([Id])
);

CREATE INDEX [IX_BankTransfers_DebitAccountId] ON [BankTransfers] ([DebitAccountId]);
CREATE INDEX [IX_BankTransfers_CreditAccountId] ON [BankTransfers] ([CreditAccountId]);


