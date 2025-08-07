using System.ComponentModel.DataAnnotations;
namespace FirmaDasboardDemo.Enums
{
    public enum BayiMesajTuru
    {
        [Display(Name = "Hesaplama Sonucu")]
        HesaplamaSonucu = 0,

        [Display(Name = "Teklif Talebi")]
        Teklif = 1,

        [Display(Name = "Destek Talebi")]
        Destek = 2
    }
}
