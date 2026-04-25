using System;
using System.Collections.Generic;

namespace Aura.Application.DTOs.Project
{
    public class SlotAvailabilityResponseDTO
    {
        public DateTime Date { get; set; }
        public int BookedCount { get; set; }
        public bool IsAvailable { get; set; }
        public IEnumerable<Guid> BookedProjectIds { get; set; } = new List<Guid>();
    }
}
