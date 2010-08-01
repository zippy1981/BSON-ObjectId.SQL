-- Demonstrates byte shifting in SQL.
-- Inspires by: http://sqlblog.com/blogs/adam_machanic/archive/2006/07/12/bitmask-handling-part-4-left-shift-and-right-shift.aspx
SELECT
	CAST(0xFF * POWER(CAST(256 AS BIGINT), 0) AS BINARY(12)) AS Shift0,
	CAST(0xFF * POWER(CAST(256 AS BIGINT), 1) AS BINARY(12)) AS Shift1,
	CAST(0xFF * POWER(CAST(256 AS BIGINT), 2) AS BINARY(12)) AS Shift2,
	CAST(0xFF * POWER(CAST(256 AS BIGINT), 3) AS BINARY(12)) AS Shift3,
	CAST(0xFF * POWER(CAST(256 AS BIGINT), 4) AS BINARY(12)) AS Shift4,
	CAST(0xFF * POWER(CAST(256 AS BIGINT), 5) AS BINARY(12)) AS Shift5