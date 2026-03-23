using Motwane.UVSS.Domain.Entities;
using Motwane.UVSS.Application.Interfaces.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Motwane.UVSS.DAL.Repositories
{
    public class VehicleEntryRepository : IVehicleEntryRepository
    {

        private readonly string _connectionString;
       
        public VehicleEntryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int GetTotalRowCount(DateTime? from, DateTime? to, string user, string plate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(@"SELECT COUNT(*) FROM vehicle_entry_log
                WHERE   (@FromDate IS NULL OR entry_date >= @FromDate)
                AND (@ToDate IS NULL OR entry_date < @ToDate)
                AND (@Username IS NULL OR username = @Username)
                AND (@Numberplate IS NULL OR numberplate LIKE '%' + @Numberplate + '%')", conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", (object)from ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)to ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Username", (object)user ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Numberplate", (object)plate ?? DBNull.Value);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public List<string> GetDistinctUsernames()
        {
            List<string> users = new List<string>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT DISTINCT username FROM vehicle_entry_log", conn))
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
            using (SqlCommand cmd = new SqlCommand("sp_GetVehicleEntryLogs", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

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
                            Username = r["username"] as string,
                            EntryDate = r["entry_date"] as DateTime?,
                            EntryTime = r["entry_time"] as TimeSpan?,
                            Status = r["status"] as string,
                            Remark = r["remark"] as string,
                            Numberplate = r["numberplate"] as string,
                            UndersideBytes = r["underside_image"] as byte[],
                            DriverCamBytes = r["driver_cam_image"] as byte[],
                            AnprBytes = r["anpr_image"] as byte[]
                        };
                    }
                }
            }
        }

        public string GetLastVehicleRemark(string numberplate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    $"Select Top(1) [remark] from vehicle_entry_log where [numberplate]= '{numberplate}' order by Concat(entry_date,[entry_time]) desc",
                    conn
                );

                SqlDataAdapter adpt = new SqlDataAdapter(cmd);

                cmd.ExecuteNonQuery();

                DataTable dt = new DataTable();
                adpt.Fill(dt);

                if (dt.Rows.Count == 0)
                    return null;

                return dt.Rows[0][0].ToString();
            }
        }

        public void InsertVehicleEntry(
           
            string username,
            DateTime entryDate,
            TimeSpan entryTime,
            string status,
            string remark,
            string numberplate,
            byte[] undersideImage,
            byte[] driverCamImage,
            byte[] anprImage
        )
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                INSERT INTO vehicle_entry_log 
                (username, entry_date, entry_time, status, remark, numberplate, underside_image, driver_cam_image, anpr_image)
                VALUES 
                (@username, @entry_date, @entry_time, @status, @remark, @numberplate, @underside_image, @driver_cam_image, @anpr_image)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@entry_date", entryDate);
                    cmd.Parameters.AddWithValue("@entry_time", entryTime);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@remark", remark);
                    cmd.Parameters.AddWithValue("@numberplate", numberplate);
                    cmd.Parameters.AddWithValue("@underside_image", undersideImage);
                    cmd.Parameters.AddWithValue("@driver_cam_image", driverCamImage);
                    cmd.Parameters.AddWithValue("@anpr_image", anprImage);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertVideoManagementRecord(
            
            string vehicleNumber,
            string video1,
            string video2,
            string video3,
            byte[] vehicleImage
        )
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO video_management_table
                (vehicle_number, capture_date, capture_time,
                 video1_path, video2_path, video3_path, vehicle_image)
                VALUES (@vehicle_number, @date, @time,
                        @video1, @video2, @video3, @image)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@vehicle_number", vehicleNumber);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now.TimeOfDay);
                    cmd.Parameters.AddWithValue("@video1", video1);
                    cmd.Parameters.AddWithValue("@video2", video2);
                    cmd.Parameters.AddWithValue("@video3", video3);
                    cmd.Parameters.AddWithValue("@image", (object)vehicleImage ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}