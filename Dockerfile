FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /source
COPY . .

RUN dotnet publish -c Release -o /output

CMD ["dotnet", "/output/QuestBot.dll"]