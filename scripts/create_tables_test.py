from dbconn import connect

# Imports connection to Azure SQL Server DB from dbconn.py

def run():
    with connect() as conn:
        cursor = conn.cursor()
