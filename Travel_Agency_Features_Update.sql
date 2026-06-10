USE Travel_Agency;
GO

IF COL_LENGTH('dbo.Users', 'U_Type') IS NULL
BEGIN
    ALTER TABLE dbo.Users
    ADD U_Type VARCHAR(20) NOT NULL CONSTRAINT DF_Users_U_Type DEFAULT ('user');
END
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
        Travel_Date DATE NULL,
        Booking_Date DATETIME2 NOT NULL CONSTRAINT DF_Bookings_Date DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Bookings_Packages FOREIGN KEY (Package_Id) REFERENCES dbo.Travel_Packages(Package_Id),
        CONSTRAINT FK_Bookings_Users FOREIGN KEY (U_Id) REFERENCES dbo.Users(U_Id),
        CONSTRAINT CK_Bookings_Status CHECK (Booking_Status IN ('Pending', 'Confirmed', 'Cancelled'))
    );
END
GO

IF COL_LENGTH('dbo.Bookings', 'Travel_Date') IS NULL
BEGIN
    ALTER TABLE dbo.Bookings ADD Travel_Date DATE NULL;
END
GO

IF OBJECT_ID('dbo.Password_Reset_Codes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Password_Reset_Codes (
        Reset_Id INT PRIMARY KEY IDENTITY(1,1),
        U_Email VARCHAR(255) NOT NULL,
        Reset_Code_Hash VARCHAR(255) NOT NULL,
        Expires_At DATETIME2 NOT NULL,
        Is_Used BIT NOT NULL CONSTRAINT DF_ResetCodes_IsUsed DEFAULT (0),
        Created_At DATETIME2 NOT NULL CONSTRAINT DF_ResetCodes_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE U_Email = 'auditor@travelagency.com')
