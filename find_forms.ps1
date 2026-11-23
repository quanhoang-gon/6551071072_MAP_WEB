# Script to list all Form and Designer files in lab 2
$files = @("sec1_dichvu.txt", "sec2_tiennghi.txt", "sec3_khachhang.txt", "sec4_nhanvien.txt", "sec5_chitiettiennghi.txt", "sec6_datphong.txt")
foreach ($f in $files) {
    Write-Output "=== $f ==="
    $content = Get-Content $f -Raw
    $matches = [regex]::Matches($content, "(?:Form\w+\.cs|Form\w+\.Designer\.cs|file\s+Form\w+)")
    $found = @{}
    foreach ($m in $matches) {
        $found[$m.Value] = $true
    }
    $found.Keys | ForEach-Object { Write-Output "   $_" }
}
