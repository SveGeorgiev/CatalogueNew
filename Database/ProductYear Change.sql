USE CatalogueSystemDB

ALTER TABLE dbo.Products DROP COLUMN ProductYear
ALTER TABLE dbo.Products ADD Year int NOT NULL
