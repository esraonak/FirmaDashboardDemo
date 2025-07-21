using FirmaDasboardDemo.Models;
using System.ComponentModel.DataAnnotations;

public class Rol
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Ad { get; set; }

    // Navigation
    public ICollection<FirmaCalisani> Calisanlar { get; set; }
    public ICollection<Bayi> Bayiler { get; set; }
}
