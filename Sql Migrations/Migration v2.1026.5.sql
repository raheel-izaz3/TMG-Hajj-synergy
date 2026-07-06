ALTER TABLE [CombinedFlightSummaryDetail] ADD[DepartureTime] nvarchar(max) NULL;

ALTER TABLE [FamilyHeadFlight] ALTER COLUMN [DepartureTime] time NULL;

ALTER TABLE [FamilyHeadFlight] ALTER COLUMN [ArrivalTime] time NULL;

ALTER TABLE [FamilyHeadTrain] ALTER COLUMN [ArrivalTime] time NULL;

ALTER TABLE [FamilyHeadTrain] ALTER COLUMN [DepartureTime] time NULL;

-- Up method: Adding the Currencies column
ALTER TABLE [Account]
ADD [Currencies] INT NULL;

-- Up method: Adding the ROE column
ALTER TABLE [NewPaymentVouchers]
ADD [ROE] REAL NULL DEFAULT 0;

select ProformaDate, *
from FamilyHeads
where FamilyHeadId = 261