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

                string query = @"
                    SELECT 
                        M.M_ID,
                        E.Vehicle_Registration_No,
                        E.Entry_Date,
                        E.Entry_time,
                        M.Video_Cam1,
                        M.Video_Cam2,
                        M.Video_Cam3,
                        M.Underside_image_path
                    FROM TB_Vehicle_Entry_Media M
                    INNER JOIN TB_Vehicle_Entry_Log E
                        ON M.V_Entry_ID = E.V_Entry_ID
                    ORDER BY E.Entry_DateTime DESC";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new VideoRecord
                        {
                            id = reader["M_ID"] != DBNull.Value
                                ? Convert.ToInt32(reader["M_ID"])
                                : 0,

                            vehicle_number = reader["Vehicle_Registration_No"]?.ToString(),

                            capture_date = reader["Entry_Date"] != DBNull.Value
                                ? Convert.ToDateTime(reader["Entry_Date"])
                                : DateTime.MinValue,

                            capture_time = reader["Entry_time"] != DBNull.Value
                                ? Convert.ToDateTime(reader["Entry_time"]).TimeOfDay
                                : TimeSpan.Zero,

                            video1_path = reader["Video_Cam1"]?.ToString(),

                            video2_path = reader["Video_Cam2"]?.ToString(),

                            video3_path = reader["Video_Cam3"]?.ToString(),

                            // keeping old model compatibility
                            vehicle_image = null,

                            // ✅ if your VideoRecord supports string path add this
                            vehicle_image_path = reader["Underside_image_path"]?.ToString()
                        });
                    }
                }
            }

            return data;
        }
    }
}