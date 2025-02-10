using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VehicleOwnershipTracks.Reports;

namespace VehicleOwnershipTracks
{
    public partial class FormReport2 : Form
    {
        public FormReport2()
        {
            InitializeComponent();
        }

        private void FormReport2_Load(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vehicles ", con))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds, "vehicles1");
                    da.SelectCommand.CommandText = @"SELECT * FROM vehicleownertracks";
                    da.Fill(ds, "vehicleownertracks");
                    //Add image column
                    ds.Tables["vehicles1"].Columns.Add(new DataColumn("image", typeof(byte[])));
                    for (var i = 0; i < ds.Tables["vehicles1"].Rows.Count; i++)
                    {
                        ds.Tables["vehicles1"].Rows[i]["image"] = File.ReadAllBytes($@"..\..\Pictures\{ds.Tables["vehicles1"].Rows[i]["picture"]}");
                    }
                  
                    Report2 rpt = new Report2();
                    rpt.SetDataSource(ds);
                    crystalReportViewer1.ReportSource = rpt;
                    rpt.Refresh();
                    crystalReportViewer1.Refresh();
                }
            }
        }
    }
}
