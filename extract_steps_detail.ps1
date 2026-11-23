$files = @(
    @{ Path = "sec1_dichvu.txt"; Name = "Mục 1: Dịch vụ" },
    @{ Path = "sec2_tiennghi.txt"; Name = "Mục 2: Tiện nghi" },
    @{ Path = "sec3_khachhang.txt"; Name = "Mục 3: Khách hàng" },
    @{ Path = "sec4_nhanvien.txt"; Name = "Mục 4: Nhân viên" },
    @{ Path = "sec5_chitiettiennghi.txt"; Name = "Mục 5: Chi tiết tiện nghi" },
    @{ Path = "sec6_datphong.txt"; Name = "Mục 6: Đặt phòng" }
)

foreach ($f in $files) {
    Write-Output "`n=================================================="
    Write-Output "SECTION: $($f.Name)"
    Write-Output "=================================================="
    $text = [System.IO.File]::ReadAllText($f.Path)
    $matches = [System.Text.RegularExpressions.Regex]::Matches($text, "(Bư.*?c\s*\d+\s*:[\s\S]*?)(?=Bư.*?c\s*\d+\s*:|=== PAGE|\Z)")
    foreach ($m in $matches) {
        $val = ($m.Groups[1].Value -replace '\s+', ' ').Trim()
        if ($val.Length > 200) { $val = $val.Substring(0, 200) + "..." }
        Write-Output "  -> $val"
    }
}
