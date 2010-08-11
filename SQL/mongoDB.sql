-- We are callign our database mongoDB beccause BSON was written for mongo.
-- TODO: Rename database

USE master;
GO

IF  NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'mongoDB')
CREATE DATABASE [mongoDB] 
GO

USE [mongoDB]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufn_NewOid]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ufn_NewOid]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufn_GetOidDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ufn_GetOidDate]
GO



IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'BSONOID' AND ss.name = N'dbo')
DROP TYPE [dbo].[BSONOID]
GO

CREATE TYPE [dbo].[BSONOID] FROM [binary](12) NULL
GO

-- Creates a new BSONOID
CREATE FUNCTION dbo.ufn_NewOid(@machineId BINARY(3)) RETURNS  BSONOID AS
BEGIN
	DECLARE @ret BSONOID
	DECLARE @currDate AS DATETIME2
	DECLARE @currDateBinary AS CHAR(10)
	DECLARE @twoFiftySix AS BIGINT
	SET @twoFiftySix = 256
	-- To get the current date as a unix timestamp
	-- See: http://mysql.databases.aspfaq.com/how-do-i-convert-a-sql-server-datetime-value-to-a-unix-timestamp.html
	SET @currDate = SYSDATETIME()
	SET @currDateBinary = CONVERT(CHAR(10), CAST(DATEDIFF(s, '1970-01-01', @currDate) AS varbinary), 1)
	-- BTW, to do the opposite: SELECT DATEADD(s, 1067441023, '1970-01-01')
	SET @ret = 
		(@machineId * POWER(@twoFiftySix, 5)) +
		(@@SPID * POWER(@twoFiftySix, 3)) +
		CAST(DATEPART(MCS, SYSDATETIME()) AS binary(3)) -- TODO: More complete sequence.
	SET @ret = CONVERT(BINARY(12), @currDateBinary + SUBSTRING(CONVERT(CHAR(26), @ret, 1), 11, 16), 1)
	RETURN @ret
END
GO

-- Extracts the date from a BSONOID
CREATE FUNCTION dbo.ufn_GetOidDate(@oid BSONOID) RETURNS  DATETIME AS
BEGIN
	DECLARE @strOid CHAR(26)
	DECLARE @unixTimeStamp INT

	SELECT @strOid = CONVERT(CHAR(26), @oid, 1)
	SELECT @unixTimeStamp=CAST(CONVERT(BINARY(4), SUBSTRING(@strOid, 1, 10), 1) AS INT)
	RETURN DATEADD(s, @unixTimeStamp, '1970-01-01')
END
GO