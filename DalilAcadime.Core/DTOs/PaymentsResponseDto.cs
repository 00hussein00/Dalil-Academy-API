using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.DTOs
{
    public record PaymentsResponseDto
    {
        public int PaymentId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string ApprovalUrl { get; set; } = string.Empty;
    }

}
