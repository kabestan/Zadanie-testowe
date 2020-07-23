CREATE OR ALTER VIEW dateTimeSplit AS
	SELECT 
		Timestamp AS RecordDateTime,
		YEAR(Timestamp) AS RecordYear,
		MONTH(Timestamp) AS RecordMonth,
		WorkerId AS RecordWorkerId,
		LoggerType AS RecordSource,
		ActionType AS RecordType
	FROM 
		RCPlogs
	WHERE 
		ActionType = 0 -- 'entry'
		OR ActionType = 1 -- 'exit'
		OR ActionType = 5 -- 'exitAux'
GO

CREATE OR ALTER FUNCTION RecordsOfWorker (@RecordWorkerId INT)
RETURNS TABLE
RETURN SELECT 
		*,
		LAG(RecordType) OVER(ORDER BY RecordDateTime ASC) AS RecordPrevType,
		DATEDIFF(MINUTE, LAG(RecordDateTime) OVER(ORDER BY RecordDateTime ASC), RecordDateTime) AS RecordSplitMinutes
	FROM dateTimeSplit
	WHERE RecordWorkerId = @RecordWorkerId
GO

CREATE OR ALTER FUNCTION RecordsOfValidExitsOfWorker (@RecordWorkerId INT)
RETURNS TABLE
	RETURN SELECT *
	FROM RecordsOfWorker(@RecordWorkerId)
	WHERE (RecordType = 1 AND RecordPrevType = 0) -- exit after entry
	OR (RecordType = 5 AND RecordPrevType = 0) -- exitAux after entry
GO

CREATE OR ALTER FUNCTION HoursOfWorkInMonth (@RecordYear INT, @RecordMonth INT, @RecordWorkerId INT)
RETURNS TABLE 
	RETURN SELECT SUM(RecordSplitMinutes) / 60 AS TotalHours
	FROM RecordsOfValidExitsOfWorker(@RecordWorkerId)
	WHERE RecordYear = @RecordYear
	AND RecordMonth = @RecordMonth
GO

CREATE OR ALTER PROCEDURE GetReport
AS 
	SELECT
		CAST(RecordYear AS VARCHAR(4)) + '-' + CAST(RecordMonth AS VARCHAR(2)) AS RecordYearMonth,
		RecordWorkerId,
		ISNULL((SELECT * FROM HoursOfWorkInMonth(RecordYear, RecordMonth, RecordWorkerId)), 0) AS TotalHours
	FROM dateTimeSplit topLayer
	GROUP BY
		RecordYear,
		RecordMonth,
		RecordWorkerId
	ORDER BY
		RecordYear,
		RecordMonth,
		RecordWorkerId;
