﻿using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public class KioskSheetReadResult<T>
        where T : ISheetRow
    {
        public bool Ok { get; set; }
        public ICollection<T> Rows { get; set; } = new List<T>();
        public ICollection<KioskSheetReadError> Errors { get; set; } = new List<KioskSheetReadError>();
        public IList<string> FlexFieldNames { get; set; } = new List<string>();
    }
}
