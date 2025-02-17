﻿using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Entities
{
    public class Auction : IUserOwnedResource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; } // Visur UTCNOW laikas
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double MinIncrement { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public string? Manufacturer { get; set; }
        public string ImageUri { get; set; } // del rodymo paziuret kaip skelbiu ratio padaro
        public Part Part { get; set; }
        [Required]
        public string UserId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User User { get; set; } 
    }
}
