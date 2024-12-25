using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace Travel_Agency_Portal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlacesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetPlaces()
        {
            string query = "SELECT * FROM dbo.Places";
            DataTable table = ExecuteQuery(query);
            return new JsonResult(table);
        }

        [HttpGet("{id}")]
        public IActionResult GetPlace(int id)
        {
            string query = "SELECT * FROM dbo.Places WHERE Place_Id = @Place_Id";
            return ExecuteQueryWithId(query, id);
        }

        [HttpPost]
        public IActionResult AddPlace([FromBody] Places p)
        {
            string query = @"INSERT INTO dbo.Places (Place_Name, Place_Description) VALUES (@Place_Name, @Place_Description)";
            ExecuteNonQuery(query, p);
            return Ok("Place added successfully");
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePlace(int id, [FromBody] Places p)
        {
            string query = @"UPDATE dbo.Places SET Place_Name = @Place_Name, Place_Description = @Place_Description WHERE Place_Id = @Place_Id";
            ExecuteNonQuery(query, p, id);
            return Ok("Place updated successfully");
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePlace(int id)
        {
            string query = "DELETE FROM dbo.Places WHERE Place_Id = @Place_Id";
            ExecuteNonQuery(query, id: id);
            return Ok("Place deleted successfully");
        }

        //Utility methods for database operations (similar to UsersController)
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
                    myCommand.Parameters.AddWithValue("@Place_Id", id);
                    myCon.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(myCommand);
                    adapter.Fill(table);
                }
            }
            return table.Rows.Count > 0 ? Ok(table.Rows[0]) : NotFound();
        }

        private void ExecuteNonQuery(string query, Place place = null, int? id = null)
        {
            string sqlDataSource = _configuration.GetConnectionString("CRUDCS");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    if (place != null)
                    {
                        myCommand.Parameters.AddWithValue("@Place_Name", place.Place_Name);
                        myCommand.Parameters.AddWithValue("@Place_Description", place.Place_Description);
                    }
                    if (id.HasValue)
                    {
                        myCommand.Parameters.AddWithValue("@Place_Id", id.Value);
                    }
                    myCon.Open();
                    myCommand.ExecuteNonQuery();
                }
            }
        }

        
    }
}
