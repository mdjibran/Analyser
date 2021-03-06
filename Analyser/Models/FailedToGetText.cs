namespace Analyser.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FailedToGetText")]
    public partial class FailedToGetText : BaseModel
    {

        public string URL { get; set; }

        [Required]
        [StringLength(250)]
        public string Company { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [Required]
        [StringLength(250)]
        public string City { get; set; }

        [Required]
        [StringLength(250)]
        public string Provience { get; set; }
    }
}