BEGIN
    INSERT INTO dbo.Users (U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Password, U_RepeatPassword, U_Type)
    VALUES (
        'Audit',
        'Reviewer',
        'auditor@travelagency.com',
        'auditor',
        '',
        'FC3152BA74C04A29D7ABFF83F689D7641F6289EA84D3FF033E1D2667D765A03D',
        'FC3152BA74C04A29D7ABFF83F689D7641F6289EA84D3FF033E1D2667D765A03D',
        'auditor'
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

INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
SELECT v.Place_Name, v.Place_Description, v.Place_Url
FROM (VALUES
    ('Tokyo', 'Explore neon streets, quiet temples, world-class food, and modern city culture.', 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf'),
    ('Rome', 'Walk through ancient ruins, lively piazzas, art-filled churches, and classic Italian cafes.', 'https://images.unsplash.com/photo-1529260830199-42c24126f198'),
    ('New York', 'Experience skyline views, museums, Broadway energy, parks, and diverse neighborhoods.', 'https://images.unsplash.com/photo-1485871981521-5b1fd3805eee'),
    ('Dubai', 'Visit desert landscapes, luxury shopping, bold architecture, beaches, and rooftop views.', 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c'),
    ('Cape Town', 'Discover mountain scenery, coastal drives, vineyards, beaches, and vibrant markets.', 'https://images.unsplash.com/photo-1580060839134-75a5edca2e99'),
    ('Kyoto', 'Enjoy historic shrines, bamboo paths, tea houses, gardens, and seasonal blossoms.', 'https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e'),
    ('Istanbul', 'Cross continents through bazaars, mosques, waterfront views, and rich food traditions.', 'https://images.unsplash.com/photo-1524231757912-21f4fe3a7200'),
    ('Barcelona', 'See Gaudi landmarks, Mediterranean beaches, tapas streets, and colorful neighborhoods.', 'https://images.unsplash.com/photo-1539037116277-4db20889f2d4'),
    ('Sydney', 'Enjoy harbor views, beaches, coastal walks, wildlife parks, and a relaxed city pace.', 'https://images.unsplash.com/photo-1506973035872-a4ec16b8e8d9'),
    ('Cairo', 'Explore pyramids, museums, Nile views, historic streets, and ancient Egyptian heritage.', 'https://images.unsplash.com/photo-1572252009286-268acec5ca0a'),
    ('Santorini', 'Visit whitewashed villages, caldera views, sunset terraces, and volcanic beaches.', 'https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff'),
    ('Marrakech', 'Explore colorful souks, garden courtyards, desert day trips, and Moroccan cuisine.', 'https://images.unsplash.com/photo-1597212618440-806262de4f6b'),
    ('Singapore', 'Enjoy futuristic gardens, skyline dining, cultural districts, and waterfront walks.', 'https://images.unsplash.com/photo-1525625293386-3f8f99389edd'),
    ('Vancouver', 'Discover mountain views, harbor paths, forests, food markets, and outdoor adventures.', 'https://images.unsplash.com/photo-1559511260-66a654ae982a')
) AS v(Place_Name, Place_Description, Place_Url)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Places p
    WHERE p.Place_Name = v.Place_Name
);
GO

INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
SELECT p.Place_Id, v.Hotel_Name, v.Hotel_Description, v.Hotel_Stars, v.Hotel_Url
FROM (VALUES
    ('Paris', 'Seine View Hotel', 'A central Paris hotel near museums, cafes, and evening river walks.', 4, 'https://images.unsplash.com/photo-1566073771259-6a8506099945'),
    ('Bali', 'Ubud Garden Resort', 'A peaceful Bali resort with garden villas, breakfast, and pool access.', 5, 'https://images.unsplash.com/photo-1571896349842-33c89424de2d'),
    ('Maldives', 'Crystal Lagoon Villas', 'Private island villas with beach access and turquoise lagoon views.', 5, 'https://images.unsplash.com/photo-1582719508461-905c673771fd'),
    ('Tokyo', 'Shinjuku Skyline Hotel', 'A modern Tokyo hotel close to transit, restaurants, shopping streets, and city views.', 4, 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb'),
    ('Rome', 'Colosseum Boutique Hotel', 'A warm Rome stay near historic landmarks, piazzas, trattorias, and evening walks.', 4, 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa'),
    ('New York', 'Manhattan Central Hotel', 'A convenient New York hotel near museums, theaters, parks, and skyline viewpoints.', 4, 'https://images.unsplash.com/photo-1564501049412-61c2a3083791'),
    ('Dubai', 'Marina Dunes Resort', 'A stylish Dubai resort with beach access, skyline views, dining, and desert tour pickup.', 5, 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b'),
    ('Cape Town', 'Table Mountain Lodge', 'A Cape Town lodge with mountain views, coastal access, breakfast, and market connections.', 4, 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461'),
    ('Kyoto', 'Gion Garden Inn', 'A quiet Kyoto inn near temples, gardens, tea houses, and traditional streets.', 4, 'https://images.unsplash.com/photo-1571896349842-33c89424de2d'),
    ('Istanbul', 'Bosphorus Heritage Hotel', 'An Istanbul hotel near bazaars, historic mosques, ferry routes, and waterfront dining.', 4, 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa'),
    ('Barcelona', 'Gothic Quarter Suites', 'A Barcelona stay near Gaudi landmarks, tapas streets, beaches, and neighborhood walks.', 4, 'https://images.unsplash.com/photo-1566073771259-6a8506099945'),
    ('Sydney', 'Harbour Lights Hotel', 'A Sydney hotel with harbor access, breakfast, beach transfers, and city connections.', 4, 'https://images.unsplash.com/photo-1582719508461-905c673771fd'),
    ('Cairo', 'Nile Pyramid Hotel', 'A Cairo hotel with museum access, guided pickup options, Nile views, and local dining.', 4, 'https://images.unsplash.com/photo-1564501049412-61c2a3083791'),
    ('Santorini', 'Caldera Sunset Villas', 'Santorini villas with caldera views, terrace breakfasts, beach access, and sunset walks.', 5, 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b'),
    ('Marrakech', 'Medina Garden Riad', 'A Marrakech riad with courtyard rooms, breakfast, souk access, and desert excursion pickup.', 4, 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461'),
    ('Singapore', 'Marina Bay Stay', 'A Singapore hotel near gardens, waterfront dining, cultural districts, and metro links.', 4, 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb'),
    ('Vancouver', 'Pacific Mountain Hotel', 'A Vancouver hotel close to harbor paths, food markets, forest trails, and mountain views.', 4, 'https://images.unsplash.com/photo-1564501049412-61c2a3083791')
) AS v(Place_Name, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
INNER JOIN dbo.Places p ON p.Place_Name = v.Place_Name
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Hotels h
    WHERE h.Hotel_Name = v.Hotel_Name
);
GO

INSERT INTO dbo.Travel_Packages
    (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
SELECT p.Place_Id, h.Hotel_Id, v.Package_Name, v.Package_Description, v.Price_Per_Person, v.Start_Date, v.End_Date, v.Available_Seats, v.Package_Url
FROM (VALUES
    ('Paris', 'Seine View Hotel', 'Paris Romantic Trip', 'Five nights in Paris with hotel, breakfast, and a guided city tour included.', 799.00, CONVERT(date, '2026-06-10'), CONVERT(date, '2026-06-15'), 18, 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34'),
    ('Bali', 'Ubud Garden Resort', 'Bali Culture Escape', 'Seven nights in Bali with resort stay, temple visits, breakfast, and airport transfer.', 1099.00, CONVERT(date, '2026-07-04'), CONVERT(date, '2026-07-11'), 22, 'https://images.unsplash.com/photo-1537953773345-d172ccf13cf1'),
    ('Maldives', 'Crystal Lagoon Villas', 'Maldives Island Retreat', 'Six nights in a lagoon villa with breakfast, boat transfer, and snorkeling tour.', 1499.00, CONVERT(date, '2026-08-02'), CONVERT(date, '2026-08-08'), 12, 'https://images.unsplash.com/photo-1573843981267-be1999ff37cd'),
    ('Tokyo', 'Shinjuku Skyline Hotel', 'Neon Tokyo Discovery', 'Six nights in Tokyo with hotel, breakfast, metro access guidance, and a city highlights tour.', 1299.00, CONVERT(date, '2026-09-05'), CONVERT(date, '2026-09-11'), 20, 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf'),
    ('Rome', 'Colosseum Boutique Hotel', 'Rome Ancient Wonders', 'Five nights in Rome with hotel, breakfast, guided landmark visits, and food district time.', 899.00, CONVERT(date, '2026-09-18'), CONVERT(date, '2026-09-23'), 18, 'https://images.unsplash.com/photo-1529260830199-42c24126f198'),
    ('New York', 'Manhattan Central Hotel', 'New York City Lights', 'Four nights in New York with hotel, breakfast, skyline viewpoint access, and museum time.', 1199.00, CONVERT(date, '2026-10-03'), CONVERT(date, '2026-10-07'), 16, 'https://images.unsplash.com/photo-1485871981521-5b1fd3805eee'),
    ('Dubai', 'Marina Dunes Resort', 'Dubai Desert and Skyline', 'Five nights in Dubai with resort stay, breakfast, desert safari, and marina evening tour.', 1399.00, CONVERT(date, '2026-10-15'), CONVERT(date, '2026-10-20'), 20, 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c'),
    ('Cape Town', 'Table Mountain Lodge', 'Cape Town Coast Adventure', 'Six nights in Cape Town with lodge stay, breakfast, coastal drive, and mountain day tour.', 999.00, CONVERT(date, '2026-11-02'), CONVERT(date, '2026-11-08'), 14, 'https://images.unsplash.com/photo-1580060839134-75a5edca2e99'),
    ('Kyoto', 'Gion Garden Inn', 'Kyoto Temples and Tea', 'Five nights in Kyoto with inn stay, breakfast, temple visits, garden walks, and tea tasting.', 1099.00, CONVERT(date, '2026-11-14'), CONVERT(date, '2026-11-19'), 15, 'https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e'),
    ('Istanbul', 'Bosphorus Heritage Hotel', 'Istanbul Two Continents', 'Five nights in Istanbul with hotel, breakfast, bazaar tour, ferry ride, and historic district walk.', 849.00, CONVERT(date, '2026-12-01'), CONVERT(date, '2026-12-06'), 22, 'https://images.unsplash.com/photo-1524231757912-21f4fe3a7200'),
    ('Barcelona', 'Gothic Quarter Suites', 'Barcelona Art and Beach', 'Five nights in Barcelona with hotel, breakfast, Gaudi tour, tapas evening, and beach time.', 899.00, CONVERT(date, '2026-12-10'), CONVERT(date, '2026-12-15'), 19, 'https://images.unsplash.com/photo-1539037116277-4db20889f2d4'),
    ('Sydney', 'Harbour Lights Hotel', 'Sydney Harbour Escape', 'Six nights in Sydney with hotel, breakfast, harbor walk, beach transfer, and wildlife visit.', 1299.00, CONVERT(date, '2027-01-08'), CONVERT(date, '2027-01-14'), 17, 'https://images.unsplash.com/photo-1506973035872-a4ec16b8e8d9'),
    ('Cairo', 'Nile Pyramid Hotel', 'Cairo Pyramids Journey', 'Five nights in Cairo with hotel, breakfast, pyramid visit, museum time, and Nile evening.', 799.00, CONVERT(date, '2027-01-22'), CONVERT(date, '2027-01-27'), 18, 'https://images.unsplash.com/photo-1572252009286-268acec5ca0a'),
    ('Santorini', 'Caldera Sunset Villas', 'Santorini Sunset Getaway', 'Five nights in Santorini with villa stay, breakfast, caldera walk, beach visit, and sunset dinner.', 1399.00, CONVERT(date, '2027-02-05'), CONVERT(date, '2027-02-10'), 12, 'https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff'),
    ('Marrakech', 'Medina Garden Riad', 'Marrakech Medina Escape', 'Five nights in Marrakech with riad stay, breakfast, souk tour, garden visit, and desert day trip.', 749.00, CONVERT(date, '2027-02-18'), CONVERT(date, '2027-02-23'), 20, 'https://images.unsplash.com/photo-1597212618440-806262de4f6b'),
    ('Singapore', 'Marina Bay Stay', 'Singapore Garden City', 'Four nights in Singapore with hotel, breakfast, gardens visit, waterfront evening, and food tour.', 999.00, CONVERT(date, '2027-03-04'), CONVERT(date, '2027-03-08'), 18, 'https://images.unsplash.com/photo-1525625293386-3f8f99389edd'),
    ('Vancouver', 'Pacific Mountain Hotel', 'Vancouver Mountain and Harbor', 'Six nights in Vancouver with hotel, breakfast, harbor walk, forest trail, and mountain day trip.', 1099.00, CONVERT(date, '2027-03-18'), CONVERT(date, '2027-03-24'), 16, 'https://images.unsplash.com/photo-1559511260-66a654ae982a')
) AS v(Place_Name, Hotel_Name, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
INNER JOIN dbo.Places p ON p.Place_Name = v.Place_Name
INNER JOIN dbo.Hotels h ON h.Place_Id = p.Place_Id AND h.Hotel_Name = v.Hotel_Name
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Travel_Packages tp
    WHERE tp.Package_Name = v.Package_Name
);
GO
IF OBJECT_ID('dbo.User_Mfa_Codes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.User_Mfa_Codes (
        Mfa_Id INT PRIMARY KEY IDENTITY(1,1),
        U_Id INT NOT NULL,
        U_Email VARCHAR(255) NOT NULL,
        Code_Hash VARCHAR(255) NOT NULL,
        Expires_At DATETIME2 NOT NULL,
        Is_Used BIT NOT NULL CONSTRAINT DF_UserMfaCodes_IsUsed DEFAULT (0),
        Created_At DATETIME2 NOT NULL CONSTRAINT DF_UserMfaCodes_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID('dbo.Audit_Logs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Audit_Logs (
        Audit_Id INT PRIMARY KEY IDENTITY(1,1),
        Event_Type VARCHAR(100) NOT NULL,
        User_Email VARCHAR(255) NULL,
        U_Id INT NULL,
        Request_Path VARCHAR(255) NOT NULL,
        Http_Method VARCHAR(20) NOT NULL,
        Status_Code INT NOT NULL,
        Details VARCHAR(MAX) NULL,
        Created_At DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID('dbo.Metrics_Snapshots', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Metrics_Snapshots (
        Snapshot_Id INT PRIMARY KEY IDENTITY(1,1),
        Users_Count INT NOT NULL,
        Bookings_Count INT NOT NULL,
        Packages_Count INT NOT NULL,
        Unread_Messages_Count INT NOT NULL,
        Recorded_At DATETIME2 NOT NULL CONSTRAINT DF_MetricsSnapshots_RecordedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO
