using System;

namespace Aura.Application.DTOs.Project
{
    public class RescheduleRequestDTO
    {
        public Guid ProjectId { get; set; }
        public DateTime NewShootingDate { get; set; }
    }
}
