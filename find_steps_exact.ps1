$files = @(
    @{ F="sec1_dichvu.txt"; Name="Mục 1" },
    @{ F="sec2_tiennghi.txt"; Name="Mục 2" },
    @{ F="sec3_khachhang.txt"; Name="Mục 3" },
    @{ F="sec4_nhanvien.txt"; Name="Mục 4" },
    @{ F="sec5_chitiettiennghi.txt"; Name="Mục 5" },
    @{ F="sec6_datphong.txt"; Name="Mục 6" }
)

foreach ($item in $files) {
    Write-Output "`n=================================================="
    Write-Output "SECTION $($item.Name) ($($item.F))"
    Write-Output "=================================================="
    $text = [System.IO.File]::ReadAllText($item.F)
    $lines = $text -split "\r?\n"
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $l = $lines[$i]
        $match = [regex]::Match($l, "Bư.*?c\s*\d+\s*:[^.]*?(?:cs|\Z)")
        if ($match.Success) {
            Write-Output "Line $i : $($match.Value)"
        }
    }
}
