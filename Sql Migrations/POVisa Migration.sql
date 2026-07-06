-- Adding columns to the Vendors table
ALTER TABLE Vendors
ADD AccountNumber NVARCHAR(MAX) NULL;

ALTER TABLE Vendors
ADD Currencies INT NOT NULL DEFAULT 0;

-- Creating the POVisas table
CREATE TABLE POVisas (
    POVisaId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyId INT NOT NULL,
    Date DATETIME2 NOT NULL,
    PONumber NVARCHAR(MAX) NULL,
    VatPercentage REAL NOT NULL,
    MunicipilityPercentage REAL NOT NULL,
    OtherTaxAmount REAL NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Amount REAL NOT NULL,
    Subtotal REAL NOT NULL,
    VAT REAL NOT NULL,
    MunicipilityTax REAL NOT NULL,
    OtherTaxes REAL NOT NULL,
    Total REAL NOT NULL,
    VendorId INT NOT NULL,
    Terms NVARCHAR(MAX) NULL,
    HajjSeasonsId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    Trashed BIT NOT NULL,
    CONSTRAINT FK_POVisas_Companies FOREIGN KEY (CompanyId) REFERENCES Companies (CompanyId) ON DELETE CASCADE,
    CONSTRAINT FK_POVisas_HajjSeasons FOREIGN KEY (HajjSeasonsId) REFERENCES HajjSeasons (HajjSeasonsId) ON DELETE CASCADE,
    CONSTRAINT FK_POVisas_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors (VendorId) ON DELETE CASCADE
);

-- Adding comments to CreatedAt and UpdatedAt columns in POVisas table
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Entity''s created date time', 
    @level0type = N'SCHEMA', @level0name = 'dbo', 
    @level1type = N'TABLE', @level1name = 'POVisas', 
    @level2type = N'COLUMN', @level2name = 'CreatedAt';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Entity''s last update date time', 
    @level0type = N'SCHEMA', @level0name = 'dbo', 
    @level1type = N'TABLE', @level1name = 'POVisas', 
    @level2type = N'COLUMN', @level2name = 'UpdatedAt';

-- Creating the POVisaInstallments table
CREATE TABLE POVisaInstallments (
    POVisaInstallmentId INT IDENTITY(1,1) PRIMARY KEY,
    POVisaId INT NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Percentage REAL NOT NULL,
    Amount REAL NOT NULL,
    CONSTRAINT FK_POVisaInstallments_POVisas FOREIGN KEY (POVisaId) REFERENCES POVisas (POVisaId) ON DELETE CASCADE
);
