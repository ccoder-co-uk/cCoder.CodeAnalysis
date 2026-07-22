param([switch] $Apply)

$ErrorActionPreference = 'Stop'
$testRoot = Join-Path $PSScriptRoot '..\src\cCoder.CodeAnalysis.Sample.Tests'
$methodFiles = Get-ChildItem $testRoot -Recurse -Filter '*Tests.*.cs' |
    Where-Object Length -eq 0
$groups = $methodFiles | Group-Object {
    $_.FullName -replace '\.[^.\\]+\.cs$', '.cs'
}
$methodPattern = '(?ms)^\s*(?:\[[^\]]+\]\s*)+public\s+(?:async\s+)?(?:Task|ValueTask|void)\s+(?<name>\w+)\s*\([^)]*\)\s*\{(?>[^{}]+|\{(?<depth>)|\}(?<-depth>))*(?(depth)(?!))\}'

foreach ($group in $groups) {
    $basePath = $group.Name

    if (!(Test-Path $basePath) -or (Get-Item $basePath).Length -eq 0) {
        continue
    }

    $source = [IO.File]::ReadAllText($basePath)
    $classMatch = [regex]::Match(
        $source,
        '(?m)^public sealed (?:partial )?class (?<name>\w+)'
    )

    if (!$classMatch.Success) {
        continue
    }

    $className = $classMatch.Groups['name'].Value
    $preamble = $source.Substring(0, $classMatch.Index).TrimEnd()
    $matches = [regex]::Matches($source, $methodPattern)
    $removals = [Collections.Generic.List[object]]::new()
    $targets = @($group.Group | ForEach-Object {
        $_.BaseName.Substring($className.Length + 1)
    })

    foreach ($methodFile in $group.Group) {
        $targetMethod = $methodFile.BaseName.Substring($className.Length + 1)
        $selected = @($matches | Where-Object {
            $methodName = $_.Groups['name'].Value
            $bestTarget = $targets |
                Where-Object { $methodName.StartsWith($_, [StringComparison]::Ordinal) } |
                Sort-Object Length -Descending |
                Select-Object -First 1
            $bestTarget -eq $targetMethod
        })

        if ($selected.Count -eq 0) {
            continue
        }

        $members = foreach ($match in $selected) {
            $method = $match.Value.Trim()
            $openBrace = $method.IndexOf('{')
            $method = $method.Insert(
                $openBrace + 1,
                "`r`n        // Given`r`n        // When`r`n        // Then"
            )
            $removals.Add($match)
            $method
        }
        $partialSource = $preamble + "`r`n`r`n" +
            "public sealed partial class $className`r`n{`r`n" +
            ($members -join "`r`n`r`n") + "`r`n}`r`n"

        Write-Host "$className.$targetMethod ($($selected.Count))"

        if ($Apply) {
            [IO.File]::WriteAllText($methodFile.FullName, $partialSource)
        }
    }

    if ($Apply -and $removals.Count -gt 0) {
        foreach ($match in ($removals | Sort-Object Index -Descending -Unique)) {
            $source = $source.Remove($match.Index, $match.Length)
        }
        $source = [regex]::Replace(
            $source,
            "(?m)^public sealed (?!partial )class $className",
            "public sealed partial class $className"
        )
        [IO.File]::WriteAllText($basePath, $source)
    }
}
