SELECT DepartmentFamily,SUM(Ammount) 
FROM SomeTableGeneratedFromCsvFIle 
GROUP BY DepartmentFamily