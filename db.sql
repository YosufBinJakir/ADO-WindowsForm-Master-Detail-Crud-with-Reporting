CREATE TABLE vehicles
(
	vehicleid INT IDENTITY PRIMARY KEY,
	vehicletype NVARCHAR(30) NOT NULL,
	model NVARCHAR(40) NOT NULL,
	importormanufacturedate DATE NOT NULL,
	chesisno NVARCHAR(40) NOT NULL,
	regno NVARCHAR(40) NOT NULL,
	picture NVARCHAR(50) NOT NULL,
	islifeexpired BIT

)
GO
CREATE TABLE vehicleownertracks
(
	vehicleownertrack INT IDENTITY NOT NULL,
	ownername NVARCHAR(40) NOT NULL,
	owneraddress NVARCHAR(100) NOT NULL,
	fromdate DATE NOT NULL,
	todate DATE NULL,
	vehicleid INT NOT NULL REFERENCES vehicles(vehicleid)
)
GO