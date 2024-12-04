Create Database Travel_Agency;

Use Travel_Agency;

CREATE TABLE Users (
    U_Id INT PRIMARY KEY IDENTITY(1,1),
    U_Name VARCHAR(255) NOT NULL,
    U_Surname VARCHAR(255) NOT NULL,
    U_Email VARCHAR(255) NOT NULL,
    U_Username VARCHAR(255) NOT NULL,
    U_Phone VARCHAR(20) NOT NULL,  
    U_Password VARCHAR(255) NOT NULL,
	U_RepeatPassword Varchar(255) NOT NULL,
    U_Type VARCHAR (10) NOT NULL,  
    
);

CREATE TABLE Contact_Form (
    C_Id INT PRIMARY KEY IDENTITY(1,1),
    C_Name VARCHAR(255),
    C_Surname VARCHAR(255),
    C_Email VARCHAR(MAX),
    C_Subject VARCHAR(255),
    C_Message VARCHAR(MAX),
    U_Id INT,
    CONSTRAINT FK_Contact_Form_Users FOREIGN KEY (U_Id) REFERENCES Users(U_Id)
);


