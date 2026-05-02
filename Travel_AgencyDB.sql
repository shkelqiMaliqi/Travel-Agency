CREATE DATABASE Travel_Agency;
GO

USE Travel_Agency;
GO

IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
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
    CONSTRAINT FK_ContactForm_User FOREIGN KEY (U_Id) REFERENCES dbo.Users(U_Id)
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
('Maldives', 'Enjoy luxury island escapes with crystal-clear water and white sand.', 'https://images.unsplash.com/photo-1514282401047-d79a71a590e8');
GO

INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
VALUES
(1, 'Seine View Hotel', 'A central Paris hotel near museums, cafes, and evening river walks.', 4, 'https://images.unsplash.com/photo-1566073771259-6a8506099945'),
(2, 'Ubud Garden Resort', 'A peaceful Bali resort with garden villas, breakfast, and pool access.', 5, 'https://images.unsplash.com/photo-1571896349842-33c89424de2d'),
(3, 'Crystal Lagoon Villas', 'Private island villas with beach access and turquoise lagoon views.', 5, 'https://images.unsplash.com/photo-1582719508461-905c673771fd');
GO

INSERT INTO dbo.Travel_Packages
    (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
VALUES
(1, 1, 'Paris Romantic Trip', 'Five nights in Paris with hotel, breakfast, and a guided city tour included.', 799.00, '2026-06-10', '2026-06-15', 18, 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34'),
(2, 2, 'Bali Culture Escape', 'Seven nights in Bali with resort stay, temple visits, breakfast, and airport transfer.', 1099.00, '2026-07-04', '2026-07-11', 22, 'https://images.unsplash.com/photo-1537953773345-d172ccf13cf1'),
(3, 3, 'Maldives Island Retreat', 'Six nights in a lagoon villa with breakfast, boat transfer, and snorkeling tour.', 1499.00, '2026-08-02', '2026-08-08', 12, 'https://images.unsplash.com/photo-1573843981267-be1999ff37cd');
GO
