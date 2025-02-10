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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace VehicleOwnershipTracks
{
    public partial class Form1 : Form
    {
        readonly BindingSource bsV = new BindingSource();
        readonly BindingSource bsO = new BindingSource();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            LoadDataBindingSources();
        }
        public void LoadDataBindingSources()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vehicles ", con))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds, "vehicles");
                    da.SelectCommand.CommandText = @"SELECT * FROM vehicleownertracks";
                    da.Fill(ds, "vehicleownertracks");
                    //Add image column
                    ds.Tables["vehicles"].Columns.Add(new DataColumn("image", typeof(byte[])));
                    for (var i = 0; i < ds.Tables["vehicles"].Rows.Count; i++)
                    {
                        ds.Tables["vehicles"].Rows[i]["image"] = File.ReadAllBytes($@"..\..\Pictures\{ds.Tables["vehicles"].Rows[i]["picture"]}");
                    }
                    DataRelation rel = new DataRelation("FK_V_O", ds.Tables["vehicles"].Columns["vehicleid"], ds.Tables["vehicleownertracks"].Columns["vehicleid"]);
                    ds.Relations.Add(rel);
                    bsV.DataSource = ds;
                    bsV.DataMember = "vehicles";

                    bsO.DataSource = bsV;
                    bsO.DataMember = "FK_V_O";

                    dataGridView1.DataSource = bsO;
                    AddDataBindings();
                }
            }
        }

        private void AddDataBindings()
        {

            lblType.DataBindings.Clear();
            lblType.DataBindings.Add(new Binding("Text", bsV, "vehicletype"));
            lblModel.DataBindings.Clear();
            lblModel.DataBindings.Add(new Binding("Text", bsV, "model"));
            lblChesis.DataBindings.Clear();
            lblChesis.DataBindings.Add(new Binding("Text", bsV, "chesisno"));
            lblReg.DataBindings.Clear();
            lblReg.DataBindings.Add(new Binding("Text", bsV, "regno"));
            //lblprice.DataBindings.Add(bp);
            pictureBox1.DataBindings.Clear();
            pictureBox1.DataBindings.Add(new Binding("Image", bsV, "image", true));
            Binding bm = new Binding("Text", bsV, "importormanufacturedate", true);
            bm.Format += Bm_Format;
            lblImMf.DataBindings.Clear();
            lblImMf.DataBindings.Add(bm);
            checkBox1.DataBindings.Clear();
            checkBox1.DataBindings.Add("Checked", bsV, "islifeexpired", true);

        }

        private void Bm_Format(object sender, ConvertEventArgs e)
        {
            DateTime d = (DateTime)e.Value;
            e.Value = d.ToString("yyyy-MM-dd");
        }

        private void Bp_Format(object sender, ConvertEventArgs e)
        {
            decimal d = (decimal)e.Value;
            e.Value = d.ToString("0.00");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bsV.Position < bsV.Count - 1) bsV.MoveNext();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (bsV.Position >0) bsV.MovePrevious();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bsV.MoveLast();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bsV.MoveFirst();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAddNew {  SyncForm=this}.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int id = int.Parse((bsV.Current as DataRowView).Row[0].ToString());
            new FormEditExisting { SyncForm=this, IdToEdit=id}.ShowDialog();
        }

        private void report1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormReport1().Show();
        }

        private void report2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormReport2().Show();
        }
    }
}
