using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Motwane_UVSS.Application.Interfaces.DAL;
using Motwane_UVSS.Domain.Entities;

namespace Motwane_UVSS.DAL.Repositories
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

                string query = "SELECT * FROM video_management_table";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new VideoRecord
                        {
                            id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,

                            vehicle_number = reader["vehicle_number"]?.ToString(),

                            capture_date = reader["capture_date"] != DBNull.Value
                                ? Convert.ToDateTime(reader["capture_date"])
                                : DateTime.MinValue,

                            capture_time = reader["capture_time"] != DBNull.Value
                                ? (TimeSpan)reader["capture_time"]
                                : TimeSpan.Zero,

                            video1_path = reader["video1_path"]?.ToString(),

                            video2_path = reader["video2_path"]?.ToString(),

                            video3_path = reader["video3_path"]?.ToString(),

                            vehicle_image = reader["vehicle_image"] as byte[]
                        });
                    }
                }
            }

            return data;
        }
    }
}