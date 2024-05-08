-- Create the TaxiTrips table
CREATE TABLE TaxiTrips (
    PickupDateTimeUtc DATETIME2,
    DropOffDateTimeUtc DATETIME2,
    PassengerCount INT,
    TripDistance DECIMAL(10, 2),
    StoreAndFwdFlag VARCHAR(3),
    PULocationID INT, 
    DOLocationID INT,
    FareAmount DECIMAL(10, 2),
    TipAmount DECIMAL(10, 2),
    CONSTRAINT PK_TaxiTrips PRIMARY KEY (PickupDateTimeUtc, DropoffDateTimeUtc, PassengerCount)
);

-- Create an index on PULocationID for optimized search
CREATE INDEX IX_PULocationID ON TaxiTrips (PULocationID);

-- Create an index on tip_amount for finding the highest tip_amount on average
CREATE INDEX IX_TipAmount ON TaxiTrips (TipAmount);
