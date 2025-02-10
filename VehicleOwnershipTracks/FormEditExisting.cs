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
using static VehicleOwnershipTracks.FormAddNew;

namespace VehicleOwnershipTracks
{
    public partial class FormEditExisting : Form
    {
        readonly List<TrackData> tracks = new List<TrackData>();
        private string currentFile = "";
        private string oldFile = "";
        public FormEditExisting()
        {
            InitializeComponent();
        }
        public Form1 SyncForm { get; set; }
        public int IdToEdit { get; set; }
        private void FormEditExisting_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            LoadDataToForm();
        }

        private void LoadDataToForm()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM vehicles WHERE vehicleid=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", IdToEdit);
                    con.Open();
                    SqlDataReader dr1 = cmd.ExecuteReader();
                    if (dr1.Read())
                    {
                        comboBox1.Text = dr1.GetString(1);
                        textBox1.Text = dr1.GetString(2);
                        dateTimePicker3.Value = dr1.GetDateTime(3);
                        textBox2.Text = dr1.GetString(4);
                        textBox3.Text = dr1.GetString(5);
                        pictureBox1.Image = Image.FromFile(@"..\..\Pictures\" + dr1.GetString(6));
                        oldFile = dr1.GetString(6);
                        checkBox1.Checked = dr1.GetBoolean(7);
                    }
                    dr1.Close();
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT * FROM vehicleownertracks where vehicleid=@i";
                    cmd.Parameters.AddWithValue("@i", IdToEdit);
                    SqlDataReader dr2 = cmd.ExecuteReader();
                    while (dr2.Read())
                    {
                        var t = new TrackData { ownername = dr2.GetString(1), owneraddress = dr2.GetString(2), fromdate = dr2.GetDateTime(3) };
                        if (dr2[4].ToString() != "")
                        {
                            t.todate = dr2.GetDateTime(4);
                        }
                        tracks.Add(t);
                    }
                    dr2.Close();
                    con.Close();
                    SetDataSources();
                }
            }
        }
        private void SetDataSources()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = tracks;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            dateTimePicker2.Enabled = checkBox2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                tracks.RemoveAt(e.RowIndex);
                SetDataSources();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    string sql = @"UPDATE  vehicles SET vehicletype=@t, model=@m, importormanufacturedate=@i, chesisno=@c, regno=@r, picture=@p, islifeexpired=@e
                                    WHERE vehicleid=@id";
                    using (SqlCommand cmd = new SqlCommand(sql, con, tran))
                    {


                        cmd.Parameters.AddWithValue("@id", IdToEdit);
                        cmd.Parameters.AddWithValue("@t", comboBox1.Text);
                        cmd.Parameters.AddWithValue("@m", textBox1.Text);
                        cmd.Parameters.AddWithValue("@i", dateTimePicker3.Value);
                        cmd.Parameters.AddWithValue("@c", textBox2.Text);
                        cmd.Parameters.AddWithValue("@r", textBox3.Text);

                        cmd.Parameters.AddWithValue("@e", checkBox1.Checked);
                        if (currentFile != "")
                        {
                            string ext = Path.GetExtension(this.currentFile);
                            string f = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                            string savePath = @"..\..\Pictures\" + f;
                            MemoryStream ms = new MemoryStream(File.ReadAllBytes(currentFile));
                            byte[] bytes = ms.ToArray();
                            FileStream fs = new FileStream(savePath, FileMode.Create);
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Close();
                            cmd.Parameters.AddWithValue("@p", f);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@p", oldFile);
                        }
                        try
                        {
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "DELETE FROM vehicleownertracks WHERE vehicleid=@id";
                            cmd.Parameters.AddWithValue("@id", IdToEdit);
                            cmd.ExecuteNonQuery();
                            foreach (var t in tracks)
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = @"INSERT INTO vehicleownertracks (ownername, owneraddress, fromdate, todate, vehicleid)
                                    VALUES (@o, @a, @f, @t,@v)";
                                cmd.Parameters.AddWithValue("@o", t.ownername);
                                cmd.Parameters.AddWithValue("@a", t.owneraddress);
                                cmd.Parameters.AddWithValue("@f", t.fromdate);
                                cmd.Parameters.AddWithValue("@v", IdToEdit);
                                if (t.todate != null)
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
                        catch (Exception)
                        {

                            tran.Rollback();
                            MessageBox.Show("Save failed", "Error");

                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    string sql = @"DELETE  vehicleownertracks
                                    WHERE vehicleid=@id";
                    using (SqlCommand cmd = new SqlCommand(sql, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", IdToEdit);
                        try
                        {
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "DELETE FROM vehicles WHERE vehicleid=@id";
                            cmd.Parameters.AddWithValue("@id", IdToEdit);
                            cmd.ExecuteNonQuery();
                            tran.Commit();
                            MessageBox.Show("Data Deleted", "Success");
                            SyncForm.LoadDataBindingSources();
                            this.Close();
                        }
                        catch
                        {
                            tran.Rollback();
                            MessageBox.Show("Failed to Delete", "Error");
                        }
                        con.Close();
                    }
                }
            }
        }
    }
}
