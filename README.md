# cCoder.CodeAnalysis

[View the latest code coverage report](https://ccoder-co-uk.github.io/cCoder.CodeAnalysis/)

`cCoder.CodeAnalysis` analyses C# projects against the architectural and coding conventions defined by The Standard. It reports findings through normal compiler diagnostics and produces a portable architecture document for tooling and visualisation.

## Installation

Add one package reference to each project that should be analysed:

```xml
<PackageReference Include="cCoder.CodeAnalysis" Version="2026.7.22.0000" />
```

No separate analyzer or build-task package is required. Building the project activates the compiler analyzer and, for non-test projects, generates `project.stxjson` beside the project file.

The package currently requires the .NET 10 SDK for its build integration.

## Build behaviour

During a normal build the package:

1. Analyses the Roslyn compilation.
2. Emits each analysis item as an `STX` compiler diagnostic, making it visible in build output and the Visual Studio Error List.
3. Generates `project.stxjson` after a successful non-test build.
4. Avoids rewriting an unchanged architecture document.

Projects with `IsTestProject` set to `true` are still analysed, but do not produce an architecture document.

Generation can be disabled or redirected when required:

```xml
<PropertyGroup>
  <cCoderCodeAnalysisGenerateArchitecture>false</cCoderCodeAnalysisGenerateArchitecture>
  <cCoderCodeAnalysisArchitecturePath>$(MSBuildProjectDirectory)\architecture\project.stxjson</cCoderCodeAnalysisArchitecturePath>
</PropertyGroup>
```

## Architecture document

The generated document contains:

- Classes and their Standard element types.
- Properties, methods, inputs, and fully qualified type names.
- Directed links between internal types.
- Analysis items including rule code, description, type, and source line.

Enum values are serialized as readable strings. Source paths are relative to the project containing `project.stxjson`, allowing the document to be moved with the project and consumed directly by Visual Studio or browser-based diagramming tools.

## Standard element types

The model recognises the principal layers used by The Standard, including:

- Brokers and dependencies
- Foundation services
- Processing services
- Orchestration services
- Coordination services
- Management services
- Aggregation services
- Exposures and controllers
- Models
- Tests

Rules cover architectural dependencies, public contracts, exception handling, validation, naming, asynchronous operations, controller behaviour, test structure, source layout, and formatting.

Models are data carriers only. `STXM001` rejects every explicitly declared method-like member, including instance and static constructors, destructors, operators, conversions, and overrides.

## Runtime use

The architecture builder can also be resolved through dependency injection when analysis is needed programmatically:

```csharp
ServiceCollection services = new ServiceCollection();
services.AddCodeAnalysis();

using ServiceProvider serviceProvider = services.BuildServiceProvider();
IArchitectureBuilder architectureBuilder = serviceProvider
    .GetRequiredService<IArchitectureBuilder>();

Architecture architecture = architectureBuilder.Generate(
    path: projectPath);
```

## Building the repository

```powershell
dotnet restore src/cCoder.CodeAnalysis.slnx
dotnet build src/cCoder.CodeAnalysis.slnx -c Release --no-restore
dotnet test src/cCoder.CodeAnalysis.Tests/cCoder.CodeAnalysis.Tests.csproj -c Release --no-build
dotnet test src/cCoder.CodeAnalysis.Sample.Tests/cCoder.CodeAnalysis.Sample.Tests.csproj -c Release --no-build
dotnet pack src/cCoder.CodeAnalysis/cCoder.CodeAnalysis.csproj -c Release --no-restore
```

The sample projects provide known valid and deliberately invalid code used to verify every analysis rule. Deliberate rule violations are expected to appear as compiler warnings during repository builds.
