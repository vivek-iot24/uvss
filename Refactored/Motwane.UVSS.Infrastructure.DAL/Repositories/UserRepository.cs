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
        public User GetUserById(string userId)
        {
            return GetUserByName(userId);
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
                Comapny_Name = @Comapny_Name
            WHERE User_ID = @User_ID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_ID", Convert.ToInt32(user.UserID));
                    cmd.Parameters.AddWithValue("@User_Name", user.UserName);
                    cmd.Parameters.AddWithValue("@Mobile_no", user.MobileNo);
                    cmd.Parameters.AddWithValue("@Comapny_Name", user.CompanyName);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetDeletedUserHistory()
        {
            return new DataTable();
        }

        public void DeleteUser(string userId, string userName, string idNo)
        {
            DeleteUser(Convert.ToInt32(userId));
        }
        public int ValidateUser(string userName, string passwordHash, string userType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT COUNT(*)
                    FROM TB_Users U
                    INNER JOIN TB_Usertype T ON U.Usertype_ID = T.Usertype_ID
                    WHERE U.User_Name = @User_Name
                    AND U.PasswordHash = @PasswordHash
                    AND T.Usertype = @Usertype";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_Name", userName);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@Usertype", userType);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void InsertLoginLog(int userId, int machineId, DateTime loginTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO TB_User_Login_log
                    (User_ID, Machine__ID, Logged_in)
                    VALUES
                    (@User_ID, @Machine__ID, @Logged_in)";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_ID", userId);
                    cmd.Parameters.AddWithValue("@Machine__ID", machineId);
                    cmd.Parameters.AddWithValue("@Logged_in", loginTime);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLogoutLog(int userId, DateTime logoutTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    UPDATE TB_User_Login_log
                    SET Logged_out = @Logged_out
                    WHERE User_ID = @User_ID
                    AND Logged_out IS NULL";

                using (SqlCommand cmd = new SqlCommand(query, connection))
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
                    (
                        Usertype_ID,
                        Authentication_ID,
                        User_Name,
                        Mobile_no,
                        Comapny_Name,
                        PasswordHash,
                        PasswordSalt
                    )
                    VALUES
                    (
                        @Usertype_ID,
                        @Authentication_ID,
                        @User_Name,
                        @Mobile_no,
                        @Comapny_Name,
                        @PasswordHash,
                        @PasswordSalt
                    )";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Usertype_ID", user.Usertype_ID);
                    cmd.Parameters.AddWithValue("@Authentication_ID", user.Authentication_ID);
                    cmd.Parameters.AddWithValue("@User_Name", user.UserName);
                    cmd.Parameters.AddWithValue("@Mobile_no", user.MobileNo);
                    cmd.Parameters.AddWithValue("@Comapny_Name", user.CompanyName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public User GetUserByName(string userName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM TB_Users
                    WHERE User_Name = @User_Name";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User_Name", userName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserID = reader["User_ID"].ToString(),
                                Usertype_ID = Convert.ToInt32(reader["Usertype_ID"]),
                                Authentication_ID = Convert.ToInt32(reader["Authentication_ID"]),
                                UserName = reader["User_Name"].ToString(),
                                MobileNo = reader["Mobile_no"].ToString(),
                                CompanyName = reader["Comapny_Name"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                PasswordSalt = reader["PasswordSalt"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public DataTable GetAllUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
                    SELECT
                        U.User_ID,
                        U.User_Name,
                        U.Mobile_no,
                        U.Comapny_Name,
                        T.Usertype
                    FROM TB_Users U
                    INNER JOIN TB_Usertype T
                        ON U.Usertype_ID = T.Usertype_ID";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetUserLoginLog(int userId)
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
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                return dt;
            }
        }

        public void DeleteUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = "DELETE FROM TB_Users WHERE User_ID = @User_ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@User_ID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}