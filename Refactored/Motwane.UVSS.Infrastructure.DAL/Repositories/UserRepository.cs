using System;
using System.Data;
using System.Data.SqlClient;
using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int ValidateUser(string userName, string password, string userType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT COUNT(*)
            FROM TB_Users U
            INNER JOIN TB_Usertype T
                ON U.Usertype_ID = T.Usertype_ID
            WHERE U.User_Name = @User_Name
            AND U.Password = @Password
            AND T.Usertype = @Usertype
            AND ISNULL(U.user_status, 'Active') = 'Active'";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_Name", userName);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@Usertype", userType);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void InsertLoginLog(string userName, DateTime loginTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int userId = GetUserIdByUserName(connection, userName);

                string loginQuery = @"
            INSERT INTO TB_User_Login_log
            (User_ID, Logged_in)
            VALUES
            (@User_ID, @Logged_in)";

                using (SqlCommand cmd = new SqlCommand(loginQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@User_ID", userId);
                    cmd.Parameters.AddWithValue("@Logged_in", loginTime);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void UpdateLogoutLog(string userName, DateTime logoutTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int userId = GetUserIdByUserName(connection, userName);

                string logoutQuery = @"
            UPDATE TB_User_Login_log
            SET Logged_out = @Logged_out
            WHERE User_ID = @User_ID
            AND Logged_out IS NULL";

                using (SqlCommand cmd = new SqlCommand(logoutQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@User_ID", userId);
                    cmd.Parameters.AddWithValue("@Logged_out", logoutTime);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void InsertUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            INSERT INTO TB_Users
            (Usertype_ID,
                User_Name,
                Mobile_no,
                Comapny_Name,
                AgencyName,
                Password,
                IdNo,
                user_status
            )
            VALUES
            (
                @Usertype_ID,
                @User_Name,
                @Mobile_no,
                @Comapny_Name,
                @AgencyName,
                @Password,
                @IdNo,
                'Active'
            )";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usertype_ID", user.Usertype_ID);
                cmd.Parameters.AddWithValue("@User_Name", user.UserName);
                cmd.Parameters.AddWithValue("@Mobile_no", user.MobileNo);
                cmd.Parameters.AddWithValue("@Comapny_Name", user.CompanyName);
                cmd.Parameters.AddWithValue("@AgencyName", user.AgencyName);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@IdNo", user.IdNo);

                cmd.ExecuteNonQuery();
            }
        }

        public User GetUserById(string userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
    SELECT *
    FROM TB_Users
    WHERE User_ID = @User_ID";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserName = reader["UserName"].ToString(),
                        IdNo = reader["IdNo"].ToString(),
                        MobileNo = reader["MobileNo"].ToString(),
                        CompanyName = reader["CompanyName"].ToString(),
                        AgencyName = reader["AgencyName"].ToString(),
                        UserID = reader["UserID"].ToString(),
                        Password = reader["Password"].ToString(),
                        UserType = reader["UserType"].ToString()
                    };
                }
            }

            return null;
        }
        public int GetUserIdByUserName(SqlConnection connection, string userName)
        {
            string query = @"
        SELECT User_ID
        FROM TB_Users
        WHERE User_Name = @User_Name";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@User_Name", userName);

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    throw new Exception($"User not found: {userName}");

                return Convert.ToInt32(result);
            }
        }

        public void UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
    UPDATE TB_Users
    SET User_Name = @User_Name,
        Mobile_no = @Mobile_no,
        Comapny_Name = @Comapny_Name,
        AgencyName = @AgencyName,
        IdNo = @IdNo,
        Password = @Password
    WHERE User_ID = @User_ID";
                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@MobileNo", user.MobileNo);
                cmd.Parameters.AddWithValue("@CompanyName", user.CompanyName);
                cmd.Parameters.AddWithValue("@AgencyName", user.AgencyName);
                cmd.Parameters.AddWithValue("@UserID", user.UserID);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@UserType", user.UserType);
                cmd.Parameters.AddWithValue("@IdNo", user.IdNo);

                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetAllUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                User_ID,
                User_Name,
                Mobile_no,
                Comapny_Name,
                AgencyName,
                IdNo,
                user_status
            FROM TB_Users
            WHERE ISNULL(user_status, 'Active') = 'Active'";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetDeletedUserHistory()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT *
            FROM TB_Users
            WHERE user_status = 'Inactive'";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetUserLoginLog(string userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT Logged_in, Logged_out
            FROM TB_User_Login_log
            WHERE User_ID = @User_ID
            ORDER BY Logged_in DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@User_ID", userId);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable logTable = new DataTable();
                adapter.Fill(logTable);

                return logTable;
            }
        }
        public void DeleteUser(string userId, string userName, string idNo)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            UPDATE TB_Users
            SET user_status = 'Inactive'
            WHERE User_ID = @User_ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@User_ID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}