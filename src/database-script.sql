-- Postal Service Management System
-- Middlesex University — CST2550 Software Engineering
-- Management and Development


-- ──> CREATE DATABASE
CREATE DATABASE PostalServiceDB;
USE PostalServiceDB;



-- CREATE TABLES

-- ──> USER TABLE
CREATE TABLE User (
    UserID VARCHAR(10) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Address VARCHAR(200) NOT NULL,
    PostCode VARCHAR(10) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL
);

-- ──> CUSTOMER TABLE
CREATE TABLE Customer (
    CustomerID VARCHAR(10) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Address VARCHAR(200) NOT NULL,
    PostCode VARCHAR(10) NOT NULL
);

-- ──> PARCEL TABLE
CREATE TABLE Parcel (
    TrackingID VARCHAR(20) PRIMARY KEY,
    CustomerID VARCHAR(10) NOT NULL,
    SenderName VARCHAR(100) NOT NULL,
    SenderAddress VARCHAR(200) NOT NULL,
    ReceiverName VARCHAR(100) NOT NULL,
    ReceiverAddress VARCHAR(200) NOT NULL,
    Weight DECIMAL(10,2) NOT NULL,
    Size VARCHAR(10) NOT NULL,
    Type VARCHAR(20) NOT NULL,
    Status VARCHAR(30) NOT NULL,
    DateSent DATETIME NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    EstimatedDelivery DATETIME,
    FOREIGN KEY (CustomerID)
    REFERENCES Customer(CustomerID)
);

-- ──> DELIVERY TABLE
CREATE TABLE Delivery (
    DeliveryID VARCHAR(10) PRIMARY KEY,
    TrackingID VARCHAR(20) NOT NULL,
    DriverName VARCHAR(100) NOT NULL,
    Route VARCHAR(50) NOT NULL,
    AssignedDate DATETIME NOT NULL,
    DeliveryStatus VARCHAR(30) NOT NULL,
    AttemptCount INT DEFAULT 0,
    LastAttemptDate DATETIME,
    Notes VARCHAR(200),
    FOREIGN KEY (TrackingID)
    REFERENCES Parcel(TrackingID)
);


-- INSERT SAMPLE DATA


-- ──> INSERT USERS
INSERT INTO User VALUES
('USER-001', 'Admin User', 'admin@postal.com',
'07700000001', '1 Admin Street London', 'EC1 1AA',
'Admin', 'admin123', '2026-01-01 09:00:00'),

('USER-002', 'Staff User', 'staff@postal.com',
'07700000002', '2 Staff Road London', 'EC1 2BB',
'Staff', 'staff123', '2026-01-02 09:00:00'),

('USER-003', 'John Smith', 'john@email.com',
'07700900001', '123 Main St London', 'N1 2AB',
'Staff', 'john123', '2026-02-01 09:00:00'),

('USER-004', 'Alice Brown', 'alice@email.com',
'07700900002', '45 Park Road London', 'SW1 3CD',
'Staff', 'alice123', '2026-02-02 09:00:00'),

('USER-005', 'Tom Harris', 'tom@email.com',
'07700900003', '78 High Street London', 'E1 4EF',
'Staff', 'tom123', '2026-02-03 09:00:00');

-- ──> INSERT CUSTOMERS
INSERT INTO Customer VALUES
('CUST-001', 'John Smith', 'john@email.com',
'07700900001', '123 Main St London', 'N1 2AB'),
('CUST-002', 'Alice Brown', 'alice@email.com',
'07700900002', '45 Park Road London', 'SW1 3CD'),
('CUST-003', 'Tom Harris', 'tom@email.com',
'07700900003', '78 High Street London', 'E1 4EF'),
('CUST-004', 'Mary Jones', 'mary@email.com',
'07700900004', '12 Oak Avenue London', 'W1 5GH'),
('CUST-005', 'David Lee', 'david@email.com',
'07700900005', '56 Rose Lane London', 'SE1 6IJ'),
('CUST-006', 'Emma Wilson', 'emma@email.com',
'07700900006', '34 Maple Street London', 'EC1 7KL'),
('CUST-007', 'James Taylor', 'james@email.com',
'07700900007', '67 Cedar Road London', 'WC1 8MN'),
('CUST-008', 'Sophie Clark', 'sophie@email.com',
'07700900008', '89 Birch Lane London', 'EC2 9OP'),
('CUST-009', 'Oliver White', 'oliver@email.com',
'07700900009', '23 Pine Avenue London', 'WC2 1QR'),
('CUST-010', 'Isabella Moore', 'isabella@email.com',
'07700900010', '45 Elm Court London', 'N2 2ST');

