DROP DATABASE IF EXISTS [RCPdb];
CREATE DATABASE [RCPdb];
GO
USE [RCPdb];

CREATE TABLE [RCPlogs]
(
	[RecordId]		INT				NOT NULL PRIMARY KEY,
	[Timestamp]		SMALLDATETIME	NOT NULL,
    [WorkerId]		INT				NOT NULL, 
    [ActionType]	TINYINT			NOT NULL, 
    [LoggerType]	TINYINT			NOT NULL
)

DROP INDEX IF EXISTS RCPlogs.uniqueRecords;
CREATE UNIQUE NONCLUSTERED INDEX uniqueRecords
ON RCPlogs (
	Timestamp ASC,
	WorkerId ASC,
	ActionType ASC,
	LoggerType ASC)
WITH(
	IGNORE_DUP_KEY = ON);