CREATE OR ALTER FUNCTION RecordLikeThis (@RecordDateTime DATETIME, @RecordWorkerId INT, @RecordType INT, @RecordSource INT)
RETURNS TABLE
RETURN SELECT * 
	FROM RCPdb.dbo.RCPlogs
	WHERE Timestamp = @RecordDateTime
	AND WorkerId = @RecordWorkerId
	AND ActionType = @RecordType
	AND LoggerType = @RecordSource
GO

CREATE OR ALTER PROCEDURE InsertDistinct (@RecordDateTime DATETIME, @RecordWorkerId INT, @RecordType INT, @RecordSource INT)
AS
IF NOT EXISTS(SELECT * FROM RecordLikeThis(@RecordDateTime, @RecordWorkerId, @RecordType, @RecordSource))
	INSERT INTO RCPdb.dbo.RCPlogs (Timestamp, WorkerId, ActionType, LoggerType)
	VALUES (@RecordDateTime, @RecordWorkerId, @RecordType, @RecordSource)

--EXEC InsertDistinct @t = '2018-06-25 07:48:00', @w = 0, @a = 0, @l = 0;