-- ──> INSERT PARCELS
INSERT INTO Parcel VALUES
('PS-2026-00001', 'CUST-001', 'John Smith',
'123 Main St London', 'Jane Doe',
'45 Park Road Manchester',
2.5, 'Medium', 'Express', 'In Transit',
'2026-03-06 09:00:00', 12.99,
'2026-03-08 09:00:00'),

('PS-2026-00002', 'CUST-002', 'Alice Brown',
'45 Park Road London', 'Bob Wilson',
'78 High Street Birmingham',
1.2, 'Small', 'Standard', 'Delivered',
'2026-03-05 10:00:00', 5.99,
'2026-03-08 10:00:00'),

('PS-2026-00003', 'CUST-003', 'Tom Harris',
'78 High Street London', 'Sarah Lee',
'12 Oak Avenue Leeds',
3.8, 'Large', 'Next Day', 'Pending',
'2026-03-07 11:00:00', 19.99,
'2026-03-08 11:00:00'),

('PS-2026-00004', 'CUST-004', 'Mary Jones',
'12 Oak Avenue London', 'Chris Evans',
'56 Rose Lane Bristol',
0.8, 'Small', 'Standard', 'Failed',
'2026-03-04 12:00:00', 4.99,
'2026-03-07 12:00:00'),

('PS-2026-00005', 'CUST-005', 'David Lee',
'56 Rose Lane London', 'Emma White',
'90 Elm Street Liverpool',
5.0, 'Large', 'Express', 'Out for Delivery',
'2026-03-08 08:00:00', 24.99,
'2026-03-10 08:00:00'),

('PS-2026-00006', 'CUST-006', 'Emma Wilson',
'34 Maple Street London', 'Liam Johnson',
'12 Birch Road Sheffield',
1.5, 'Small', 'Standard', 'Pending',
'2026-03-09 09:00:00', 6.99,
'2026-03-12 09:00:00'),

('PS-2026-00007', 'CUST-007', 'James Taylor',
'67 Cedar Road London', 'Olivia Brown',
'45 Oak Street Newcastle',
4.2, 'Large', 'Express', 'In Transit',
'2026-03-09 10:00:00', 22.99,
'2026-03-11 10:00:00'),

('PS-2026-00008', 'CUST-008', 'Sophie Clark',
'89 Birch Lane London', 'Noah Davis',
'78 Pine Road Cardiff',
0.5, 'Small', 'Next Day', 'Delivered',
'2026-03-08 11:00:00', 9.99,
'2026-03-09 11:00:00'),

('PS-2026-00009', 'CUST-009', 'Oliver White',
'23 Pine Avenue London', 'Ava Martinez',
'34 Cedar Lane Edinburgh',
2.8, 'Medium', 'Standard', 'In Transit',
'2026-03-10 08:00:00', 8.99,
'2026-03-14 08:00:00'),

('PS-2026-00010', 'CUST-010', 'Isabella Moore',
'45 Elm Court London', 'William Taylor',
'67 Maple Street Glasgow',
3.1, 'Medium', 'Express', 'Pending',
'2026-03-10 09:00:00', 15.99,
'2026-03-12 09:00:00');

-- ──> INSERT DELIVERIES
INSERT INTO Delivery VALUES
('DEL-001', 'PS-2026-00001', 'Mike Jones',
'Zone A', '2026-03-07 08:00:00',
'In Progress', 1, '2026-03-07 14:00:00',
'Customer not home first attempt'),

('DEL-002', 'PS-2026-00002', 'Sarah Lee',
'Zone B', '2026-03-06 08:00:00',
'Completed', 1, '2026-03-06 15:00:00',
'Delivered successfully'),

