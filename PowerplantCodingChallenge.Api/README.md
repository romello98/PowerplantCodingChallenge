# powerplant-coding-challenge

## Launch the app

### Docker

You can use the `Dockerfile` located in the project folder
- Execute from the solution folder (root) the command `docker build -t powerplant-challenge -f PowerplantCodingChallenge.Api\Dockerfile .`
- Run the newly created image with this command: `docker run -p 8888:8888 powerplant-challenge`. 

### Dotnet CLI

The dotnet CLI can also be used to launch the app.
- Run the `dotnet build` command in the PowerplantCodingChallenge.Api project
- Execute the file `PowerplantCodingChallenge.Api.exe` that is in the `bin\Debug\net8.0` folder

Note that you can also run directly the app using `dotnet run` in the project folder.