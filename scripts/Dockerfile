FROM mcr.microsoft.com/mssql/server:2022-latest

# Install Python and required libraries
#RUN apt-get update && \
#    apt-get install -y python3 python3-pip unixodbc-dev && \
#    pip3 install pyodbc


# Switch to root to install packages
USER root

# Install SQLCMD and other tools
RUN apt-get update && \
    apt-get install -y curl apt-transport-https gnupg && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/trusted.gpg.d/microsoft.gpg && \
    curl -sSL https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 unixodbc-dev && \
    ln -s /opt/mssql-tools18/bin/sqlcmd /usr/bin/sqlcmd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Switch back to the default user for SQL Server

# Copy scripts and entrypoint
COPY . /scripts
RUN chmod +x /scripts/entrypoint.sh
ENTRYPOINT ["/scripts/entrypoint.sh"]


USER mssql