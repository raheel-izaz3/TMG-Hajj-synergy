-- Rename column in Account Table
EXEC sp_rename 'Account.IsReceivable', 'IsHajjiAccount', 'COLUMN';

-- Add new column in Account Table
ALTER TABLE Account
ADD IsExpenseAccount BIT NOT NULL DEFAULT 0;

-- Add new column in Users Table
ALTER TABLE Users
ADD UserSignature NVARCHAR(MAX) NULL;

ALTER TABLE NewInvoice
ADD TPHBNo NVARCHAR(MAX) NULL;

ALTER TABLE NewInvoice
ADD TPHBReciptNo NVARCHAR(MAX) NULL;

-- <- 12/11/2024 ->

ALTER TABLE GeneralLedgers
ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_GeneralLedgers_CreatedAt DEFAULT GETDATE(),
    UpdatedAt datetime2 NOT NULL CONSTRAINT DF_GeneralLedgers_UpdatedAt DEFAULT GETDATE(),
    Trashed bit NOT NULL CONSTRAINT DF_GeneralLedgers_Trashed DEFAULT 0;

-- Drop the index on the 'CompanyId' column in the 'Vendors' table
DROP INDEX IX_Vendors_CompanyId ON Vendors;

-- Drop the 'CompanyId' column from the 'Vendors' table
ALTER TABLE Vendors
DROP COLUMN CompanyId;

-- Alter the 'CompanyId' column in the 'Account' table to be nullable
ALTER TABLE Account
ALTER COLUMN CompanyId INT NULL;

-- < 13/11/2024 >

ALTER TABLE Taxes
ADD TaxTypes INT NOT NULL DEFAULT 0;


-- < 24/12/2024 > 

CREATE TABLE CityTaxes (
    CityTaxId INT IDENTITY(1,1) NOT NULL,
    Code NVARCHAR(MAX) NULL,
    Name NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    Trashed BIT NOT NULL,
    CONSTRAINT PK_CityTaxes PRIMARY KEY (CityTaxId)
);

ALTER TABLE PackageFlights
ADD PNRGDSNumber NVARCHAR(MAX) NULL;

ALTER TABLE FamilyHeadFlight
ADD PNRGDSNo NVARCHAR(MAX) NULL;

-- Create POTicketInvoices Table
CREATE TABLE POTicketInvoices (
    POTicketInvoicesId INT IDENTITY(1,1) NOT NULL,
    POTicketHeadId INT NULL,
    POTicketId INT NULL,
    CONSTRAINT PK_POTicketInvoices PRIMARY KEY (POTicketInvoicesId)
);

-- Create POTickets Table
CREATE TABLE POTickets (
    POTicketId INT IDENTITY(1,1) NOT NULL,
    PONumber NVARCHAR(MAX) NULL,
    XONumber NVARCHAR(MAX) NULL,
    Date DATETIME2 NOT NULL,
    DueDate DATETIME2 NULL,
    FamilyHeadId INT NULL,
    TPRFNo NVARCHAR(MAX) NULL,
    FamilyMemberId INT NULL,
    CompanyId INT NULL,
    VendorTypeId INT NULL,
    VendorId INT NULL,
    BookingAgentId INT NULL,
    TicketingAgentId INT NULL,
    RecoveryOfficerId INT NULL,
    PostingAgentId INT NULL,
    TicketType INT NOT NULL,
    FareType INT NOT NULL,
    PostingType INT NOT NULL,
    IssueDate DATETIME2 NULL,
    BasicFare REAL NULL,
    SP REAL NULL,
    RG REAL NULL,
    SF REAL NULL,
    SVC REAL NULL,
    SRB REAL NULL,
    PK REAL NULL,
    PTT REAL NULL,
    PB REAL NULL,
    YQ REAL NULL,
    YR REAL NULL,
    KPR REAL NULL,
    KBP REAL NULL,
    CommissionPersentage REAL NULL,
    CommissionAmount REAL NULL,
    WithHoldingPers REAL NULL,
    WithHoldingAmount REAL NULL,
    SPTaxPers REAL NULL,
    SPTaxAmount REAL NULL,
    GrossClient REAL NULL,
    GrossAirline REAL NULL,
    NetClient REAL NULL,
    NetAirline REAL NULL,
    IsPrimary BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    Trashed BIT NOT NULL,
    CONSTRAINT PK_POTickets PRIMARY KEY (POTicketId)
);

-- Create POFlightDetails Table
CREATE TABLE POFlightDetails (
    POFlightDetailId INT IDENTITY(1,1) NOT NULL,
    TicketCategory INT NOT NULL,
    SeatNumber NVARCHAR(MAX) NULL,
    TicketNumber NVARCHAR(MAX) NULL,
    FlightNumber NVARCHAR(MAX) NULL,
    PNRNo NVARCHAR(MAX) NULL,
    PNRGDSNo NVARCHAR(MAX) NULL,
    FlightOrigion NVARCHAR(MAX) NULL,
    FlightDestination NVARCHAR(MAX) NULL,
    FlightClass INT NOT NULL,
    AirlineName NVARCHAR(MAX) NULL,
    DepartureDate DATETIME2 NOT NULL,
    CityTaxId INT NOT NULL,
    Amount REAL NOT NULL,
    POTicketId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    Trashed BIT NOT NULL,
    CONSTRAINT PK_POFlightDetails PRIMARY KEY (POFlightDetailId),
    CONSTRAINT FK_POFlightDetails_POTickets_POTicketId FOREIGN KEY (POTicketId)
        REFERENCES POTickets (POTicketId)
        ON DELETE CASCADE
);

-- Create POPayments Table
CREATE TABLE POPayments (
    PoPaymentId INT IDENTITY(1,1) NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Persentage REAL NULL,
    Amount REAL NULL,
    POTicketId INT NOT NULL,
    CONSTRAINT PK_POPayments PRIMARY KEY (PoPaymentId),
    CONSTRAINT FK_POPayments_POTickets_POTicketId FOREIGN KEY (POTicketId)
        REFERENCES POTickets (POTicketId)
        ON DELETE CASCADE
);

-- Create Index on POFlightDetails Table
CREATE INDEX IX_POFlightDetails_POTicketId
    ON POFlightDetails (POTicketId);

-- Create Index on POPayments Table
CREATE INDEX IX_POPayments_POTicketId
    ON POPayments (POTicketId);

ALTER TABLE FamilyHeads
ADD CnicUpload NVARCHAR(MAX) NULL;

-- Rename the column 'AirlineName' to 'PreferedMeals' in the 'POFlightDetails' table
EXEC sp_rename 'POFlightDetails.AirlineName', 'PreferedMeals', 'COLUMN';

-- Add the 'AirlineId' column to the 'POFlightDetails' table
ALTER TABLE POFlightDetails
ADD AirlineId INT NOT NULL DEFAULT 0;