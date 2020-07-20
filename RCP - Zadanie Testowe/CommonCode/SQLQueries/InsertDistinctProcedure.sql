CREATE PROCEDURE InsertDistinct @t DATETIME, @w INT, @a INT, @l INT
AS
INSERT INTO RCPdb.dbo.RCPlogs (Timestamp, WorkerId, ActionType, LoggerType)
SELECT @t, @w, @a, @l WHERE NOT EXISTS 
(
	SELECT null FROM RCPdb.dbo.RCPlogs r WHERE r.Timestamp = @t
	AND r.WorkerId = @w
	AND r.ActionType = @a
	AND r.LoggerType = @l
)
--EXEC InsertDistinct @t = '2018-06-25 07:48:00', @w = 0, @a = 0, @l = 0;