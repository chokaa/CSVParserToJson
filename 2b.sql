SELECT DepartmentFamily,ExpenseType,SUM(Ammount) 
FROM SomeTableGeneratedFromCsvFIle 
GROUP BY DepartmentFamily,ExpenseType