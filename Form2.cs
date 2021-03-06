using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Office.Interop;
using System.IO;
using System.Diagnostics;

namespace Dnevnik_2._0
{
    public partial class Form2 : Form
    {
        public SQLiteConnection conn, conn2;
        Form1 f1;
        public Form2()
        {
            InitializeComponent();
        }
        public int id_nastavnika;
        public string username, password;
        public string ucenik, ocene;
        public Size s;

        public List<ucenik> u = new List<ucenik>();
        public List<PictureBox> p = new List<PictureBox>();
        public List<Label> l = new List<Label>();


        public int h_d;

        public int petice = 0, cetvorke = 0, trojke = 0, dvojke = 0, jedinice = 0;

        int n = 0;

        int border = 10;
        int rw, rh, x, y;
        public int a, b;

        private void button1_Click(object sender, EventArgs e)
        {
            if (!dataGridView1.Visible)
            {
                if (chart1.Visible)
                    chart1.Hide();
                else
                    chart1.Show();
            }
            else
            {
                EXCEL();
            }
        }
        //nw je koloko ucenika moze da stane u jedan res
        double nw;
        public void velicina_forme()
        {
            this.Size = new Size(1280, 720);
        }
        public Size velicina_ekrana;
        private void Form2_Load(object sender, EventArgs e)
        {
            Screen Srn = Screen.PrimaryScreen;

            velicina_ekrana = new Size(Srn.Bounds.Width,Srn.Bounds.Height);
            

            //omogucava pristup formi 1
            f1 = (Form1)Application.OpenForms[0];

            conn = f1.conn2;
            conn2 = f1.conn3;

            //postavlja formu na odredjenu velicinu
            velicina_forme();

            //kalkulacija koliko moze ucenika da stane u jednom redu
            Kalkulacije_broja_ucenika();

            //cita iz baze
            UCITAJ();

            progressBar1.Minimum = 0;
            progressBar1.Maximum = dataGridView1.RowCount * (dataGridView1.ColumnCount-1);

            //postavlja picture bog sa slikom i eventom da kad kliknes na taj picture box
            foreach (var pic in p)
            {
                pic.Click += new EventHandler(pictureBox1_Click);
                this.Controls.Add(pic);
            }
            //postavlja label sa imenom ucenika
            foreach (var label in l)
            {
                this.Controls.Add(label);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //pokazije ili sakriva formu
            if (dataGridView1.Visible)
            {
                dataGridView1.Hide();
                velicina_forme();
                foreach (var item in p)
                {
                    this.Controls.Add(item);
                }
                foreach (var item in l)
                {
                    this.Controls.Add(item);
                }
                button1.Text = "Statistika";
            }
            else
            {
                button1.Text = "Excel";
                dataGridView1.Show();
                int d = dataGridView1.Width + dataGridView1.Location.X;
                MessageBox.Show(velicina_ekrana.Width.ToString());
                if (d < velicina_ekrana.Width)
                {
                    this.Width = d; 
                    int dgv = dataGridView1.Height + dataGridView1.Location.Y + 39;
                    int bh = button2.Height + button2.Location.Y + 39;

                    if (dgv > velicina_ekrana.Height)
                    {
                        dgv = velicina_ekrana.Height - (dataGridView1.Location.Y + 39);
                        this.Height = dgv;
                    }
                    else if (dgv > bh)
                        this.Height = dgv;
                    else
                        this.Height = bh;

                    foreach (var item in p)
                    {
                        this.Controls.Remove(item);
                    }
                    foreach (var item in l)
                    {
                        this.Controls.Remove(item);
                    }
                    

                }
                else
                {
                    MessageBox.Show("Horizontala vaseg monitora je mala da bi pokazala ocene. Excel sa ocenama se pokrece");
                    EXCEL();
                }   
            }
        }
        public void EXCEL()
        {
            progressBar1.Show();
            progressBar1.Value = 0;

            //Putanja fajla
            string putanja = "Ocene.xlsx";
            //worksheet.Cells.AutoFit();

            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            app.Visible = true;


            worksheet = workbook.Sheets["Sheet1"];
            worksheet = workbook.ActiveSheet;
            worksheet.Columns.AutoFit();


            for (int i = 1; i < dataGridView1.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = dataGridView1.Columns[i - 1].HeaderText;
            }
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    progressBar1.Value += 1;
                    if (dataGridView1.Rows[i].Cells[j].Value != null)
                    {
                        worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                    }
                    else
                    {
                        worksheet.Cells[i + 2, j + 1] = "";
                    }
                    worksheet.Columns[j + 1].AutoFit();

                }
                //MessageBox.Show(progressBar1.Value.ToString());
            }
            progressBar1.Hide();

