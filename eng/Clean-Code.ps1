[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryPath = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repositoryPath 'src\cCoder.CodeAnalysis.slnx'
$argumentNamingProject = Join-Path $repositoryPath 'eng\NameArguments\NameArguments.csproj'
$copyrightHeader = @(
    '// ---------------------------------------------------------------'
    '// Copyright (c) Paul.Ward@ccoder.co.uk'
    '// ---------------------------------------------------------------'
    ''
    ''
) -join "`r`n"
$ruleViolationPaths = Get-ChildItem `
    -Path (Join-Path $repositoryPath 'src') `
    -Directory `
    -Recurse `
    -Filter 'RuleViolations' |
    Select-Object -ExpandProperty FullName

$formatArguments = @(
    'format'
    $solutionPath
    '--no-restore'
    '--exclude'
) + $ruleViolationPaths

& dotnet run --project $argumentNamingProject

if ($LASTEXITCODE -ne 0)
{
    throw "Argument naming cleanup failed with exit code $LASTEXITCODE."
}

& dotnet tool restore

if ($LASTEXITCODE -ne 0)
{
    throw "Tool restore failed with exit code $LASTEXITCODE."
}

& dotnet csharpier format $repositoryPath

if ($LASTEXITCODE -ne 0)
{
    throw "C# line wrapping failed with exit code $LASTEXITCODE."
}

& dotnet @formatArguments

if ($LASTEXITCODE -ne 0)
{
    throw "dotnet format failed with exit code $LASTEXITCODE."
}

$sourceFiles = Get-ChildItem `
    -Path (Join-Path $repositoryPath 'src') `
    -File `
    -Recurse |
    Where-Object {
        $_.Extension -in '.cs', '.csproj', '.props', '.targets', '.slnx' -and
        $_.FullName -notmatch '\\(bin|obj)\\'
    }

foreach ($sourceFile in $sourceFiles)
{
    $content = [System.IO.File]::ReadAllText($sourceFile.FullName)
    $content = $content.Replace("`r`n", "`n").Replace("`r", "`n")
    $lines = $content.Split("`n") |
        ForEach-Object { $_.TrimEnd() }

    if ($sourceFile.FullName -notmatch '\\RuleViolations\\')
    {
        while ($lines.Count -gt 0 -and $lines[-1].Length -eq 0)
        {
            $lines = $lines[0..($lines.Count - 2)]
        }
    }

    $normalizedContent = [string]::Join("`r`n", $lines)

    if ($sourceFile.Extension -eq '.cs' -and
        !$sourceFile.FullName.EndsWith(
            'InvalidIdentifierProcessingService.cs',
            [StringComparison]::OrdinalIgnoreCase) -and
        !$normalizedContent.StartsWith(
            $copyrightHeader,
            [StringComparison]::Ordinal))
    {
        $normalizedContent = $copyrightHeader + $normalizedContent.TrimStart("`r", "`n")
    }

    if ($sourceFile.Extension -eq '.cs' -and
        !$sourceFile.FullName.EndsWith(
            'InvalidIdentifierProcessingService.cs',
            [StringComparison]::OrdinalIgnoreCase))
    {
        $expressionBodyPattern =
            '(?m)^([ \t]*(?:public|internal|private|protected)[^\r\n]*\))' +
            '[ \t]*(?:\r?\n[ \t]*)?=>[ \t]*([^\r\n]+)\r?$'

        $normalizedContent = [regex]::Replace(
            $normalizedContent,
            $expressionBodyPattern,
            {
                param($match)
                $indent = [regex]::Match($match.Groups[1].Value, '^[ \t]*').Value
                $expression = $match.Groups[2].Value.TrimStart()
                $expressionIndent = if ($expression.TrimEnd().EndsWith(';')) {
                    $indent + '    '
                }
                else {
                    $indent
                }

                return $match.Groups[1].Value + ' =>' + "`r`n" +
                    $expressionIndent + $expression
            })

    }

    if ($sourceFile.Extension -eq '.cs' -and
        !$sourceFile.FullName.EndsWith(
            'InvalidIdentifierProcessingService.cs',
            [StringComparison]::OrdinalIgnoreCase))
    {
        $chainPattern = '(?m)^([ \t]*)([^\r\n]*\))\.(?=[A-Za-z_][A-Za-z0-9_]*\()'

        do
        {
            $priorContent = $normalizedContent
            $normalizedContent = [regex]::Replace(
                $normalizedContent,
                $chainPattern,
                {
                    param($match)
                    $continuationIndent = $match.Groups[1].Value + '    '

                    return $match.Groups[1].Value + $match.Groups[2].Value.TrimStart() + "`r`n" +
                        $continuationIndent + '.'
                })
        }
        while ($normalizedContent -ne $priorContent)
    }

    if ($sourceFile.FullName -notmatch '\\RuleViolations\\' -and
        $sourceFile.Extension -eq '.cs')
    {
        $methodBoundaryPattern =
            '(?m)(^[ \t]{4}(?:}|[^\r\n]*;))[ \t]*(?:\r?\n[ \t]*)+' +
            '(?=[ \t]{4}(?:public|internal|private|protected)[^\r\n]*\()'

        $normalizedContent = [regex]::Replace(
            $normalizedContent,
            $methodBoundaryPattern,
            '$1' + "`r`n`r`n")
    }

    if (!$normalizedContent.EndsWith("`r`n", [StringComparison]::Ordinal))
    {
        $normalizedContent += "`r`n"
    }

    [System.IO.File]::WriteAllText(
        $sourceFile.FullName,
        $normalizedContent,
        [System.Text.UTF8Encoding]::new($false))
}

& dotnet run --project $argumentNamingProject

if ($LASTEXITCODE -ne 0)
{
    throw "Final structural cleanup failed with exit code $LASTEXITCODE."
}
