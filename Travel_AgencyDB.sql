CREATE DATABASE Travel_Agency;
GO

USE Travel_Agency;
GO

IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID('dbo.Password_Reset_Codes', 'U') IS NOT NULL DROP TABLE dbo.Password_Reset_Codes;
IF OBJECT_ID('dbo.Travel_Packages', 'U') IS NOT NULL DROP TABLE dbo.Travel_Packages;
IF OBJECT_ID('dbo.Hotels', 'U') IS NOT NULL DROP TABLE dbo.Hotels;
IF OBJECT_ID('dbo.Contact_Form', 'U') IS NOT NULL DROP TABLE dbo.Contact_Form;
IF OBJECT_ID('dbo.Places', 'U') IS NOT NULL DROP TABLE dbo.Places;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    U_Id INT PRIMARY KEY IDENTITY(1,1),
    U_Name VARCHAR(100) NOT NULL,
    U_Surname VARCHAR(100) NOT NULL,
    U_Email VARCHAR(255) NOT NULL UNIQUE,
    U_Username VARCHAR(100) NOT NULL UNIQUE,
    U_Phone VARCHAR(30) NULL,
    U_Password VARCHAR(255) NOT NULL,
    U_RepeatPassword VARCHAR(255) NOT NULL,
    U_Type VARCHAR(20) NOT NULL CONSTRAINT DF_Users_U_Type DEFAULT ('user')
);
GO

CREATE TABLE dbo.Places (
    Place_Id INT PRIMARY KEY IDENTITY(1,1),
    Place_Name VARCHAR(150) NOT NULL,
    Place_Description VARCHAR(1000) NOT NULL,
    Place_Url VARCHAR(500) NULL
);
GO

CREATE TABLE dbo.Contact_Form (
    C_Id INT PRIMARY KEY IDENTITY(1,1),
    C_Name VARCHAR(100) NOT NULL,
    C_Surname VARCHAR(100) NOT NULL,
    C_Email VARCHAR(255) NOT NULL,
    C_Subject VARCHAR(150) NOT NULL,
    C_Message VARCHAR(MAX) NOT NULL,
    U_Id INT NULL,
    C_IsRead BIT NOT NULL CONSTRAINT DF_ContactForm_IsRead DEFAULT (0),
    C_IsArchived BIT NOT NULL CONSTRAINT DF_ContactForm_IsArchived DEFAULT (0),
    C_CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ContactForm_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_ContactForm_User FOREIGN KEY (U_Id) REFERENCES dbo.Users(U_Id)
);
GO

