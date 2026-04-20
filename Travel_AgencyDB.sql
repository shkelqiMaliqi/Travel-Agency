CREATE DATABASE Travel_Agency;
GO

USE Travel_Agency;
GO

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

INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
VALUES
('Paris', 'Discover iconic landmarks, art, food, and romantic city walks.', 'https://images.unsplash.com/photo-1499856871958-5b9627545d1a'),
('Bali', 'Relax on tropical beaches and explore temples, rice terraces, and culture.', 'https://images.unsplash.com/photo-1537996194471-e657df975ab4'),
('Maldives', 'Enjoy luxury island escapes with crystal-clear water and white sand.', 'https://images.unsplash.com/photo-1514282401047-d79a71a590e8');
GO
