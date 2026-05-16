using System.Collections.Generic;

namespace Victorina.Core.Models
{
    public class WinnerInfo
    {
        public string Date { get; set; }

        public string WinnerName { get; set; }

        public List<string> Teams { get; set; } = new();
    }
}