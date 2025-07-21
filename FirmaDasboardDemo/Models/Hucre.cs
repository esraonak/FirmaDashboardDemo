using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Hucre
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string HucreAdi { get; set; } // Örn: A1, B2

    [Required]
    public string Formul { get; set; } // Örn: =X+Y veya sabit sayı

    public bool IsFormul { get; set; }

    // Bağlı olduğu tablo
    public int TabloId { get; set; }
    [ForeignKey("TabloId")]
    public FormulTablosu Tablo { get; set; }
}
