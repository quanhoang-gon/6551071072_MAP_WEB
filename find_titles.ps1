$files = @('sec1_dichvu.txt', 'sec2_tiennghi.txt', 'sec3_khachhang.txt', 'sec4_nhanvien.txt', 'sec5_chitiettiennghi.txt', 'sec6_datphong.txt')
foreach ($f in $files) {
    Write-Host "File: $f"
    Get-Content $f -Head 5 | Out-String | Write-Host
}
