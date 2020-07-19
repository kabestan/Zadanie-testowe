SELECT a.Month FROM
	(SELECT (CAST(YEAR(Timestamp) AS VARCHAR(4)) + '-' + CAST(MONTH(Timestamp) AS VARCHAR(2))) AS Month FROM RCPdb.dbo.RCPlogs) a
GROUP BY Month;
--SELECT * FROM RCPdb.dbo.RCPlogs 
--ORDER BY Timestamp;