            //Ako hoce da se sacuva 
            //workbook.SaveAs(putanja);
        }
        public void UCITAJ()
        {

            dataGridView1.Height = 0;
            h_d = dataGridView1.RowHeadersWidth;

            conn.Open();
            //konekcija za tabelu ucenici
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader2;
            SQLiteCommand sqlite_cmd2;


            //komanda za citanje
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT * FROM Ucenik where ID_nastavnika = {id_nastavnika}";
            
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                //tuple ocena,opis ocene
                List<Tuple<int,string>> o = new List<Tuple<int,string>>();
                

                int index = sqlite_datareader.GetInt16(0);
                string uc = sqlite_datareader.GetString(1);
                conn2.Open();
                //konekcija za tabelu ocene

                //komanda za citanje
                sqlite_cmd2 = conn2.CreateCommand();
                sqlite_cmd2.CommandText = $"SELECT * FROM Ocena Where ID_ucenika = {index}";

                sqlite_datareader2 = sqlite_cmd2.ExecuteReader();
                int srednja = 0;

                //dodaje ucenike u datagridview
                //dataGridView1.Rows.Add(uc);
                dataGridView1.Rows.Add(uc);
                dataGridView1.Height +=h_d;
                
                //cita ocene i broj da li su 5, 4, 3, 2 ili 1 da bi se uradila pita 
                while (sqlite_datareader2.Read())
                {
                    int ocena = sqlite_datareader2.GetInt16(2);
                    string opis = sqlite_datareader2.GetString(4);
                    o.Add(Tuple.Create(ocena,opis));
                    srednja += ocena;
                    if (ocena == 5)
                        petice++;
                    else if (ocena == 4)
                        cetvorke++;
                    else if (ocena == 3)
                        trojke++;
                    else if (ocena == 2)
                        dvojke++;
                    else
                        jedinice++;
                    //izdvaja mesec iz datuma
                    int dt = int.Parse(sqlite_datareader2.GetString(3).Split('-')[1]);
                    //dataGridView1.Rows[n].Cells[dt].Value += ocena.ToString()+" ";
                    dataGridView1.Rows[n].Cells[dt].Value += ocena.ToString()+" ";
                }
                //raspodela ucenika po ekranu
                Raspodela(uc, sqlite_datareader.GetBoolean(5));
                conn2.Close();

                //dodaje ucenika u klasu
                u.Add(new ucenik(uc, o, srednja, sqlite_datareader.GetInt16(0), sqlite_datareader.GetBoolean(5)));
                dataGridView1.Rows[n].Cells["prosek"].Value = u[n].srednja;
                //pomera u desno
                x += a + rw;
                n++;

                //kad ce da prelomi u novi red
                if (n % nw == 0)
                {
                    y += b + rh;
                    x = 100;
                }
                
            }
            conn.Close();
            pita();
            //MessageBox.Show(dataGridView1.RowCount.ToString());
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            //kad se forma 2 resajzuje oda sve kalkulacije da izracuna i da raspodeli ucenike
            //Responziv
            Kalkulacije_broja_ucenika();

            ponovna_raspodela_lab();
            ponovna_raspodela_pic();
            
        }
            

        public void Update_pita()
        {
            //Update pite

            chart1.Series.Clear();
            chart1.Series.Add("s1");
            //chart1.Series["s1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;

            pita();
        }
        public void pita()
        {
            //dodavanje vrsta u piti

            if (petice != 0)
                chart1.Series["s1"].Points.AddXY("5", petice);
            if (cetvorke != 0)
                chart1.Series["s1"].Points.AddXY("4", cetvorke);
            if (trojke != 0)
                chart1.Series["s1"].Points.AddXY("3", trojke);
            if (dvojke != 0)
                chart1.Series["s1"].Points.AddXY("2", dvojke);
            if (jedinice != 0)
                chart1.Series["s1"].Points.AddXY("1", jedinice);
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            f1.Show();
        }

        public void Raspodela(string uc, bool pol)
        {
            //rasporedjuje ucenike

            string odredi_pol;
            //pos
            if (pol)
                odredi_pol = "musko.jpg";
            else
                odredi_pol = "zensko.jpg";


            //dadavanje pictueboxa i labla
            p.Add(new PictureBox
            {
                Name = n.ToString(),
                Size = new Size(a, b),
                Location = new Point(x, y),
                Image = new Bitmap(Image.FromFile(odredi_pol), new Size(a, b)),
                BorderStyle = BorderStyle.Fixed3D
            });
            l.Add(new Label
            {
                Name = n.ToString(),
                Size = new Size(a, rh * 3 / 4),
                Location = new Point(x, y + b),
                Text = uc,
                TextAlign = ContentAlignment.MiddleCenter,
            });


        }

        //ponovna raspodela pictureBoxa i labla
        public void ponovna_raspodela_pic()
        {
            kordinate();
            int i = 0;
            foreach (var pic in p)
            {
                i++;
                pic.Location = new Point(x, y);

                Povecaj_x_y(i);

            }
        }
        public void ponovna_raspodela_lab()
        {
            kordinate();
            int i = 0;
            foreach (var lab in l)
            {
                i++;
                lab.Location = new Point(x, y + b);
                Povecaj_x_y(i);

            }
        }
        //kontlolise x i y u jednoj metodi
        public void Povecaj_x_y(int i)
        {
            x += a + rw;
            if (i % nw == 0)
            {
                y += b + rh;
                x = 100;
            }
        }
        //Default kordnate
        public void kordinate()
        {
            x = 100;

            y = border;
        }
        public void Kalkulacije_broja_ucenika()
        {
            int k = 0;

            if (this.VerticalScroll.Visible == true)
            {
                k = System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
            }
            rw = 15; rh = 40;
            a = 100; b = 120;
            double d = (this.Width - border * 2 - a - k) / (a + rw);
            nw = Math.Floor(d);
            if (nw < 1)
                nw = 1;
            kordinate();

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3();
            f3.index = int.Parse(((PictureBox)sender).Name);
            f3.conn = conn2;
            f3.Show();
            this.Hide();

        }
    }
}
