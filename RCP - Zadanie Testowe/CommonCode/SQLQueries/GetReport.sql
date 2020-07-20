BEGIN -- declare available actions table
	DECLARE @actions TABLE (
		ActionName VARCHAR(10) NOT NULL,
		ActionValue SMALLINT NOT NULL);
	INSERT INTO @actions VALUES ('Enter', 0), ('Exit', 1), ('AuxExit', 5);
END;

BEGIN -- declare accumulator table
	DECLARE @acc TABLE (
		OnlyYear INT NOT NULL,
		OnlyMonth INT NOT NULL,
		WorkerId INT NOT NULL,
		AccountedTimeMinutes INT NOT NULL,
		EntryTime SMALLDATETIME);
END;

BEGIN -- populate accumulator table
	WITH casted AS
	(
		SELECT
			YEAR(Timestamp) AS OnlyYear,
			Month(Timestamp) AS OnlyMonth,
			WorkerId
		FROM RCPdb.dbo.RCPlogs
	)
	INSERT INTO @acc
	SELECT 
		*,
		0,
		null
	FROM casted a
	GROUP BY OnlyYear, OnlyMonth, WorkerId;
END;

BEGIN -- process all records and accumulate worked hours
	DECLARE @CursorId CURSOR
	DECLARE @row BIGINT, @stamp SMALLDATETIME, @worker INT, @action INT;
	DECLARE @entryAction INT;
	SELECT @entryAction = ActionValue FROM @actions WHERE ActionName = 'Enter';

	SET @CursorId = CURSOR FOR
	SELECT 
		ROW_NUMBER() OVER(ORDER BY Timestamp) AS RowNumber,
		Timestamp,
		WorkerId,
		ActionType
	FROM RCPdb.dbo.RCPlogs r
	WHERE EXISTS (SELECT null FROM @actions a WHERE a.ActionValue = r.ActionType);

	OPEN @CursorId;
	FETCH NEXT FROM @CursorId INTO @row, @stamp, @worker, @action;
	WHILE @@FETCH_STATUS = 0
	BEGIN -- start iterating over all records
		IF @action = @entryAction
		BEGIN -- register entry time
			UPDATE @acc
			SET EntryTime = @stamp
			WHERE WorkerId = @worker
			AND OnlyYear = YEAR(@stamp)
			AND OnlyMonth = MONTH(@stamp);
		END;
		ELSE
		BEGIN -- calculate and accumulate worked ours
			DECLARE @entryTime SMALLDATETIME;
			SELECT @entryTime = EntryTime FROM @acc
			WHERE WorkerId = @worker
			AND OnlyYear = YEAR(@stamp)
			AND OnlyMonth = MONTH(@stamp);

			IF @entryTime IS NOT NULL
			BEGIN --...but only once for each entry
				UPDATE @acc
				SET EntryTime = null,
					AccountedTimeMinutes += DATEDIFF(minute, @entryTime, @stamp)
				WHERE WorkerId = @worker
				AND OnlyYear = YEAR(@stamp)
				AND OnlyMonth = MONTH(@stamp);
			END;
		END;
		
		FETCH NEXT FROM @CursorId INTO @row, @stamp, @worker, @action;
	END;
END;

SELECT 
	(CAST(OnlyYear AS VARCHAR(4)) + '-' + CAST(OnlyMonth AS VARCHAR(2))) AS YearMonth,
	WorkerId,
	AccountedTimeMinutes / 60 AS TotalHours 
FROM @acc 
ORDER BY OnlyYear, OnlyMonth, WorkerId;
