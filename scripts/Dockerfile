FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN apt-get update && \
    apt-get install -y curl apt-transport-https gnupg && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/trusted.gpg.d/microsoft.gpg && \
    curl -sSL https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 unixodbc-dev && \
    ln -s /opt/mssql-tools18/bin/sqlcmd /usr/bin/sqlcmd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY . /scripts
RUN chmod +x /scripts/entrypoint.sh && chown -R mssql:mssql /scripts

ENV PATH="$PATH:/opt/mssql-tools18/bin"

USER mssql

ENTRYPOINT ["/scripts/entrypoint.sh"]