('DEL-003', 'PS-2026-00003', 'Tom Brown',
'Zone A', '2026-03-08 08:00:00',
'Assigned', 0, NULL,
NULL),

('DEL-004', 'PS-2026-00004', 'Mike Jones',
'Zone C', '2026-03-05 08:00:00',
'Failed', 3, '2026-03-07 16:00:00',
'3 failed attempts - returning to depot'),

('DEL-005', 'PS-2026-00005', 'Emma Davis',
'Zone B', '2026-03-09 08:00:00',
'In Progress', 1, '2026-03-09 11:00:00',
'Out for delivery'),

('DEL-006', 'PS-2026-00006', 'Tom Brown',
'Zone A', '2026-03-10 08:00:00',
'Assigned', 0, NULL,
NULL),

('DEL-007', 'PS-2026-00007', 'Mike Jones',
'Zone B', '2026-03-10 09:00:00',
'In Progress', 1, '2026-03-10 13:00:00',
'Left with neighbour'),

('DEL-008', 'PS-2026-00008', 'Sarah Lee',
'Zone C', '2026-03-09 08:00:00',
'Completed', 1, '2026-03-09 12:00:00',
'Delivered to front door'),

('DEL-009', 'PS-2026-00009', 'Emma Davis',
'Zone A', '2026-03-11 08:00:00',
'Assigned', 0, NULL,
NULL),

('DEL-010', 'PS-2026-00010', 'Tom Brown',
'Zone B', '2026-03-11 09:00:00',
'Assigned', 0, NULL,
NULL);



-- CREATE VIEWS

-- ──> PARCEL JOURNEY VIEW
CREATE VIEW ParcelJourney AS
SELECT
    c.FullName AS CustomerName,
    c.Email,
    p.TrackingID,
    p.SenderName,
    p.ReceiverName,
    p.Status AS ParcelStatus,
    p.Type,
    p.Price,
    p.EstimatedDelivery,
    d.DriverName,
    d.Route,
    d.DeliveryStatus,
    d.AttemptCount,
    d.Notes
FROM Customer c
JOIN Parcel p ON c.CustomerID = p.CustomerID
JOIN Delivery d ON p.TrackingID = d.TrackingID;

-- ──> USER LOGIN VIEW
CREATE VIEW UserLogin AS
SELECT
    UserID,
    FullName,
    Email,
    Role,
    CreatedDate
FROM User;

-- ──> DASHBOARD STATS VIEW
CREATE VIEW DashboardStats AS
SELECT
    COUNT(*) AS TotalParcels,
    SUM(CASE WHEN Status = 'In Transit' 
        THEN 1 ELSE 0 END) AS InTransit,
    SUM(CASE WHEN Status = 'Delivered' 
        THEN 1 ELSE 0 END) AS Delivered,
    SUM(CASE WHEN Status = 'Pending' 
        THEN 1 ELSE 0 END) AS Pending,
    SUM(CASE WHEN Status = 'Failed' 
        THEN 1 ELSE 0 END) AS Failed,
    SUM(CASE WHEN Status = 'Out for Delivery' 
        THEN 1 ELSE 0 END) AS OutForDelivery
FROM Parcel;


-- CREATE STORED PROCEDURES


-- ──> SEARCH PARCEL PROCEDURE
-- Matches Algorithm 2 — Search Parcel in Hash Table
DELIMITER //
CREATE PROCEDURE SearchParcel(IN trackID VARCHAR(20))
BEGIN
    SELECT p.*, d.DriverName, d.DeliveryStatus,
    d.AttemptCount, d.Notes
    FROM Parcel p
    LEFT JOIN Delivery d
    ON p.TrackingID = d.TrackingID
    WHERE p.TrackingID = trackID;
END //
DELIMITER ;

-- ──> LOGIN USER PROCEDURE
-- Matches Algorithm 11 — Login User
DELIMITER //
CREATE PROCEDURE LoginUser(
    IN userEmail VARCHAR(100),
    IN userPassword VARCHAR(255)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM User
        WHERE Email = userEmail
        AND Password = userPassword
    ) THEN
        SELECT UserID, FullName, Email, Role
        FROM User
        WHERE Email = userEmail
        AND Password = userPassword;
    ELSE
        SELECT 'Invalid email or password' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> REGISTER USER PROCEDURE
