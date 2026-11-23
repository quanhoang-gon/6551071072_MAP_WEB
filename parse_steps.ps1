$files = Get-ChildItem -Path "sec*.txt" | Sort-Object Name
foreach ($f in $files) {
    Write-Output "`n=== FILE: $($f.Name) ==="
    $lines = Get-Content $f.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match "^\s*=== PAGE\s+(\d+)" -or $line -match "Bư.*?c\s*(\d+)\s*:" -or $line -match "^\s*(\d+)\.\s+X.*?y\s+d.*?ng") {
            # print matched line trimmed
            $clean = ($line -replace '\s+', ' ').Trim()
            if ($clean.Length > 100) { $clean = $clean.Substring(0, 100) + "..." }
            Write-Output "[$i] $clean"
        }
    }
}
