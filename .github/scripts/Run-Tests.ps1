$ErrorActionPreference = "Stop"

$testProjects = @(
    "src/cCoder.CodeAnalysis.Tests/cCoder.CodeAnalysis.Tests.csproj",
    "src/cCoder.CodeAnalysis.Sample.Tests/cCoder.CodeAnalysis.Sample.Tests.csproj",
    "src/cCoder.CodeAnalysis.Sample.AcceptanceTests/cCoder.CodeAnalysis.Sample.AcceptanceTests.csproj"
)

$processes = foreach ($project in $testProjects) {
    Start-Process dotnet -NoNewWindow -PassThru -ArgumentList @(
        "test",
        $project,
        "-c", "Release",
        "--no-build",
        "--no-restore"
    )
}

$processes | Wait-Process
$failedProcesses = @($processes | Where-Object ExitCode -ne 0)

if ($failedProcesses.Count -ne 0) {
    throw "$($failedProcesses.Count) test project(s) failed."
}
