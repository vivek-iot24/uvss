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


        // Validate User Login
        public int ValidateUser(string userName, string password, string userType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT COUNT(*)
                FROM TB_Users U
                INNER JOIN TB_Usertype T
                    ON U.Usertype_UUID = T.Usertype_UUID
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


        // Get User UUID from User Name
        public Guid GetUserUUIDByUserName(SqlConnection connection, string userName)
        {
            string query = @"
                SELECT User_UUID
                FROM TB_Users
                WHERE User_Name = @User_Name";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@User_Name", userName);

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("User not found: " + userName);
                }

                return (Guid)result;
            }
        }


        // Insert Login Log
        public void InsertLoginLog(string userName, Guid machineUUID, DateTime loginTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Guid userUUID = GetUserUUIDByUserName(connection, userName);

                string query = @"
                INSERT INTO TB_User_Login_log
                (
                    User_UUID,
                    Machine_UUID,
                    Logged_in
                )
                VALUES
                (
                    @User_UUID,
                    @Machine_UUID,
                    @Logged_in
                )";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_UUID", userUUID);
                    cmd.Parameters.AddWithValue("@Machine_UUID", machineUUID);
                    cmd.Parameters.AddWithValue("@Logged_in", loginTime);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Update Logout Log
        public void UpdateLogoutLog(string userName, DateTime logoutTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Guid userUUID = GetUserUUIDByUserName(connection, userName);

                string query = @"
                UPDATE TB_User_Login_log
                SET Logged_out = @Logged_out
                WHERE User_UUID = @User_UUID
                AND Logged_out IS NULL";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_UUID", userUUID);
                    cmd.Parameters.AddWithValue("@Logged_out", logoutTime);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Insert New User
        public void InsertUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                INSERT INTO TB_Users
                (
                    Usertype_UUID,
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
                    @Usertype_UUID,
                    @User_Name,
                    @Mobile_no,
                    @Comapny_Name,
                    @AgencyName,
                    @Password,
                    @IdNo,
                    'Active'
                )";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Usertype_UUID", user.Usertype_UUID);
                    cmd.Parameters.AddWithValue("@User_Name", user.UserName);
                    cmd.Parameters.AddWithValue("@Mobile_no", user.MobileNo);
                    cmd.Parameters.AddWithValue("@Comapny_Name", user.CompanyName);
                    cmd.Parameters.AddWithValue("@AgencyName", user.AgencyName);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@IdNo", user.IdNo);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Get User Details by UUID
        public User GetUserById(Guid userUUID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT *
                FROM TB_Users
                WHERE User_UUID = @User_UUID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_UUID", userUUID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserUUID = (Guid)reader["User_UUID"],
                                UserName = reader["User_Name"].ToString(),
                                MobileNo = reader["Mobile_no"].ToString(),
                                CompanyName = reader["Comapny_Name"].ToString(),
                                AgencyName = reader["AgencyName"].ToString(),
                                Password = reader["Password"].ToString(),
                                IdNo = reader["IdNo"].ToString(),
                                UserStatus = reader["user_status"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }
        // Update User Details
        public void UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                UPDATE TB_Users
                SET 
                    Usertype_UUID = @Usertype_UUID,
                    User_Name = @User_Name,
                    Mobile_no = @Mobile_no,
                    Comapny_Name = @Comapny_Name,
                    AgencyName = @AgencyName,
                    Password = @Password,
                    IdNo = @IdNo,
                    user_status = @user_status,
                    isactive = @isactive
                WHERE User_UUID = @User_UUID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_UUID", user.UserUUID);
                    cmd.Parameters.AddWithValue("@Usertype_UUID", user.Usertype_UUID);
                    cmd.Parameters.AddWithValue("@User_Name", user.UserName);
                    cmd.Parameters.AddWithValue("@Mobile_no", user.MobileNo);
                    cmd.Parameters.AddWithValue("@Comapny_Name", user.CompanyName);
                    cmd.Parameters.AddWithValue("@AgencyName", user.AgencyName);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@IdNo", user.IdNo);
                    cmd.Parameters.AddWithValue("@user_status", user.UserStatus);
                    cmd.Parameters.AddWithValue("@isactive", user.IsActive);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Get All Active Users
        public DataTable GetAllUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT 
                    User_UUID,
                    User_Name,
                    Mobile_no,
                    Comapny_Name,
                    AgencyName,
                    IdNo,
                    user_status,
                    isactive
                FROM TB_Users
                WHERE ISNULL(user_status, 'Active') = 'Active'";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }


        // Get Deleted / Inactive User History
        public DataTable GetDeletedUserHistory()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT 
                    User_UUID,
                    User_Name,
                    Mobile_no,
                    Comapny_Name,
                    AgencyName,
                    IdNo,
                    user_status,
                    isactive
                FROM TB_Users
                WHERE user_status = 'Inactive'";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
        // Get User Login History
        public DataTable GetUserLoginLog(Guid userUUID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT 
                    User_Login_UUID,
                    Machine_UUID,
                    Logged_in,
                    Logged_out
                FROM TB_User_Login_log
                WHERE User_UUID = @User_UUID
                ORDER BY Logged_in DESC";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_UUID", userUUID);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }


        // Soft Delete User
        public void DeleteUser(Guid userUUID, string userName, string idNo)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                UPDATE TB_Users
                SET 
                    user_status = 'Inactive',
                    isactive = 'False'
                WHERE User_UUID = @User_UUID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_UUID", userUUID);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}