-- Matches Algorithm 10 — Register New User
DELIMITER //
CREATE PROCEDURE RegisterUser(
    IN newUserID VARCHAR(10),
    IN newFullName VARCHAR(100),
    IN newEmail VARCHAR(100),
    IN newPhone VARCHAR(20),
    IN newAddress VARCHAR(200),
    IN newPostCode VARCHAR(10),
    IN newRole VARCHAR(20),
    IN newPassword VARCHAR(255)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM User WHERE Email = newEmail
    ) THEN
        INSERT INTO User VALUES (
            newUserID,
            newFullName,
            newEmail,
            newPhone,
            newAddress,
            newPostCode,
            newRole,
            newPassword,
            NOW()
        );
        SELECT 'Account created successfully' AS Message;
    ELSE
        SELECT 'Email already registered' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> ADD CUSTOMER PROCEDURE
-- Matches Algorithm 12 — Add Customer
DELIMITER //
CREATE PROCEDURE AddCustomer(
    IN newCustID VARCHAR(10),
    IN newFullName VARCHAR(100),
    IN newEmail VARCHAR(100),
    IN newPhone VARCHAR(20),
    IN newAddress VARCHAR(200),
    IN newPostCode VARCHAR(10)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Customer WHERE Email = newEmail
    ) THEN
        INSERT INTO Customer VALUES (
            newCustID,
            newFullName,
            newEmail,
            newPhone,
            newAddress,
            newPostCode
        );
        SELECT 'Customer added successfully' AS Message;
    ELSE
        SELECT 'Customer already exists' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> SEARCH CUSTOMER PROCEDURE
-- Matches Algorithm 16 — Search Customer
DELIMITER //
CREATE PROCEDURE SearchCustomer(IN searchTerm VARCHAR(100))
BEGIN
    SELECT * FROM Customer
    WHERE FullName LIKE CONCAT('%', searchTerm, '%')
    OR Email LIKE CONCAT('%', searchTerm, '%')
    OR PostCode LIKE CONCAT('%', searchTerm, '%');
END //
DELIMITER ;

-- ──> DELETE CUSTOMER PROCEDURE
-- Matches Algorithm 17 — Delete Customer
DELIMITER //
CREATE PROCEDURE DeleteCustomer(IN custID VARCHAR(10))
BEGIN
    IF EXISTS (
        SELECT 1 FROM Customer WHERE CustomerID = custID
    ) THEN
        DELETE FROM Delivery WHERE TrackingID IN (
            SELECT TrackingID FROM Parcel
            WHERE CustomerID = custID
        );
        DELETE FROM Parcel
        WHERE CustomerID = custID;
        DELETE FROM Customer
        WHERE CustomerID = custID;
        SELECT 'Customer deleted successfully' AS Message;
    ELSE
        SELECT 'Customer not found' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> UPDATE PARCEL STATUS PROCEDURE
-- Matches Algorithm 18 — Update Parcel Status
DELIMITER //
CREATE PROCEDURE UpdateParcelStatus(
    IN trackID VARCHAR(20),
    IN newStatus VARCHAR(30)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM Parcel WHERE TrackingID = trackID
    ) THEN
        UPDATE Parcel
        SET Status = newStatus
        WHERE TrackingID = trackID;
        IF newStatus = 'Delivered' THEN
            UPDATE Delivery
            SET DeliveryStatus = 'Completed'
            WHERE TrackingID = trackID;
        END IF;
        SELECT 'Status updated successfully' AS Message;
    ELSE
        SELECT 'Parcel not found' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> ASSIGN DRIVER PROCEDURE
-- Matches Algorithm 13 — Assign Driver
DELIMITER //
CREATE PROCEDURE AssignDriver(
    IN delID VARCHAR(10),
    IN driverName VARCHAR(100),
    IN route VARCHAR(50)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM Delivery WHERE DeliveryID = delID
    ) THEN
        UPDATE Delivery
        SET DriverName = driverName,
            Route = route,
            AssignedDate = NOW(),
            DeliveryStatus = 'Assigned'
        WHERE DeliveryID = delID;
        SELECT 'Driver assigned successfully' AS Message;
    ELSE
        SELECT 'Delivery not found' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> UPDATE DELIVERY STATUS PROCEDURE
