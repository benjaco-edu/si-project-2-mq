FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as base
WORKDIR  /app

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 as final
WORKDIR /app

COPY --from=base /app/out .
ENTRYPOINT [ "dotnet", "Splitter.dll" ]