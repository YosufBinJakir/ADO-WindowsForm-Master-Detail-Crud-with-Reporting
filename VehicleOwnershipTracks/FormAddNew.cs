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

namespace VehicleOwnershipTracks
{
    public partial class FormAddNew : Form
    {
        readonly List<TrackData> tracks = new List<TrackData>();
        public string currentFile = "";
        public FormAddNew()
        {
            InitializeComponent();
        }
        public Form1 SyncForm { get; set; }
        private void FormAddNew_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            pictureBox1.Image = Image.FromFile(@"..\..\Pictures\nopic.jpeg");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker2.Enabled = checkBox2.Checked;
        }
        private void LoadDataToGrid()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = tracks;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TrackData trackData = new TrackData
            {
                ownername = textBox4.Text,
                owneraddress = textBox5.Text,
                fromdate = dateTimePicker1.Value,
                todate = null

            };
            if (checkBox2.Checked)
            {
                trackData.todate = dateTimePicker2.Value;
            }
            tracks.Add(trackData);
            LoadDataToGrid();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.currentFile = this.openFileDialog1.FileName;
                pictureBox1.Image = Image.FromFile(this.currentFile);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    string sql = @"INSERT INTO vehicles (vehicletype, model, importormanufacturedate, chesisno, regno, picture, islifeexpired)
                                    VALUES (@t, @m,@i, @c, @r, @p, @e); SELECT SCOPE_IDENTITY()";
                    using (SqlCommand cmd = new SqlCommand(sql, con, tran))
                    {

                        string ext = Path.GetExtension(this.currentFile);
                        string f = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                        string savePath = @"..\..\Pictures\" + f;
                        MemoryStream ms = new MemoryStream(File.ReadAllBytes(currentFile));
                        byte[] bytes = ms.ToArray();
                        FileStream fs = new FileStream(savePath, FileMode.Create);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();
                        cmd.Parameters.AddWithValue("@t", comboBox1.Text);
                        cmd.Parameters.AddWithValue("@m", textBox1.Text);
                        cmd.Parameters.AddWithValue("@i", dateTimePicker3.Value);
                        cmd.Parameters.AddWithValue("@c", textBox2.Text);
                        cmd.Parameters.AddWithValue("@r", textBox3.Text);
                        cmd.Parameters.AddWithValue("@p", f);
                        cmd.Parameters.AddWithValue("@e", checkBox1.Checked);
                        try
                        {
                            var vid = cmd.ExecuteScalar();
                            foreach (var t in tracks)
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = @"INSERT INTO vehicleownertracks (ownername, owneraddress, fromdate, todate, vehicleid)
                                    VALUES (@o, @a, @f, @t,@v)";
                                cmd.Parameters.AddWithValue("@o", t.ownername);
                                cmd.Parameters.AddWithValue("@a", t.owneraddress);
                                cmd.Parameters.AddWithValue("@f", t.fromdate);
                                cmd.Parameters.AddWithValue("@v", vid);
                                if(t.todate != null)
                                    cmd.Parameters.AddWithValue("@t", t.todate);
                                else
                                    cmd.Parameters.AddWithValue("@t", DBNull.Value);
                                //cmd.Parameters["@t"].Value = (Object)t.todate ?? DBNull.Value;
                                cmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                            MessageBox.Show("Data saved", "Success");
                            SyncForm.LoadDataBindingSources();
                            textBox4.Text = "";
                            textBox5.Text = "";
                            dateTimePicker1.Value = DateTime.Now;
                            dateTimePicker2.Value = DateTime.Now;
                            dateTimePicker3.Value = DateTime.Now;
                            comboBox1.SelectedIndex = -1;
                            textBox1.Text = "";
                            textBox2.Text = "";
                            textBox3.Text = "";
                            checkBox1.Checked = false;
                            checkBox2.Checked = true;
                            dateTimePicker2.Enabled = false;
                        }
                        catch (Exception )
                        {
                            {
                                tran.Rollback();
                                MessageBox.Show("Save failed", "Error");
                            }

                        }
                    }
                }
            }
        }
        public class TrackData
        {
            public string ownername { get; set; }
            public string owneraddress { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime? todate { get; set; }
            public int vehiclid { get; set; }
        }
    }
}
