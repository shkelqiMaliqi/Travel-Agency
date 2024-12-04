using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Xml.Linq;

namespace Travel_Agency_Portal.Models
{
    public class Users
    {

        [Key]
        public int U_Id { get; set; }

        public string U_Name { get; set; }
        public string U_Surname { get; set; }

        public string U_Email { get; set; }
        public string U_Username { get; set; }
        public int U_Phone { get; set; }
        public string U_Password { get; set; }
        public string U_RepeatPassword { get; set; }
        public string U_Type { get; set; }


    }
}
