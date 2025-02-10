using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VehicleOwnershipTracks.Reports;

namespace VehicleOwnershipTracks
{
    public partial class FormReport1 : Form
    {
        public FormReport1()
        {
            InitializeComponent();
        }

        private void FormReport1_Load(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vehicles ", con))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds, "vehicles");
                    da.SelectCommand.CommandText = @"SELECT * FROM vehicleownertracks";
                    da.Fill(ds, "vehicleownertracks");
                   Reprt1 rpt = new Reprt1();
                    rpt.SetDataSource(ds);
                    crystalReportViewer1.ReportSource = rpt;
                    rpt.Refresh();
                    crystalReportViewer1.Refresh();
                }
            }
        }
    }
}
