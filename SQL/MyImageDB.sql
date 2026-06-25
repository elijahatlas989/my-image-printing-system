/* Optional SQL Server script for the MyImage college project. */
CREATE DATABASE MyImageDB;
GO

USE MyImageDB;
GO

/* Customer personal details. */
CREATE TABLE Customers
(
    CustId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    DOB DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    PhoneNo NVARCHAR(20) NOT NULL,
    Address NVARCHAR(200) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE
);

/* Customer login details. */
CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CustId INT NOT NULL UNIQUE,
    CONSTRAINT FK_Users_Customers FOREIGN KEY (CustId) REFERENCES Customers(CustId) ON DELETE CASCADE
);

/* Reference table controlled by the administrator. */
CREATE TABLE PrintSizes
(
    PrintSizeId INT IDENTITY(1,1) PRIMARY KEY,
    SizeName NVARCHAR(20) NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);

/* Main purchase order table. */
CREATE TABLE Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    CustId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    FolderName NVARCHAR(100) NOT NULL,
    ShippingAddress NVARCHAR(250) NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustId) REFERENCES Customers(CustId)
);

/* One row for every uploaded photograph. */
CREATE TABLE OrderDetails
(
    DetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    OriginalFileName NVARCHAR(255) NOT NULL,
    StoredFileName NVARCHAR(255) NOT NULL,
    PrintSizeId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_PrintSizes FOREIGN KEY (PrintSizeId) REFERENCES PrintSizes(PrintSizeId)
);

/* Payment mode and protected demo card data. */
CREATE TABLE Payments
(
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL UNIQUE,
    PaymentMethod NVARCHAR(50) NOT NULL,
    EncryptedCardNumber NVARCHAR(MAX) NULL,
    CardLastFour NVARCHAR(4) NULL,
    PaymentDate DATETIME2 NOT NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE
);

/* Starting prices used by the application. */
INSERT INTO PrintSizes (SizeName, Price) VALUES
('4 x 6', 50.00),
('5 x 7', 80.00),
('8 x 10', 150.00),
('10 x 12', 250.00);
GO
