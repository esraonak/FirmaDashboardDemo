using FirmaDasboardDemo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FormulTablosu
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Ad { get; set; }

    public string Aciklama { get; set; }

    // Hangi ürün için oluşturuldu?
    public int UrunId { get; set; }
    [ForeignKey("UrunId")]
    public Urun Urun { get; set; }

    // Hangi çalışan tarafından eklendi?
    public int CalisanId { get; set; }
    [ForeignKey("CalisanId")]
    public FirmaCalisani Calisan { get; set; }

    // Oluşturulma zamanı
    public DateTime OlusturmaTarihi { get; set; }

    // Hücreler
    public ICollection<Hucre> Hucreler { get; set; }
}
