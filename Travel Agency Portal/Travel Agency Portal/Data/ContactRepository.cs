using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class ContactRepository(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public List<ContactMessage> GetMessages()
    {
        const string query = SqlQuery2;

        var messages = new List<ContactMessage>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            messages.Add(new ContactMessage
            {
                C_Id = reader.GetInt32(reader.GetOrdinal("C_Id")),
                C_Name = reader.GetString(reader.GetOrdinal("C_Name")),
                C_Surname = reader.GetString(reader.GetOrdinal("C_Surname")),
                C_Email = reader.GetString(reader.GetOrdinal("C_Email")),
                C_Subject = reader.GetString(reader.GetOrdinal("C_Subject")),
                C_Message = reader.GetString(reader.GetOrdinal("C_Message")),
                U_Id = reader.IsDBNull(reader.GetOrdinal("U_Id")) ? null : reader.GetInt32(reader.GetOrdinal("U_Id")),
                C_IsRead = reader.GetBoolean(reader.GetOrdinal("C_IsRead")),
                C_IsArchived = reader.GetBoolean(reader.GetOrdinal("C_IsArchived")),
                C_CreatedAt = reader.GetDateTime(reader.GetOrdinal("C_CreatedAt"))
            });
        }

        return messages;
    }

    public int Delete(int id)
    {
        const string query = SqlQuery5;
        return ExecuteNonQuery(query, new SqlParameter("@C_Id", id));
    }

    public int Archive(int id)
    {
        const string query = SqlQuery4;
        return ExecuteNonQuery(query, new SqlParameter("@C_Id", id));
    }

    public int MarkAsRead(int id) => ExecuteNonQuery(SqlQuery3, new SqlParameter("@C_Id", id));

    public int Create(ContactMessage message, int? userId)
    {
        return ExecuteNonQuery(SqlQuery1,
            new SqlParameter("@C_Name", message.C_Name),
            new SqlParameter("@C_Surname", message.C_Surname),
            new SqlParameter("@C_Email", message.C_Email),
            new SqlParameter("@C_Subject", message.C_Subject),
            new SqlParameter("@C_Message", message.C_Message),
            new SqlParameter("@U_Id", (object?)userId ?? DBNull.Value));
    }

    private const string SqlQuery1 = @"
            INSERT INTO dbo.Contact_Form (C_Name, C_Surname, C_Email, C_Subject, C_Message, U_Id, C_IsRead, C_IsArchived, C_CreatedAt)
            VALUES (@C_Name, @C_Surname, @C_Email, @C_Subject, @C_Message, @U_Id, 0, 0, SYSUTCDATETIME())";
    private const string SqlQuery2 = @"
            SELECT C_Id, C_Name, C_Surname, C_Email, C_Subject, C_Message, U_Id, C_IsRead, C_IsArchived, C_CreatedAt
            FROM dbo.Contact_Form
            WHERE C_IsArchived = 0
            ORDER BY C_CreatedAt DESC, C_Id DESC";
    private const string SqlQuery3 = "UPDATE dbo.Contact_Form SET C_IsRead = 1 WHERE C_Id = @C_Id";
    private const string SqlQuery4 = "UPDATE dbo.Contact_Form SET C_IsArchived = 1 WHERE C_Id = @C_Id";
    private const string SqlQuery5 = "DELETE FROM dbo.Contact_Form WHERE C_Id = @C_Id";
}
