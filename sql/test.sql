CREATE TABLE testtable (
    personID INT,
);

-- BAD: SELECT * usage
SELECT * FROM testtable;

-- BAD: missing semicolon
SELECT personID FROM testtable

-- BAD: lowercase keyword
select personID FROM testtable;

-- BAD: alias without AS
SELECT personID my_id FROM testtable;

-- GOOD: clean select with alias and semicolon
SELECT personID AS my_id FROM testtable;