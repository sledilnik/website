dotnet tool install dotnet-fsharplint
dotnet fsharplint --format msbuild lint -l fsharplint-config.json SloCovid19Website.sln
yarn test-fsharp
