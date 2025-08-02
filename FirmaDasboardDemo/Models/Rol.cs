using FirmaDasboardDemo.Models;
using System.ComponentModel.DataAnnotations;

public class Rol
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Ad { get; set; } // Örnek: "SuperAdmin", "Calisan", "Bayi"

    // Navigation Properties
    public ICollection<FirmaCalisani> Calisanlar { get; set; }
    public ICollection<Bayi> Bayiler { get; set; }

    // (Eğer SuperAdmin de kullanıcı tablosundan gelecekse ekle)
    public ICollection<SuperAdmin> SuperAdminler { get; set; }
}