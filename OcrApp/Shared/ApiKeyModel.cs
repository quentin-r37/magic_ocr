using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrApp.Shared
{
    public class ApiKeyModel
    {
        public string? Id { get; set; }
        public string? Key { get; set; }
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
