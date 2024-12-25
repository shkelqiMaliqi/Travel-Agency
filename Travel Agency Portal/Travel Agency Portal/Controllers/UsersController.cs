using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Travel_Agency_Portal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            string query = @"SELECT * FROM dbo.Users";
            DataTable table = ExecuteQuery(query);
            return new JsonResult(table);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            string query = "SELECT * FROM dbo.Users WHERE U_Id = @U_Id";
            return ExecuteQueryWithId(query, id);
        }

        [HttpPost]
        public IActionResult AddUser(Users userData)
        {
            string query = @"INSERT INTO dbo.Users (U_Name, U_Surname, U_Email, U_Phone, U_Password, U_RepeatPassword, U_Type)
                             VALUES (@U_Name, @U_Surname, @U_Email, @U_Phone, @U_Password, @U_RepeatPassword, @U_Type)";
            ExecuteNonQuery(query, userData);
            return Ok("User registered successfully");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] Users user)
        {
            string query = @"UPDATE dbo.Users SET U_Name = @U_Name, U_Surname = @U_Surname, U_Email = @U_Email, 
                             U_Phone = @U_Phone, U_Password = @U_Password, 
                             U_RepeatPassword = @U_RepeatPassword, U_Type = @U_Type WHERE U_Id = @U_Id";
            ExecuteNonQuery(query, user, id);
            return Ok("User updated successfully");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            string query = "DELETE FROM dbo.Users WHERE U_Id = @U_Id";
            ExecuteNonQuery(query, id: id);
            return Ok("User deleted successfully");
        }

        
         //Utility methods for database operations
        private DataTable ExecuteQuery(string query)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("CRUDCS");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCon.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(myCommand);
                    adapter.Fill(table);
                }
            }
            return table;
        }

        private IActionResult ExecuteQueryWithId(string query, int id)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("CRUDCS");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@U_Id", id);
                    myCon.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(myCommand);
                    adapter.Fill(table);
                }
            }
            return table.Rows.Count > 0 ? Ok(table.Rows[0]) : NotFound();
        }

        private void ExecuteNonQuery(string query, Users user = null, int? id = null)
        {
            string sqlDataSource = _configuration.GetConnectionString("CRUDCS");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    if (user != null)
                    {
                        myCommand.Parameters.AddWithValue("@U_Name", user.U_Name);
                        myCommand.Parameters.AddWithValue("@U_Surname", user.U_Surname);
                        myCommand.Parameters.AddWithValue("@U_Email", user.U_Email);
                        myCommand.Parameters.AddWithValue("@U_Phone", user.U_Phone);
                        myCommand.Parameters.AddWithValue("@U_Password", user.U_Password);
                        myCommand.Parameters.AddWithValue("@U_RepeatPassword", user.U_RepeatPassword);
                        myCommand.Parameters.AddWithValue("@U_Type", user.U_Type);
                    }
                    if (id.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@U_Id", id.Value);
                    }
                    myCon.Open();
                    myCommand.ExecuteNonQuery();
                }
            }
        }
        
    }
}
