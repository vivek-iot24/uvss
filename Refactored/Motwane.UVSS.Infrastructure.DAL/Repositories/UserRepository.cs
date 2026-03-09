using System;
using System.Data;
using System.Data.SqlClient;
using Motwane_UVSS.Application.Interfaces.DAL;
using Motwane_UVSS.Domain.Entities;

namespace Motwane_UVSS.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int ValidateUser(string userId, string password, string userType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM dbo.Users WHERE UserID = @UserID AND Password = @Password AND UserType = @UserType";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@UserType", userType);

                    int count = (int)cmd.ExecuteScalar();
                    return count;
                }
            }
        }

        public void InsertLoginLog(string userId, DateTime loginTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string loginQuery = @" INSERT INTO user_login_log (UserID, LoginTime)VALUES (@UserID, @LoginTime)";

                using (SqlCommand cmd = new SqlCommand(loginQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@LoginTime", loginTime);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLogoutLog(string userId, DateTime logoutTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string logoutQuery = @" UPDATE user_login_log SET LogoutTime = @LogoutTime WHERE UserID = @UserID AND LogoutTime IS NULL";

                using (SqlCommand cmd = new SqlCommand(logoutQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@LogoutTime", logoutTime);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query =
                    "INSERT INTO dbo.Users (UserName, MobileNo, CompanyName, AgencyName, UserID, Password,UserType,IdNo) " +
                    "VALUES (@UserName, @MobileNo, @CompanyName, @AgencyName, @UserID, @Password, @UserType, @IdNo)";

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

        public User GetUserById(string userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query =
                    "SELECT UserName, MobileNo, CompanyName, AgencyName, UserID, Password, UserType, IdNo FROM dbo.Users WHERE UserID = @UserID";

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

        public void UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query =
                    "UPDATE dbo.Users SET UserName = @UserName, MobileNo = @MobileNo, CompanyName = @CompanyName, AgencyName = @AgencyName, @IdNo = IdNo, Password = @Password, UserType = @UserType WHERE UserID = @UserID";

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

                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM dbo.Users", con);
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

                SqlDataAdapter da =
                    new SqlDataAdapter("SELECT * FROM dbo.deleted_user_history", con);

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

                string query =
                    @"SELECT LoginTime, LogoutTime FROM user_login_log WHERE UserID = @UserID ORDER BY LoginTime DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userId);

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

                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    string insertQuery =
                        @"INSERT INTO deleted_user_history (UserName, UserID, IdNo, DeletedDate)
                          VALUES (@UserName, @UserID, @IdNo, GETDATE())";

                    SqlCommand insertCmd =
                        new SqlCommand(insertQuery, con, transaction);

                    insertCmd.Parameters.AddWithValue("@UserName", userName);
                    insertCmd.Parameters.AddWithValue("@UserID", userId);
                    insertCmd.Parameters.AddWithValue("@IdNo", idNo);

                    insertCmd.ExecuteNonQuery();

                    string deleteQuery =
                        "DELETE FROM dbo.Users WHERE UserID = @UserID";

                    SqlCommand deleteCmd =
                        new SqlCommand(deleteQuery, con, transaction);

                    deleteCmd.Parameters.AddWithValue("@UserID", userId);

                    deleteCmd.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }
    }
}