-- Matches Algorithm 14 — Update Delivery Status
DELIMITER //
CREATE PROCEDURE UpdateDeliveryStatus(
    IN delID VARCHAR(10),
    IN newStatus VARCHAR(30)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM Delivery WHERE DeliveryID = delID
    ) THEN
        IF newStatus = 'Failed' THEN
            UPDATE Delivery
            SET DeliveryStatus = newStatus,
                AttemptCount = AttemptCount + 1,
                LastAttemptDate = NOW()
            WHERE DeliveryID = delID;
            IF (SELECT AttemptCount FROM Delivery
                WHERE DeliveryID = delID) >= 3 THEN
                UPDATE Delivery
                SET DeliveryStatus = 'Return to Depot'
                WHERE DeliveryID = delID;
                SELECT 'Max attempts reached' AS Message;
            ELSE
                SELECT 'Status updated successfully' 
                AS Message;
            END IF;
        ELSE
            UPDATE Delivery
            SET DeliveryStatus = newStatus
            WHERE DeliveryID = delID;
            SELECT 'Status updated successfully' AS Message;
        END IF;
    ELSE
        SELECT 'Delivery not found' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> DELETE PARCEL PROCEDURE
-- Matches Algorithm 3 — Delete Parcel
DELIMITER //
CREATE PROCEDURE DeleteParcel(IN trackID VARCHAR(20))
BEGIN
    IF EXISTS (
        SELECT 1 FROM Parcel WHERE TrackingID = trackID
    ) THEN
        DELETE FROM Delivery
        WHERE TrackingID = trackID;
        DELETE FROM Parcel
        WHERE TrackingID = trackID;
        SELECT 'Parcel deleted successfully' AS Message;
    ELSE
        SELECT 'Parcel not found' AS Message;
    END IF;
END //
DELIMITER ;

-- ──> AUTO ASSIGN DELIVERY PROCEDURE
-- Matches Algorithm 19 — Auto Assign Delivery
DELIMITER //
CREATE PROCEDURE AutoAssignDelivery(
    IN trackID VARCHAR(20),
    IN newDelID VARCHAR(10)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM Parcel WHERE TrackingID = trackID
    ) THEN
        INSERT INTO Delivery VALUES (
            newDelID,
            trackID,
            'Unassigned',
            'Unassigned',
            NOW(),
            'Assigned',
            0,
            NULL,
            NULL
        );
        SELECT 'Delivery created successfully' AS Message;
    ELSE
        SELECT 'Parcel not found' AS Message;
    END IF;
END //
DELIMITER ;


-- TEST QUERIES

-- ──> USER TESTS
-- SELECT * FROM User;
-- SELECT * FROM User WHERE Role = 'Admin';
-- SELECT * FROM User WHERE Email = 'admin@postal.com';
-- CALL LoginUser('admin@postal.com', 'admin123');
-- CALL LoginUser('wrong@email.com', 'wrongpass');
-- CALL RegisterUser('USER-006', 'Test User',
-- 'test@email.com', '07700000006',
-- '1 Test Road London', 'EC1 3CC', 'Staff', 'test123');

-- ──> BASIC TESTS
-- SELECT * FROM Customer;
-- SELECT * FROM Parcel;
-- SELECT * FROM Delivery;

-- ──> SEARCH TESTS
-- CALL SearchParcel('PS-2026-00001');
-- CALL SearchCustomer('John');
-- CALL SearchCustomer('N1 2AB');
-- SELECT * FROM Customer WHERE FullName = 'John Smith';
-- SELECT * FROM Customer WHERE PostCode = 'N1 2AB';

-- ──> STATUS TESTS
-- SELECT * FROM Parcel WHERE Status = 'In Transit';
-- SELECT * FROM Parcel WHERE Status = 'Delivered';
-- SELECT * FROM Parcel WHERE Status = 'Pending';
-- SELECT * FROM Delivery WHERE DeliveryStatus = 'Failed';
-- SELECT * FROM Delivery WHERE DeliveryStatus = 'Completed';

