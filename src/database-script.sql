-- Postal Service Management System
-- Middlesex University — CST2550 Software Engineering Management and Development



-- Database Script



-- ──> CREATE DATABASE
CREATE DATABASE PostalServiceDB;
USE PostalServiceDB;

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

-- ──> CREATE VIEW 
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

-- ──> CREATE STORED PROCEDURE 
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


-- TEST QUERIES


-- ──> BASIC TESTS 
-- SELECT * FROM Customer;
-- SELECT * FROM Parcel;
-- SELECT * FROM Delivery;

-- ──> SEARCH TESTS
-- SELECT * FROM Parcel WHERE TrackingID = 'PS-2026-00001';
-- SELECT * FROM Customer WHERE FullName = 'John Smith';
-- SELECT * FROM Customer WHERE PostCode = 'N1 2AB';

-- ──> STATUS TESTS 
-- SELECT * FROM Parcel WHERE Status = 'In Transit';
-- SELECT * FROM Parcel WHERE Status = 'Delivered';
-- SELECT * FROM Parcel WHERE Status = 'Pending';
-- SELECT * FROM Delivery WHERE DeliveryStatus = 'Failed';
-- SELECT * FROM Delivery WHERE DeliveryStatus = 'Completed';

-- ──> JOIN TESTS 
-- SELECT c.FullName, c.Email, p.TrackingID, p.Status, p.Price
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
-- SELECT Status, COUNT(*) AS Count FROM Parcel GROUP BY Status;
-- SELECT DeliveryStatus, COUNT(*) AS Count 
-- FROM Delivery GROUP BY DeliveryStatus;
-- SELECT c.FullName, COUNT(p.TrackingID) AS TotalParcels
-- FROM Customer c JOIN Parcel p 
-- ON c.CustomerID = p.CustomerID GROUP BY c.FullName;

-- ──> SORT TESTS 
-- SELECT * FROM Parcel ORDER BY DateSent DESC;
-- SELECT * FROM Parcel ORDER BY Price ASC;
-- SELECT * FROM Parcel ORDER BY Weight DESC;

-- ──> VIEW TEST
-- SELECT * FROM ParcelJourney;

-- ──> STORED PROCEDURE TEST
-- CALL SearchParcel('PS-2026-00001');