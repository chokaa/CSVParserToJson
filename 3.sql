DECLARE   @SQLQuery AS NVARCHAR(MAX)
DECLARE   @PivotColumns AS NVARCHAR(MAX)
 
--Uzimamo sve vrednosti za ExpensesArea ma koje one bile
SELECT   @PivotColumns= COALESCE(@PivotColumns + ',','') + QUOTENAME(ExpensesArea)
FROM (SELECT DISTINCT ExpensesArea FROM [dbo].[PivotExample]) AS PivotExample
 
SELECT   @PivotColumns
 
--Formiramo dinamicki query za pivot kolona 
SET   @SQLQuery = 
    N'SELECT ExpensesType, ' +   @PivotColumns + '
    FROM [dbo].[PivotExample] 
    PIVOT( SUM(ApAmount) 
          FOR ExpensesArea IN (' + @PivotColumns + ')) AS P'
 
SELECT   @SQLQuery
--Izvrsavamo query
EXEC sp_executesql @SQLQuery