using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FirmaDasboardDemo.DosyaHelper
{
    public static class EnumDisplayNameExtension
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var enumMember = enumValue.GetType()
                .GetMember(enumValue.ToString());

            if (enumMember.Length > 0)
            {
                var attr = enumMember[0].GetCustomAttribute<DisplayAttribute>();
                if (attr != null)
                    return attr.Name;
            }

            return enumValue.ToString();
        }
    }
}