-- ──> UPDATE TESTS
-- CALL UpdateParcelStatus('PS-2026-00003', 'In Transit');
-- CALL UpdateParcelStatus('PS-2026-00002', 'Delivered');
-- CALL AssignDriver('DEL-003', 'New Driver', 'Zone B');
-- CALL UpdateDeliveryStatus('DEL-001', 'Completed');
-- CALL UpdateDeliveryStatus('DEL-004', 'Failed');

-- ──> ADD TESTS
-- CALL AddCustomer('CUST-011', 'New Customer',
-- 'new@email.com', '07700000011',
-- '1 New Road London', 'EC1 4DD');
-- CALL AutoAssignDelivery('PS-2026-00001', 'DEL-011');

-- ──> DELETE TESTS
-- CALL DeleteParcel('PS-2026-00001');
-- CALL DeleteCustomer('CUST-001');

-- ──> JOIN TESTS
-- SELECT c.FullName, c.Email, p.TrackingID,
-- p.Status, p.Price
-- FROM Customer c JOIN Parcel p
-- ON c.CustomerID = p.CustomerID;

-- SELECT p.TrackingID, p.Status, d.DriverName,
-- d.Route, d.DeliveryStatus
-- FROM Parcel p JOIN Delivery d
-- ON p.TrackingID = d.TrackingID;

-- SELECT c.FullName, p.TrackingID, p.Status,
-- d.DriverName, d.DeliveryStatus
-- FROM Customer c
-- JOIN Parcel p ON c.CustomerID = p.CustomerID
-- JOIN Delivery d ON p.TrackingID = d.TrackingID;

-- ──> FILTER TESTS
-- SELECT * FROM Parcel WHERE Type = 'Express';
-- SELECT * FROM Parcel WHERE Weight > 2.0;
-- SELECT * FROM Parcel WHERE Price < 10.00;
-- SELECT * FROM Delivery WHERE AttemptCount > 1;
-- SELECT * FROM Delivery WHERE DriverName = 'Mike Jones';
-- SELECT * FROM Delivery WHERE Route = 'Zone A';

-- ──> COUNT TESTS
-- SELECT COUNT(*) AS TotalParcels FROM Parcel;
-- SELECT Status, COUNT(*) AS Count
-- FROM Parcel GROUP BY Status;
-- SELECT DeliveryStatus, COUNT(*) AS Count
-- FROM Delivery GROUP BY DeliveryStatus;
-- SELECT c.FullName, COUNT(p.TrackingID) AS TotalParcels
-- FROM Customer c JOIN Parcel p
-- ON c.CustomerID = p.CustomerID GROUP BY c.FullName;

-- ──> SORT TESTS
-- SELECT * FROM Parcel ORDER BY DateSent DESC;
-- SELECT * FROM Parcel ORDER BY Price ASC;
-- SELECT * FROM Parcel ORDER BY Weight DESC;

-- ──> VIEW TESTS
-- SELECT * FROM ParcelJourney;
-- SELECT * FROM UserLogin;
-- SELECT * FROM DashboardStats;

-- ──> STORED PROCEDURE TESTS
-- CALL SearchParcel('PS-2026-00001');
-- CALL LoginUser('admin@postal.com', 'admin123');
-- CALL RegisterUser('USER-006', 'Test User',
-- 'test@email.com', '07700000006',
-- '1 Test Road London', 'EC1 3CC', 'Staff', 'test123');
-- CALL AddCustomer('CUST-011', 'New Customer',
-- 'new@email.com', '07700000011',
-- '1 New Road London', 'EC1 4DD');
-- CALL SearchCustomer('John');
-- CALL DeleteCustomer('CUST-010');
-- CALL UpdateParcelStatus('PS-2026-00003', 'In Transit');
-- CALL AssignDriver('DEL-003', 'New Driver', 'Zone B');
-- CALL UpdateDeliveryStatus('DEL-001', 'Completed');
-- CALL DeleteParcel('PS-2026-00010');
-- CALL AutoAssignDelivery('PS-2026-00001', 'DEL-011');