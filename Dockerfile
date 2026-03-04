# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Install dotnet-ef tool and generate migration bundle
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools:$PATH"
# Set dummy connection string to prevent startup crash during bundle generation
ENV ConnectionStrings__DefaultConnection="Host=localhost;Database=dummy;Username=postgres;Password=password"
RUN dotnet ef migrations bundle -o /app/publish/efbundle

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh efbundle

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["./entrypoint.sh"]