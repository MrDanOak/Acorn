SELECT c.*,
       i.ItemId,
       i.Amount
FROM Characters c
         LEFT JOIN
     Inventory i ON c.Name = i.Characters_Name
WHERE c.Name = @name