#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["AuthorizerTests/AuthorizerTests.csproj", "AuthorizerTests/"]
COPY ["Authorizer/Authorizer.csproj", "Authorizer/"]
RUN dotnet restore "AuthorizerTests/AuthorizerTests.csproj"
RUN dotnet restore "Authorizer/Authorizer.csproj"

COPY . .
WORKDIR "/src/AuthorizerTests"
RUN dotnet test "AuthorizerTests.csproj" 

WORKDIR "/src/Authorizer"
RUN dotnet build "Authorizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authorizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authorizer.dll"]
