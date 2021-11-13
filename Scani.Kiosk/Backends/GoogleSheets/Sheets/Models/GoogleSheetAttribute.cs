using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class GoogleSheetAttribute : Attribute
    {
        private readonly int _headerCount;
        private readonly string _sheetName;

        public GoogleSheetAttribute(string SheetName, int HeaderCount)
        {
            this._sheetName = SheetName;
            this._headerCount = HeaderCount;
        }

        public string SheetName => _sheetName;

        public int HeaderCount => _headerCount;

        public static bool TryGetSheetName<T>([NotNullWhen(true)] out string? sheetName)
        {
            sheetName = typeof(T)
                .GetCustomAttribute<GoogleSheetAttribute>()
                ?.SheetName;
            return sheetName != null;
        }

        public static string GetSheetName<T>()
        {
            if (!TryGetSheetName<T>(out var sheetName))
            {
                throw new ArgumentException($"The type {typeof(T)} does does not have a [GoogleSheet] attribute");
            }

            return sheetName;
        }

        public static bool TryGetNumberOfHeaderRows<T>([NotNullWhen(true)] out int? numberOfHeaderRows)
        {
            numberOfHeaderRows = typeof(T)
                .GetCustomAttribute<GoogleSheetAttribute>()
                ?.HeaderCount;
            return numberOfHeaderRows != null;
        }
    }
}
