dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:"Backend/**/TestResults/*/coverage.cobertura.xml" `
                -targetdir:"coveragereport" `
                -reporttypes:Html `
                -classfilters:"-Domain.DTOs.*;-*.Migrations.*"

Start-Process "coveragereport\index.html"
