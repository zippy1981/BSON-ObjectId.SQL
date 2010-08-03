-- SQL To add BSON Object Id to SQL server.
USE master;
GO

IF  NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'mongoDB')
CREATE DATABASE [mongoDB] 
GO

ALTER AUTHORIZATION ON DATABASE::mongoDB TO "sa"
GO

ALTER DATABASE MongoDB SET TRUSTWORTHY ON
GO

sp_configure 'clr enabled', 1
go
reconfigure
go

USE [mongoDB]
GO

IF  EXISTS (SELECT * FROM sys.assembly_types at INNER JOIN sys.schemas ss on at.schema_id = ss.schema_id WHERE at.name = N'CLR_Oid' AND ss.name=N'dbo')
DROP TYPE [dbo].[CLR_Oid]
GO

IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'BSON' and is_user_defined = 1)
DROP ASSEMBLY [BSON]
GO

DECLARE @assemblyPath VARCHAR(MAX)
-- DEBUG
SET @assemblyPath = 'C:\src\BSON-ObjectId.SQL\dotNET\BSON-Sharp\BSON-Sharp\bin\Debug\BSON-Sharp.dll'
-- RELEASE
--SET @assemblyPath = 'C:\src\BSON-ObjectId.SQL\dotNET\BSON-Sharp\BSON-Sharp\bin\Release\BSON-Sharp.dll'

CREATE ASSEMBLY [BSON]
FROM @assemblyPath
WITH PERMISSION_SET = SAFE;
GO

CREATE TYPE dbo.CLR_Oid
EXTERNAL NAME BSON.[Oid];
GO