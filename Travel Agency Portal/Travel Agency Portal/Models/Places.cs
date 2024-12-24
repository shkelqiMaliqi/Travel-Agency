using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Xml.Linq;


namespace Travel_Agency_Portal.Models
{
    public class Places
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]

        public string Description { get; set; }
        [Required]

        public string Url { get; set; }
        [Required]

        

    }

}