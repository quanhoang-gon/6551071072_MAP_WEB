# Dịch bảng ánh xạ font lỗi PDF
$charMap = @{
    [char]0x2012 = "-"  # ‒
    [char]0x2015 = "―"  # ―
    [char]0x2128 = "ℨ"  # 1
    [char]0x2129 = "℩"  # 2
    [char]0x212A = "K"  # 3
    [char]0x212B = "Å"  # 4
    [char]0x212C = "ℬ"  # 5
    [char]0x212D = "ℭ"  # 6
    [char]0x212E = "℮"  # 7
    [char]0x212F = "ℯ"  # 8
    [char]0x2130 = "ℰ"  # 9
    [char]0x03F5 = "ϵ"  # 0
}
Write-Output "Loaded font map"
