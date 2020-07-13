CREATE TABLE [dbo].[RCPlogs]
(
	[RecordId] INT NOT NULL PRIMARY KEY,
	[Timestamp] SMALLDATETIME NOT NULL, 
    [WorkerId] INT NOT NULL, 
    [ActionType] TINYINT NOT NULL, 
    [LoggerType] TINYINT NOT NULL
)