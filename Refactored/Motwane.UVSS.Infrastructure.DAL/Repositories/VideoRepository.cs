using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.DAL.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly string connectionString;

        public VideoRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<VideoRecord> GetAllVideos()
        {
            var data = new List<VideoRecord>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM TB_Vehicle_Entry_Media";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new VideoRecord
                        {
                            id = reader["M_UUID"] != DBNull.Value ? Convert.ToInt32(reader["M_UUID"]) : 0,

                            vehicle_number = reader["Vehicle_Registration_No"]?.ToString(),

                            capture_date = reader["capture_date"] != DBNull.Value
                                ? Convert.ToDateTime(reader["capture_date"])
                                : DateTime.MinValue,

                            capture_time = reader["capture_time"] != DBNull.Value
                                ? (TimeSpan)reader["capture_time"]
                                : TimeSpan.Zero,

                            video1_path = reader["Video_Cam1"]?.ToString(),

                            video2_path = reader["Video_Cam2"]?.ToString(),

                            video3_path = reader["Video_Cam3"]?.ToString(),

                           
                        });
                    }
                }
            }

            return data;
        }
    }
}