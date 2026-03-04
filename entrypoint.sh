#!/bin/sh
set -e

# Append the search path to the connection string for the migration bundle.
# This is a workaround because the EF Core migration does not schema-qualify enum types in CREATE TABLE statements.
MIGRATION_CONN_STRING="${ConnectionStrings__DefaultConnection};Search Path=public,currency_enum,enrollment_enum,idempotency_resource_type_enum,payment_channel_enum,payment_transaction_status_enum,payment_type_enum,program_enum"

# Apply migrations using the modified connection string
./efbundle --connection "$MIGRATION_CONN_STRING"

# Start the application
dotnet StudentPayments_API.dll