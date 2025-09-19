import os
import pyodbc
import time

# currently has no actual point, might be needed later when connecting to production? keeping it here for now either way

conn_str = os.getenv("DefaultConnection")
if not conn_str:
    raise Exception("Missing DefaultConnection string")

# Retry loop for SQL Server to become available
for i in range(10):
    try:
        conn = pyodbc.connect(conn_str)
        break
    except Exception as e:
        print(f"DB not ready yet, retrying ({i+1}/10)...")
        time.sleep(3)
else:
    raise Exception("DB connection failed after retries")
