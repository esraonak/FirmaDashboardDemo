using System.Text;

namespace FirmaDasboardDemo.Helpers
{
    public static class SifreUretici
    {
        public static string RastgeleSifreUret(int uzunluk = 8)
        {
            const string harfler = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            const string rakamlar = "23456789";
            const string tumKarakterler = harfler + rakamlar;

            var random = new Random();
            var sb = new StringBuilder();

            sb.Append(harfler[random.Next(harfler.Length)]);
            sb.Append(rakamlar[random.Next(rakamlar.Length)]);

            for (int i = 2; i < uzunluk; i++)
                sb.Append(tumKarakterler[random.Next(tumKarakterler.Length)]);

            return new string(sb.ToString().ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
        }
    }
}
