SELECT COUNT(RecordId), COUNT(DISTINCT WorkerId), ActionType FROM RCPlogs
WHERE ActionType != 0
AND ActionType != 1
AND ActionType != 5
GROUP BY ActionType;

SELECT * FROM RCPlogs
WHERE ActionType != 0
AND ActionType != 1
AND ActionType != 5;

SELECT COUNT(RecordId), LoggerType FROM RCPlogs
WHERE LoggerType != 0
AND LoggerType != 1
GROUP BY LoggerType;