CREATE TABLE dbo.Password_Reset_Codes (
    Reset_Id INT PRIMARY KEY IDENTITY(1,1),
    U_Email VARCHAR(255) NOT NULL,
    Reset_Code_Hash VARCHAR(255) NOT NULL,
    Expires_At DATETIME2 NOT NULL,
    Is_Used BIT NOT NULL CONSTRAINT DF_ResetCodes_IsUsed DEFAULT (0),
    Created_At DATETIME2 NOT NULL CONSTRAINT DF_ResetCodes_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

CREATE TABLE dbo.Hotels (
    Hotel_Id INT PRIMARY KEY IDENTITY(1,1),
    Place_Id INT NOT NULL,
    Hotel_Name VARCHAR(150) NOT NULL,
    Hotel_Description VARCHAR(1000) NOT NULL,
    Hotel_Stars INT NOT NULL CONSTRAINT CK_Hotels_Stars CHECK (Hotel_Stars BETWEEN 1 AND 5),
    Hotel_Url VARCHAR(500) NULL,
    CONSTRAINT FK_Hotels_Places FOREIGN KEY (Place_Id) REFERENCES dbo.Places(Place_Id) ON DELETE CASCADE
);
GO

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
GO

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
GO

INSERT INTO dbo.Users (U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Password, U_RepeatPassword, U_Type)
VALUES
('Admin', 'User', 'admin@travelagency.com', 'admin', '',
 '3EB3FE66B31E3B4D10FA70B5CAD49C7112294AF6AE4E476A1C405155D45AA121',
 '3EB3FE66B31E3B4D10FA70B5CAD49C7112294AF6AE4E476A1C405155D45AA121',
 'admin');
GO

INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
VALUES
('Paris', 'Discover iconic landmarks, art, food, and romantic city walks.', 'https://images.unsplash.com/photo-1499856871958-5b9627545d1a'),
('Bali', 'Relax on tropical beaches and explore temples, rice terraces, and culture.', 'https://images.unsplash.com/photo-1537996194471-e657df975ab4'),
('Maldives', 'Enjoy luxury island escapes with crystal-clear water and white sand.', 'https://images.unsplash.com/photo-1514282401047-d79a71a590e8'),
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
('Vancouver', 'Discover mountain views, harbor paths, forests, food markets, and outdoor adventures.', 'https://images.unsplash.com/photo-1559511260-66a654ae982a');
GO

INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
VALUES
(1, 'Seine View Hotel', 'A central Paris hotel near museums, cafes, and evening river walks.', 4, 'https://images.unsplash.com/photo-1566073771259-6a8506099945'),
(2, 'Ubud Garden Resort', 'A peaceful Bali resort with garden villas, breakfast, and pool access.', 5, 'https://images.unsplash.com/photo-1571896349842-33c89424de2d'),
(3, 'Crystal Lagoon Villas', 'Private island villas with beach access and turquoise lagoon views.', 5, 'https://images.unsplash.com/photo-1582719508461-905c673771fd'),
(4, 'Shinjuku Skyline Hotel', 'A modern Tokyo hotel close to transit, restaurants, shopping streets, and city views.', 4, 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb'),
(5, 'Colosseum Boutique Hotel', 'A warm Rome stay near historic landmarks, piazzas, trattorias, and evening walks.', 4, 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa'),
(6, 'Manhattan Central Hotel', 'A convenient New York hotel near museums, theaters, parks, and skyline viewpoints.', 4, 'https://images.unsplash.com/photo-1564501049412-61c2a3083791'),
(7, 'Marina Dunes Resort', 'A stylish Dubai resort with beach access, skyline views, dining, and desert tour pickup.', 5, 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b'),
(8, 'Table Mountain Lodge', 'A Cape Town lodge with mountain views, coastal access, breakfast, and market connections.', 4, 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461'),
(9, 'Gion Garden Inn', 'A quiet Kyoto inn near temples, gardens, tea houses, and traditional streets.', 4, 'https://images.unsplash.com/photo-1571896349842-33c89424de2d'),
(10, 'Bosphorus Heritage Hotel', 'An Istanbul hotel near bazaars, historic mosques, ferry routes, and waterfront dining.', 4, 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa'),
(11, 'Gothic Quarter Suites', 'A Barcelona stay near Gaudi landmarks, tapas streets, beaches, and neighborhood walks.', 4, 'https://images.unsplash.com/photo-1566073771259-6a8506099945'),
(12, 'Harbour Lights Hotel', 'A Sydney hotel with harbor access, breakfast, beach transfers, and city connections.', 4, 'https://images.unsplash.com/photo-1582719508461-905c673771fd'),
(13, 'Nile Pyramid Hotel', 'A Cairo hotel with museum access, guided pickup options, Nile views, and local dining.', 4, 'https://images.unsplash.com/photo-1564501049412-61c2a3083791'),
(14, 'Caldera Sunset Villas', 'Santorini villas with caldera views, terrace breakfasts, beach access, and sunset walks.', 5, 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b'),
(15, 'Medina Garden Riad', 'A Marrakech riad with courtyard rooms, breakfast, souk access, and desert excursion pickup.', 4, 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461'),
(16, 'Marina Bay Stay', 'A Singapore hotel near gardens, waterfront dining, cultural districts, and metro links.', 4, 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb'),
(17, 'Pacific Mountain Hotel', 'A Vancouver hotel close to harbor paths, food markets, forest trails, and mountain views.', 4, 'https://images.unsplash.com/photo-1564501049412-61c2a3083791');
GO

INSERT INTO dbo.Travel_Packages
    (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
VALUES
(1, 1, 'Paris Romantic Trip', 'Five nights in Paris with hotel, breakfast, and a guided city tour included.', 799.00, '2026-06-10', '2026-06-15', 18, 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34'),
(2, 2, 'Bali Culture Escape', 'Seven nights in Bali with resort stay, temple visits, breakfast, and airport transfer.', 1099.00, '2026-07-04', '2026-07-11', 22, 'https://images.unsplash.com/photo-1537953773345-d172ccf13cf1'),
(3, 3, 'Maldives Island Retreat', 'Six nights in a lagoon villa with breakfast, boat transfer, and snorkeling tour.', 1499.00, '2026-08-02', '2026-08-08', 12, 'https://images.unsplash.com/photo-1573843981267-be1999ff37cd'),
(4, 4, 'Neon Tokyo Discovery', 'Six nights in Tokyo with hotel, breakfast, metro access guidance, and a city highlights tour.', 1299.00, '2026-09-05', '2026-09-11', 20, 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf'),
(5, 5, 'Rome Ancient Wonders', 'Five nights in Rome with hotel, breakfast, guided landmark visits, and food district time.', 899.00, '2026-09-18', '2026-09-23', 18, 'https://images.unsplash.com/photo-1529260830199-42c24126f198'),
(6, 6, 'New York City Lights', 'Four nights in New York with hotel, breakfast, skyline viewpoint access, and museum time.', 1199.00, '2026-10-03', '2026-10-07', 16, 'https://images.unsplash.com/photo-1485871981521-5b1fd3805eee'),
(7, 7, 'Dubai Desert and Skyline', 'Five nights in Dubai with resort stay, breakfast, desert safari, and marina evening tour.', 1399.00, '2026-10-15', '2026-10-20', 20, 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c'),
(8, 8, 'Cape Town Coast Adventure', 'Six nights in Cape Town with lodge stay, breakfast, coastal drive, and mountain day tour.', 999.00, '2026-11-02', '2026-11-08', 14, 'https://images.unsplash.com/photo-1580060839134-75a5edca2e99'),
(9, 9, 'Kyoto Temples and Tea', 'Five nights in Kyoto with inn stay, breakfast, temple visits, garden walks, and tea tasting.', 1099.00, '2026-11-14', '2026-11-19', 15, 'https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e'),
(10, 10, 'Istanbul Two Continents', 'Five nights in Istanbul with hotel, breakfast, bazaar tour, ferry ride, and historic district walk.', 849.00, '2026-12-01', '2026-12-06', 22, 'https://images.unsplash.com/photo-1524231757912-21f4fe3a7200'),
(11, 11, 'Barcelona Art and Beach', 'Five nights in Barcelona with hotel, breakfast, Gaudi tour, tapas evening, and beach time.', 899.00, '2026-12-10', '2026-12-15', 19, 'https://images.unsplash.com/photo-1539037116277-4db20889f2d4'),
(12, 12, 'Sydney Harbour Escape', 'Six nights in Sydney with hotel, breakfast, harbor walk, beach transfer, and wildlife visit.', 1299.00, '2027-01-08', '2027-01-14', 17, 'https://images.unsplash.com/photo-1506973035872-a4ec16b8e8d9'),
(13, 13, 'Cairo Pyramids Journey', 'Five nights in Cairo with hotel, breakfast, pyramid visit, museum time, and Nile evening.', 799.00, '2027-01-22', '2027-01-27', 18, 'https://images.unsplash.com/photo-1572252009286-268acec5ca0a'),
(14, 14, 'Santorini Sunset Getaway', 'Five nights in Santorini with villa stay, breakfast, caldera walk, beach visit, and sunset dinner.', 1399.00, '2027-02-05', '2027-02-10', 12, 'https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff'),
(15, 15, 'Marrakech Medina Escape', 'Five nights in Marrakech with riad stay, breakfast, souk tour, garden visit, and desert day trip.', 749.00, '2027-02-18', '2027-02-23', 20, 'https://images.unsplash.com/photo-1597212618440-806262de4f6b'),
(16, 16, 'Singapore Garden City', 'Four nights in Singapore with hotel, breakfast, gardens visit, waterfront evening, and food tour.', 999.00, '2027-03-04', '2027-03-08', 18, 'https://images.unsplash.com/photo-1525625293386-3f8f99389edd'),
(17, 17, 'Vancouver Mountain and Harbor', 'Six nights in Vancouver with hotel, breakfast, harbor walk, forest trail, and mountain day trip.', 1099.00, '2027-03-18', '2027-03-24', 16, 'https://images.unsplash.com/photo-1559511260-66a654ae982a');
GO
