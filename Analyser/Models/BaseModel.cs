using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Analyser.Models
{
    public class BaseModel
    {
        public string Id { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime EditDate { get; set; }

        public string Create()
        {
            Id = Guid.NewGuid().ToString();
            CreateDate = DateTime.Now;
            EditDate = CreateDate;
            return Id;
        }
    }
}