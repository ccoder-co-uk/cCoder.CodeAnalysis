param([switch] $Apply)

$ErrorActionPreference = 'Stop'
$sourceRoot = Join-Path $PSScriptRoot '..\src'
$validationFiles = Get-ChildItem $sourceRoot -Recurse -Filter '*.Validations.cs' |
    Where-Object Length -eq 0

foreach ($validationFile in $validationFiles) {
    $basePath = $validationFile.FullName -replace '\.Validations\.cs$', '.cs'
    $exceptionPath = $validationFile.FullName -replace '\.Validations\.cs$', '.Exceptions.cs'

    if (!(Test-Path $basePath) -or !(Test-Path $exceptionPath)) {
        continue
    }

    $source = [IO.File]::ReadAllText($basePath)
    $exceptionStart = [regex]::Match(
        $source,
        '(?m)^\s*private static (?:async )?(?:[\w<>?]+\s+)?TryCatch(?:<T>)?\s*\('
    )
    $validationStart = [regex]::Match($source, '(?m)^\s*private static void Validate\s*\(')

    if (!$exceptionStart.Success -or !$validationStart.Success -or
        $validationStart.Index -le $exceptionStart.Index) {
        continue
    }

    $classMatch = [regex]::Match(
        $source,
        '(?m)^(?<indent>\s*)internal sealed (?:partial )?class (?<name>\w+)'
    )

    if (!$classMatch.Success) {
        continue
    }

    $lastBrace = $source.LastIndexOf('}')
    $preamble = $source.Substring(0, $classMatch.Index)
    $className = $classMatch.Groups['name'].Value
    $exceptionMembers = $source.Substring(
        $exceptionStart.Index,
        $validationStart.Index - $exceptionStart.Index
    ).Trim()
    $validationMembers = $source.Substring(
        $validationStart.Index,
        $lastBrace - $validationStart.Index
    ).Trim()
    $root = $source.Remove(
        $exceptionStart.Index,
        $lastBrace - $exceptionStart.Index + 1
    )
    $root = [regex]::Replace(
        $root,
        "(?m)^internal sealed (?!partial )class $className",
        "internal sealed partial class $className"
    ).TrimEnd() + "`r`n}`r`n"
    $partialPreamble = $preamble.TrimEnd() + "`r`n`r`n"
    $exceptionSource = $partialPreamble +
        "internal sealed partial class $className`r`n{`r`n" +
        $exceptionMembers + "`r`n}`r`n"
    $validationSource = $partialPreamble +
        "internal sealed partial class $className`r`n{`r`n" +
        $validationMembers + "`r`n}`r`n"

    Write-Host "$className"

    if ($Apply) {
        [IO.File]::WriteAllText($basePath, $root)
        [IO.File]::WriteAllText($exceptionPath, $exceptionSource)
        [IO.File]::WriteAllText($validationFile.FullName, $validationSource)
    }
}
