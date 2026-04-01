using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.DAL.Repositories
{
    public class VehicleEntryRepository : IVehicleEntryRepository
    {
        private readonly string _connectionString;

        public VehicleEntryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int InsertVehicleEntry(
            string username,
            DateTime entryDate,
            TimeSpan entryTime,
            string status,
            string remark,
            string numberplate,
            string undersideImagePath,
            string driverImagePath,
            string anprImagePath)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                            INSERT INTO TB_Vehicle_Entry_Log
                            (
                                User_ID,
                                Machine_ID,
                                Entry_Date,
                                Entry_time,
                                AIC_Status,
                                Operator_Action,
                                Remark,
                                Vehicle_Registration_No
                            )
                            OUTPUT INSERTED.V_Entry_ID
                            VALUES
                            (
                                @User_ID,
                                @Machine_ID,
                                @Entry_Date,
                                @Entry_time,
                                @AIC_Status,
                                @Operator_Action,
                                @Remark,
                                @Vehicle_Registration_No
                            )";

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@User_ID", Convert.ToInt32(username));
                            cmd.Parameters.AddWithValue("@Machine_ID", 1);
                            cmd.Parameters.AddWithValue("@Entry_Date", entryDate.Date);
                            cmd.Parameters.AddWithValue("@Entry_time", entryDate.Date + entryTime);
                            cmd.Parameters.AddWithValue("@AIC_Status", status ?? "");
                            cmd.Parameters.AddWithValue("@Operator_Action", "ENTRY");
                            cmd.Parameters.AddWithValue("@Remark", remark ?? "");
                            cmd.Parameters.AddWithValue("@Vehicle_Registration_No", numberplate ?? "");

                            int entryId = Convert.ToInt32(cmd.ExecuteScalar());

                            transaction.Commit();
                            return entryId;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public int GetTotalRowCount(DateTime? from, DateTime? to, string user, string plate)
        {
            return 0;
        }

        public List<string> GetDistinctUsernames()
        {
            return new List<string>();
        }

        public IEnumerable<VehicleEntry> GetVehicleEntryLogs(
            DateTime? from,
            DateTime? to,
            string user,
            string plate)
        {
            return new List<VehicleEntry>();
        }

        public string GetLastVehicleRemark(string numberplate)
        {
            return "Normal";
        }

        public void InsertVideoManagementRecord(
            int entryId,
            string vehicleNumber,
            string video1,
            string video2,
            string video3,
            string vehicleImagePath)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    INSERT INTO TB_Vehicle_Entry_Media
                    (
                        V_Entry_ID,
                        Vehicle_Registration_No,
                        Underside_image_path,
                        Video_Cam1,
                        Video_Cam2,
                        Video_Cam3
                    )
                    VALUES
                    (
                        @V_Entry_ID,
                        @Vehicle_Registration_No,
                        @Underside_image_path,
                        @Video_Cam1,
                        @Video_Cam2,
                        @Video_Cam3
                    )";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@V_Entry_ID", entryId);
                    cmd.Parameters.AddWithValue("@Vehicle_Registration_No", vehicleNumber ?? "");
                    cmd.Parameters.AddWithValue("@Underside_image_path", vehicleImagePath ?? "");
                    cmd.Parameters.AddWithValue("@Video_Cam1", video1 ?? "");
                    cmd.Parameters.AddWithValue("@Video_Cam2", video2 ?? "");
                    cmd.Parameters.AddWithValue("@Video_Cam3", video3 ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}