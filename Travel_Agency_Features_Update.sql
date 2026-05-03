USE Travel_Agency;
GO

IF COL_LENGTH('dbo.Contact_Form', 'C_IsRead') IS NULL
BEGIN
    ALTER TABLE dbo.Contact_Form
    ADD C_IsRead BIT NOT NULL CONSTRAINT DF_ContactForm_IsRead DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Contact_Form', 'C_CreatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Contact_Form
    ADD C_CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ContactForm_CreatedAt DEFAULT (SYSUTCDATETIME());
END
GO

IF COL_LENGTH('dbo.Contact_Form', 'C_IsArchived') IS NULL
BEGIN
    ALTER TABLE dbo.Contact_Form
    ADD C_IsArchived BIT NOT NULL CONSTRAINT DF_ContactForm_IsArchived DEFAULT (0);
END
GO

IF OBJECT_ID('dbo.Hotels', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hotels (
        Hotel_Id INT PRIMARY KEY IDENTITY(1,1),
        Place_Id INT NOT NULL,
        Hotel_Name VARCHAR(150) NOT NULL,
        Hotel_Description VARCHAR(1000) NOT NULL,
        Hotel_Stars INT NOT NULL CONSTRAINT CK_Hotels_Stars CHECK (Hotel_Stars BETWEEN 1 AND 5),
        Hotel_Url VARCHAR(500) NULL,
        CONSTRAINT FK_Hotels_Places FOREIGN KEY (Place_Id) REFERENCES dbo.Places(Place_Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Travel_Packages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Travel_Packages (
        Package_Id INT PRIMARY KEY IDENTITY(1,1),
        Place_Id INT NOT NULL,
        Hotel_Id INT NOT NULL,
        Package_Name VARCHAR(150) NOT NULL,
        Package_Description VARCHAR(1200) NOT NULL,
        Price_Per_Person DECIMAL(10,2) NOT NULL CONSTRAINT CK_Packages_Price CHECK (Price_Per_Person > 0),
        Start_Date DATE NOT NULL,
        End_Date DATE NOT NULL,
        Available_Seats INT NOT NULL CONSTRAINT CK_Packages_Seats CHECK (Available_Seats >= 0),
        Package_Url VARCHAR(500) NULL,
        CONSTRAINT FK_Packages_Places FOREIGN KEY (Place_Id) REFERENCES dbo.Places(Place_Id),
        CONSTRAINT FK_Packages_Hotels FOREIGN KEY (Hotel_Id) REFERENCES dbo.Hotels(Hotel_Id),
        CONSTRAINT CK_Packages_Dates CHECK (End_Date >= Start_Date)
    );
END
GO

IF OBJECT_ID('dbo.Bookings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bookings (
        Booking_Id INT PRIMARY KEY IDENTITY(1,1),
        Package_Id INT NOT NULL,
        U_Id INT NOT NULL,
        Travelers INT NOT NULL CONSTRAINT CK_Bookings_Travelers CHECK (Travelers > 0),
        Total_Price DECIMAL(10,2) NOT NULL,
        Booking_Status VARCHAR(30) NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT ('Pending'),
        Booking_Date DATETIME2 NOT NULL CONSTRAINT DF_Bookings_Date DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Bookings_Packages FOREIGN KEY (Package_Id) REFERENCES dbo.Travel_Packages(Package_Id),
        CONSTRAINT FK_Bookings_Users FOREIGN KEY (U_Id) REFERENCES dbo.Users(U_Id),
        CONSTRAINT CK_Bookings_Status CHECK (Booking_Status IN ('Pending', 'Confirmed', 'Cancelled'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Hotels)
BEGIN
    INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
    SELECT Place_Id, 'Seine View Hotel', 'A central hotel near museums, cafes, and evening river walks.', 4, 'https://images.unsplash.com/photo-1566073771259-6a8506099945'
    FROM dbo.Places WHERE Place_Name = 'Paris';

    INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
    SELECT Place_Id, 'Ubud Garden Resort', 'A peaceful resort with garden villas, breakfast, and pool access.', 5, 'https://images.unsplash.com/photo-1571896349842-33c89424de2d'
    FROM dbo.Places WHERE Place_Name = 'Bali';

    INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
    SELECT Place_Id, 'Crystal Lagoon Villas', 'Private island villas with beach access and turquoise lagoon views.', 5, 'https://images.unsplash.com/photo-1582719508461-905c673771fd'
    FROM dbo.Places WHERE Place_Name = 'Maldives';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Place_Name = 'Maldives')
BEGIN
    INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
    VALUES ('Maldives', 'Enjoy luxury island escapes with crystal-clear water and white sand.', 'https://images.unsplash.com/photo-1514282401047-d79a71a590e8');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Hotels WHERE Hotel_Name = 'Crystal Lagoon Villas')
BEGIN
    INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
    SELECT Place_Id, 'Crystal Lagoon Villas', 'Private island villas with beach access and turquoise lagoon views.', 5, 'https://images.unsplash.com/photo-1582719508461-905c673771fd'
    FROM dbo.Places WHERE Place_Name = 'Maldives';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Travel_Packages)
BEGIN
    INSERT INTO dbo.Travel_Packages
        (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
    SELECT p.Place_Id, h.Hotel_Id, 'Paris Romantic Trip', 'Five nights in Paris with hotel, breakfast, and a guided city tour included.', 799.00, '2026-06-10', '2026-06-15', 18, 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34'
    FROM dbo.Places p INNER JOIN dbo.Hotels h ON h.Place_Id = p.Place_Id
    WHERE p.Place_Name = 'Paris' AND h.Hotel_Name = 'Seine View Hotel';

    INSERT INTO dbo.Travel_Packages
        (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
    SELECT p.Place_Id, h.Hotel_Id, 'Bali Culture Escape', 'Seven nights in Bali with resort stay, temple visits, breakfast, and airport transfer.', 1099.00, '2026-07-04', '2026-07-11', 22, 'https://images.unsplash.com/photo-1537953773345-d172ccf13cf1'
    FROM dbo.Places p INNER JOIN dbo.Hotels h ON h.Place_Id = p.Place_Id
    WHERE p.Place_Name = 'Bali' AND h.Hotel_Name = 'Ubud Garden Resort';

    INSERT INTO dbo.Travel_Packages
        (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
    SELECT p.Place_Id, h.Hotel_Id, 'Maldives Island Retreat', 'Six nights in a lagoon villa with breakfast, boat transfer, and snorkeling tour.', 1499.00, '2026-08-02', '2026-08-08', 12, 'https://images.unsplash.com/photo-1573843981267-be1999ff37cd'
    FROM dbo.Places p INNER JOIN dbo.Hotels h ON h.Place_Id = p.Place_Id
    WHERE p.Place_Name = 'Maldives' AND h.Hotel_Name = 'Crystal Lagoon Villas';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Travel_Packages WHERE Package_Name = 'Maldives Island Retreat')
BEGIN
    INSERT INTO dbo.Travel_Packages
        (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
    SELECT p.Place_Id, h.Hotel_Id, 'Maldives Island Retreat', 'Six nights in a lagoon villa with breakfast, boat transfer, and snorkeling tour.', 1499.00, '2026-08-02', '2026-08-08', 12, 'https://images.unsplash.com/photo-1573843981267-be1999ff37cd'
    FROM dbo.Places p INNER JOIN dbo.Hotels h ON h.Place_Id = p.Place_Id
    WHERE p.Place_Name = 'Maldives' AND h.Hotel_Name = 'Crystal Lagoon Villas';
END
GO
