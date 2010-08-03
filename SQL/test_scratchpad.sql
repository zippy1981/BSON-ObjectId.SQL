-- Scratch file that shows things in action
-- TODO: Organize more formal tests
USE [mongoDB]
GO

DECLARE @oid BSONOID
DECLARE @strOid CHAR(26)
DECLARE @unixTimeStamp INT

SELECT @oid=dbo.ufn_NewOid(0x710790)
SELECT @strOid = CONVERT(CHAR(26), @oid, 1)
SELECT @unixTimeStamp=CAST(CONVERT(BINARY(4), SUBSTRING(@strOid, 1, 10), 1) AS INT)

SELECT 
	@oid, --OID
	SUBSTRING(@strOid, 0, 10),
	@unixTimeStamp AS OidUnixTimeStamp,
	dbo.ufn_GetOidDate(@oid) AS OidDate, --OID-DatePart-AsDate
	GETDATE() AS RealDate