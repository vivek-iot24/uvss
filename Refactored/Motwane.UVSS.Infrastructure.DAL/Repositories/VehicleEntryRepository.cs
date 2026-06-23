using Motwane.UVSS.Domain.Entities;
using Motwane.UVSS.Application.Interfaces.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Motwane.UVSS.Application.ComputerVision;

namespace Motwane.UVSS.DAL.Repositories
{
    public class VehicleEntryRepository : IVehicleEntryRepository
    {
        private readonly string _connectionString;

        public VehicleEntryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public int GetTotalRowCount(
    DateTime? from,
    DateTime? to,
    string user,
    string plate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT COUNT(*)
            FROM TB_Vehicle_Entry_Log V
            INNER JOIN TB_Users U
                ON V.User_ID = U.User_ID
            WHERE
                (@FromDate IS NULL OR V.Entry_Date >= @FromDate)
                AND (@ToDate IS NULL OR V.Entry_Date <= @ToDate)
                AND (@Username IS NULL OR U.User_Name = @Username)
                AND (@Numberplate IS NULL OR V.Vehicle_Registration_No LIKE '%' + @Numberplate + '%')";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", (object)from ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)to ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Username", (object)user ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Numberplate", (object)plate ?? DBNull.Value);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        private int GetUserIdByUserName(
            SqlConnection conn,
            SqlTransaction transaction,
            string username)
        {
            string query = @"
                SELECT User_ID
                FROM TB_Users
                WHERE User_Name = @User_Name";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@User_Name", username);

                object result = cmd.ExecuteScalar();

                if (result == null)
                    throw new Exception("User not found.");

                return Convert.ToInt32(result);
            }
        }

        public List<string> GetDistinctUsernames()
        {
            List<string> users = new List<string>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT DISTINCT U.User_Name
                  FROM TB_Vehicle_Entry_Log V
                  INNER JOIN TB_Users U ON V.User_ID = U.User_ID", conn))
            {
                conn.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        users.Add(r.GetString(0));
                }
            }

            return users;
        }

        public IEnumerable<VehicleEntry> GetVehicleEntryLogs(
            DateTime? from,
            DateTime? to,
            string user,
            string plate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT
                        U.User_Name,
                        V.Entry_Date,
                        V.Entry_time,
                        V.AIC_Status,
                        V.Remark,
                        V.Vehicle_Registration_No,
                        M.Underside_image_path,
                        M.Driver_image_path,
                        M.ANPR_image_path
                    FROM TB_Vehicle_Entry_Log V
                    INNER JOIN TB_Users U
                        ON V.User_ID = U.User_ID
                    LEFT JOIN TB_Vehicle_Entry_Media M
                        ON V.V_Entry_ID = M.V_Entry_ID
                    WHERE
                        (@FromDate IS NULL OR V.Entry_Date >= @FromDate)
                        AND (@ToDate IS NULL OR V.Entry_Date <= @ToDate)
                        AND (@Username IS NULL OR U.User_Name = @Username)
                        AND (@Numberplate IS NULL OR V.Vehicle_Registration_No LIKE '%' + @Numberplate + '%')
                    ORDER BY V.Entry_time DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", (object)from ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)to ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Username", (object)user ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Numberplate", (object)plate ?? DBNull.Value);

                    conn.Open();

                    int sr = 0;

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            yield return new VehicleEntry
                            {
                                SrNo = ++sr,
                                Username = r["User_Name"]?.ToString(),
                                EntryDate = r["Entry_Date"] as DateTime?,
                                EntryTime = r["Entry_time"] as DateTime?,
                                Status = r["AIC_Status"]?.ToString(),
                                Remark = r["Remark"]?.ToString(),
                                Numberplate = r["Vehicle_Registration_No"]?.ToString(),
                                UndersideImagePath = r["Underside_image_path"]?.ToString(),
                                DriverImagePath = r["Driver_image_path"]?.ToString(),
                                AnprImagePath = r["ANPR_image_path"]?.ToString()
                            };
                        }
                    }
                }
            }
        }

        public string GetLastVehicleRemark(string numberplate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT TOP 1 Remark
                    FROM TB_Vehicle_Entry_Log
                    WHERE Vehicle_Registration_No = @Vehicle_Registration_No
                    ORDER BY Entry_time DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Vehicle_Registration_No", numberplate);

                    object result = cmd.ExecuteScalar();

                    return result?.ToString();
                }
            }
        }

        public void InsertVehicleEntry(
        string username,
        DateTime entryDate,
        DateTime entryTime,
        string status,
        string remark,
        string numberplate,
        string undersideImage,
        string driverCamImage,
        string anprImage,
        string videoCam1,
        string videoCam2,
        string videoCam3)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // STEP 1: username -> User_ID
                        int userId = GetUserIdByUserName(conn, transaction, username);

                        // STEP 2: insert main log
                        string logQuery = @"
                    INSERT INTO TB_Vehicle_Entry_Log
                    (
                        User_ID,
                        Entry_Date,
                        Entry_time,
                        AIC_Status,
                        Remark,
                        Vehicle_Registration_No
                    )
                    OUTPUT INSERTED.V_Entry_ID
                    VALUES
                    (
                        @User_ID,
                        @Entry_Date,
                        @Entry_time,
                        @AIC_Status,
                        @Remark,
                        @Vehicle_Registration_No
                    )";

                        int entryId;

                        using (SqlCommand cmd = new SqlCommand(logQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@User_ID", userId);
                            cmd.Parameters.AddWithValue("@Entry_Date", entryDate);
                            cmd.Parameters.AddWithValue("@Entry_time", entryTime);
                            cmd.Parameters.AddWithValue("@AIC_Status", status);
                            cmd.Parameters.AddWithValue("@Remark", remark);
                            cmd.Parameters.AddWithValue("@Vehicle_Registration_No", numberplate);

                            entryId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // STEP 3: insert media row using generated V_Entry_ID
                        string mediaQuery = @"
                    INSERT INTO TB_Vehicle_Entry_Media
                    (
                        V_Entry_ID,
                        Vehicle_Registration_No,
                        Underside_image_path,
                        Driver_image_path,
                        ANPR_image_path,
                        Video_Cam1,
                        Video_Cam2,
                        Video_Cam3
                    )
                    VALUES
                    (
                        @V_Entry_ID,
                        @Vehicle_Registration_No,
                        @Underside_image_path,
                        @Driver_image_path,
                        @ANPR_image_path,
                        @Video_Cam1,
                        @Video_Cam2,
                        @Video_Cam3
                    )";

                        using (SqlCommand cmd = new SqlCommand(mediaQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@V_Entry_ID", entryId);
                            cmd.Parameters.AddWithValue("@Vehicle_Registration_No", numberplate);
                            cmd.Parameters.AddWithValue("@Underside_image_path", (object)undersideImage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Driver_image_path", (object)driverCamImage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ANPR_image_path", (object)anprImage ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Video_Cam1", (object)videoCam1 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Video_Cam2", (object)videoCam2 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Video_Cam3", (object)videoCam3 ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }

                        // STEP 4: commit both tables
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


    }
}