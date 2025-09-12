import pyodbc
import secrets

def connect():
    # Build the connection string
    connect_str = (
        f"DRIVER={secrets.DRIVER};"
        f"SERVER={secrets.SERVER};"
        f"DATABASE={secrets.DATABASE};"
        f"UID={secrets.USERNAME};"
        f"PWD={{{secrets.PASSWORD}}};"
        "Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"
    )
    return pyodbc.connect(connect_str)
