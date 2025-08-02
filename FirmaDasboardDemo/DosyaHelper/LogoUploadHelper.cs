using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

public static class LogoUploadHelper
{
    public static string LogoYukle(IWebHostEnvironment env, IFormFile logoFile, string firmaSeoUrl)
    {
        if (logoFile == null || logoFile.Length == 0)
            return null;

        string firmaKlasorYolu = Path.Combine(env.WebRootPath, "uploads", firmaSeoUrl);
        if (!Directory.Exists(firmaKlasorYolu))
            Directory.CreateDirectory(firmaKlasorYolu);

        string dosyaAdi = "logo.png";
        string tamYol = Path.Combine(firmaKlasorYolu, dosyaAdi);

        // 🔁 Eski logo varsa sil
        if (File.Exists(tamYol))
        {
            File.Delete(tamYol);
        }

        using (var stream = logoFile.OpenReadStream())
        using (var image = Image.Load(stream))
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(300, 300),
                Mode = ResizeMode.Max
            }));

            image.Save(tamYol, new PngEncoder());
        }

        return $"/uploads/{firmaSeoUrl}/{dosyaAdi}";
    }

}
