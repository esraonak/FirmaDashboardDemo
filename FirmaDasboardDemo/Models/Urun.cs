using FirmaDasboardDemo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Urun
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Ad { get; set; }

    public string Aciklama { get; set; }

    // İlişkiler
    public int FirmaId { get; set; }
    [ForeignKey("FirmaId")]
    public Firma Firma { get; set; }
 
    public ICollection<FormulTablosu> FormulTablolari { get; set; }
}
