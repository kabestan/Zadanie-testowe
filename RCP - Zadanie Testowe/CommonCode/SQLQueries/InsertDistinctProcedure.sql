CREATE OR ALTER FUNCTION NextRecordId()
RETURNS INT
BEGIN 
	DECLARE @nextId INT = 0;
	SELECT @nextId = ISNULL(MAX(RecordId), 0) + 1 FROM RCPdb.dbo.RCPlogs;
	RETURN @nextId;
END
GO

CREATE OR ALTER PROCEDURE InsertDistinct (@RecordDateTime DATETIME, @RecordWorkerId INT, @RecordType INT, @RecordSource INT)
AS
INSERT INTO RCPdb.dbo.RCPlogs (RecordId, Timestamp, WorkerId, ActionType, LoggerType)
	VALUES (RCPdb.dbo.NextRecordId(), @RecordDateTime, @RecordWorkerId, @RecordType, @RecordSource);
