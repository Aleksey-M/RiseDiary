SELECT COUNT(*) AS 'Deleted Records Count' FROM Records WHERE Deleted = 1;
SELECT COUNT(*) AS 'Deleted Scopes Count' FROM Scopes WHERE Deleted = 1;
SELECT COUNT(*) AS 'Deleted Themes Count' FROM Themes WHERE Deleted = 1;
SELECT COUNT(*) AS 'Deleted Record Themes Count' FROM RecordThemes WHERE Deleted = 1;
SELECT COUNT(*) AS 'Deleted Images Count' FROM Images WHERE Deleted = 1;
SELECT COUNT(*) AS 'Deleted Record Images Count' FROM RecordImages WHERE Deleted = 1;


DELETE FROM RecordThemes WHERE Deleted = 1;
DELETE FROM RecordImages WHERE Deleted = 1;
DELETE FROM Themes WHERE Deleted = 1;
DELETE FROM Scopes WHERE Deleted = 1;
DELETE FROM Records WHERE Deleted = 1;
DELETE FROM Images WHERE Deleted = 1;