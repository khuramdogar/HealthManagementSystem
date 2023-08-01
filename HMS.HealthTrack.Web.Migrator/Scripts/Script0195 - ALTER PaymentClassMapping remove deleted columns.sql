BEGIN TRANSACTION
  ALTER TABLE [Inventory].[PaymentClassMapping]
  DROP COLUMN DeletedBy, DeletedOn
  COMMIT