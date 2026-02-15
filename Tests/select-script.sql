SELECT * 
FROM (
       SELECT 1 AS [Id], 'Test One' AS [Name]
 UNION SELECT 2 AS [Id], 'Test Two' AS [Name]
) q