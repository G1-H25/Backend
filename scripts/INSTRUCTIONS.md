# Inscructions for using scripts

1. Create a secrets.py (will be ignored by .gitignore)
2. Replace below with correct information 
```python
DRIVER = '{ODBC Driver 18 for SQL Server}'
SERVER = 'servername'
DATABASE = 'databasename'
USERNAME = 'username'
PASSWORD = 'pwd'
```
3. Use: ```python from dbconn import connect``` to connect to your database
4. Design scritps

