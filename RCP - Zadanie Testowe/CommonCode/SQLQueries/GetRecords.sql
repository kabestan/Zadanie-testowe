--DECLARE @start int, @count int; SET @start = -1; SET @count = 100;
IF @start < 0
	SET @start = (SELECT MAX([RecordId]) - @count + 1 FROM[RCPlogs]);
SELECT * FROM [RCPlogs] WHERE [RecordId] >= @start AND [RecordId] < @start + @count;