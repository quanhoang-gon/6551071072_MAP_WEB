using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Models;

// File riêng (partial) để khi scaffold lại DB, EF Core Power Tools ghi đè HotelManagementContext.cs
// thì cấu hình kết nối này không bị mất.
public partial class HotelManagementContext
{
    // Cấu hình kết nối tới SQL Server trên máy
    private const string ConnectionString =
        @"Server=.\MSSQLSERVER01;Database=HotelManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    // Constructor không tham số để dùng: new HotelManagementContext()
    public HotelManagementContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(ConnectionString);
    }
}
