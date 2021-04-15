﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
// Librerías a utilizar
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Mi_mercadito
{
    public partial class FrmMain : Form
    {
        public static string IdMarca, IdDepartamento, IdSuc;
        public FrmMain(System.Drawing.Image i, string[] Consulta)
        {
            InitializeComponent();
            // Se asigna el valor de la imagen de la foto a el picturebox de FrmMain.
            pbox_Camara.Image = i;
            // Mensaje de saludo.
            switch (Consulta[4])
            {
                case "M":
                    lblName.Text = "Bienvenido " + Consulta[1];
                    break;
                case "F":
                    lblName.Text = "Bienvenida " + Consulta[1];
                    break;
                default:
                    lblName.Text = "Hola " + Consulta[1];
                    break;
            }
        }
        // Creación de método para movimiento de ventana
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void btn_Foto_Click(object sender, EventArgs e)
        {
            // Hace que al dar click en el boton tomar foto se habra el formulario de la camara.
            FrmCámara Camara = new FrmCámara();
            Image Logo = Camara.pbox_Camara.Image;
            this.Enabled = false;
            if (Camara.ShowDialog() == DialogResult.OK)
                this.Show();
            this.Enabled = true; ;
            // Re asignamos en caso de que corresponda un cambio
            if (Logo != pbox_Camara.Image)
            {
                pbox_Camara.Image = Camara.pbox_Camara.Image;
                Camara.pbox_Camara.Image = Logo;
            }
        }

        private void btn_Agregar_Click(object sender, EventArgs e)
        {

                        Marca Marc = new Marca();
                        if (!Marc.ValidarMarc(txtProdMarc.Text))
                        {
                            IdMarca = Marc.ConsultId(txtProdMarc.Text);
                        }
                        else
                        {
                            Marc.InsertarMarc(txtProdMarc.Text, pbox_Camara);
                            IdMarca = Marc.ConsultId(txtProdMarc.Text);
                        }


                        Departamento Departamento = new Departamento();
                        if (!Departamento.ValidarDepart(txtProdDpto.Text))
                        {
                            IdDepartamento = Departamento.ConsultId(txtProdDpto.Text);
                        }
                        else
                        {
                        }    

                     Sucursal Sucursal = new Sucursal();
                    IdSuc = Sucursal.ConsultId(cboxSucursal.Text, txtPaís.Text, txtCiudad.Text, txtDir.Text);


            Producto Productos = new Producto();
            if (Productos.InsertProductos(txtNombreProduc.Text, txtProdPrecio.Text, txtProdContNet.Text, txtProdDesc.Text, pbox_Camara, IdMarca, IdDepartamento,IdSuc) > 0)
            {
                MessageBox.Show("Se ha guardado el producto de manera correcta.", "Producto guardado",MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("No se ha guardado el producto de manera correcta.", "Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
            string Msg = "¿Desea salir?", Title = "Salir de mi lista";
            if (MessageBox.Show(Msg, Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                this.Close();
            else
                this.Show();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Sucursal sucursal = new Sucursal();
            sucursal.ConsultaSuc(cboxSucursal);
            Autocompletar();
            AutoCompletarMarca();
            AutoCompletarDpto();
            // Condicional donde si el picturebox no cuenta con una imagen dentro, mandara a enviar el logo de Mi mercadito
            if (pbox_Camara.Image == null)
            {
                Image logo = Image.FromFile("logomiMercadito.png"); // Toma la imágen de la carpeta bin, dentro de la otra carpeta Debug
                pbox_Camara.Image = logo;
            }
        }

        private void cboxSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] arreglo = new string[3]; // Se crea un arreglo que almacena 4 datos
            Sucursal datos = new Sucursal(); // Se crea el objeto datos en base a la clase Sucursal
            arreglo = datos.Datos(cboxSucursal.Text); // Se llama al método de la clase Sucursal, que tiene como parámetro el cboxSucursal
            txtProdSuc.Text = cboxSucursal.Text; // Iguala los datos que se implemnten en las sucursales tanto en el textbox como el combobox
            txtPaís.Text = arreglo[1];
            txtCiudad.Text = arreglo[2];
            txtDir.Text = arreglo[3];
        }

        public void Autocompletar() // Método autocompletar txtNombreProduc
        {
            string Consulta = (@"SELECT * FROM Producto ;"); // Se consulta la BD en la tabla Producto
            using (SqlConnection Conn = ConnectionDB.StartConn())
            {
                DataTable Datos = new DataTable(); // Se obtuenen los datos de la tabla
                AutoCompleteStringCollection lista = new AutoCompleteStringCollection(); // Se crea  el objeto en base a la clase que general el autocompletar
                SqlDataAdapter adaptador = new SqlDataAdapter(Consulta, Conn);
                adaptador.Fill(Datos); // Rellena los datos en base a los que se encuentran en la tabla Producto de la BD
                // Ciclo for donde va leyendo fila por fila los datos que se relacionan con la tabla Producto
                for (int i = 0; i < Datos.Rows.Count; i++)
                {
                    lista.Add(Datos.Rows[i]["prodName"].ToString()); // Se añaden los datos de la columna "prodName" y los pasa a tipo string
                }
                txtNombreProduc.AutoCompleteCustomSource = lista; // Se autocompleta el textbox aplicando la propiedad autocomplete realizando la lectura de la lista
            }
        }

        public void AutoCompletarMarca() // Método autocompletar txtProdMarc
        {
            string Consulta = (@"SELECT * FROM Marca ;");
            using (SqlConnection Conn = ConnectionDB.StartConn())
            {
                DataTable Datos = new DataTable();
                AutoCompleteStringCollection lista = new AutoCompleteStringCollection();
                SqlDataAdapter adaptador = new SqlDataAdapter(Consulta, Conn);
                adaptador.Fill(Datos);

                for (int i = 0; i < Datos.Rows.Count; i++)
                {
                    lista.Add(Datos.Rows[i]["marcName"].ToString());
                }
                txtProdMarc.AutoCompleteCustomSource = lista;
            }
        }

        public void AutoCompletarDpto() // Método autocompletar txtProdDpto
        {
            string Consulta = (@"SELECT * FROM Departamento ;");
            using (SqlConnection Conn = ConnectionDB.StartConn())
            {
                DataTable Datos = new DataTable();
                AutoCompleteStringCollection lista = new AutoCompleteStringCollection();
                SqlDataAdapter adaptador = new SqlDataAdapter(Consulta, Conn);
                adaptador.Fill(Datos);

                for (int i = 0; i < Datos.Rows.Count; i++)
                {
                    lista.Add(Datos.Rows[i]["dptoName"].ToString());
                }
                txtProdDpto.AutoCompleteCustomSource = lista;
            }
        }

        // Movimiento de pantalla
        private void pnlLogin_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // Cerrar ventana y programa
        private void pbxX2_Click(object sender, EventArgs e)
        {
            // MessageBox implementación de código 
            string Msg = "¿Desea salir?", Title = "Cancelar";
            if (MessageBox.Show(Msg, Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close();
                Application.Exit();
            }
        }

        // Efectos del boton X
        private void pbxX_MouseHover(object sender, EventArgs e)
        {
            pbxX2.BackColor = Color.White;
            pbxX2.Visible = true;
        }

        private void pbxX_MouseLeave(object sender, EventArgs e)
        {
            pbxX.BackColor = Color.PaleTurquoise;
        }
        private void pbxX2_MouseLeave(object sender, EventArgs e)
        {
            pbxX2.Visible = false;
        }

        // Minimizar ventana
        private void pbxmin2_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }
        // Efectos del boton min
        private void pbxmin_MouseHover(object sender, EventArgs e)
        {
            pbxmin2.BackColor = Color.White;
            pbxmin2.Visible = true;
        }

        private void pbxmin_MouseLeave(object sender, EventArgs e)
        {
            pbxmin.BackColor = Color.PaleTurquoise;
        }

        private void pbxmin2_MouseLeave(object sender, EventArgs e)
        {
            pbxmin2.Visible = false;
        }

        // Cerrar ventana
        private void iniciarSesiónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Msg = "¿Desea cerrar sesión?", Title = "Cancelar";
            if (MessageBox.Show(Msg, Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                this.Close();
        }

        private void cmdCargarImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog cargarImg = new OpenFileDialog();
            cargarImg.Filter = "Todas las imágenes soportadas|*.Jpeg;*.JPG;*.png;*.bmp;*.ico";
            cargarImg.InitialDirectory = System.IO.Path.GetTempPath();
            if (cargarImg.ShowDialog() == DialogResult.OK) pbox_Camara.Load(cargarImg.FileName);
        }

        private void txtNombreProduc_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Tab)
            {
                //string[] arreglo = new string[7];
                //Producto datos = new Producto();
                //arreglo = datos.RellenarProduc(txtNombreProduc.Text);
                //txtProdPrecio.Text = arreglo[1];
                //txtProdContNet.Text = arreglo[2];
                //txtProdDesc.Text = arreglo[3];
                //txtProdMarc.Text = arreglo[4];
                //txtProdDpto.Text = arreglo[5];
                //txtProdSuc.Text = arreglo[6];
                //// Muestra la imágen del producto que esta almacenada en la base de datos
                //datos.Imagen(ref pbox_Camara, txtNombreProduc.Text);
                //// Hace que al presionar el TAB se modifique el combobox de la sucursal en base al del producto.
                //cboxSucursal.Text = txtProdSuc.Text;

                //if (txtNombreProduc.Text == "" || txtNombreProduc.Text == null)
                {
                    // MessageBox que obliga al usuario registrar un producto al usuario para que no rompa el programa al querer usar el TAB
                    //MessageBox.Show("Ingrese primero un producto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
                //else
                {
                    //txtProdPrecio.Focus();
                }
            }
        }

        private void txtNombreProduc_Leave(object sender, EventArgs e)
        {
            try
            {
                string[] arreglo = new string[7];
                Producto datos = new Producto();
                arreglo = datos.RellenarProduc(txtNombreProduc.Text);
                txtProdPrecio.Text = arreglo[1];
                txtProdContNet.Text = arreglo[2];
                txtProdDesc.Text = arreglo[3];
                txtProdMarc.Text = arreglo[4];
                txtProdDpto.Text = arreglo[5];
                txtProdSuc.Text = arreglo[6];
                datos.Imagen(ref pbox_Camara, txtNombreProduc.Text); // Muestra la imágen del producto que esta almacenada en la base de datos
                cboxSucursal.Text = txtProdSuc.Text; // Hace que al presionar el TAB se modifique el combobox de la sucursal en base al del producto.
            }
            catch (Exception)
            {
                //MessageBox.Show("Ingrese primero un producto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
        }

        // Función Bloqueo() que me indica cuando puedo modificar ciertos textbox dependiendo de los datos introducidos por el usuario en el txtNombreProduc
        public bool Bloqueo(string txtblockmarc) 
        {
            string Consulta = @"SELECT * FROM Producto WHERE prodName = @prodName;";
            using (SqlConnection Conn = ConnectionDB.StartConn())
            {
                SqlCommand cmd = new SqlCommand(Consulta, Conn);
                cmd.Parameters.AddWithValue("@prodName", txtblockmarc);
                int Count = Convert.ToInt32(cmd.ExecuteScalar());
                return Count == 0;
            }
        }

        private void txtNombreProduc_TextChanged(object sender, EventArgs e)
        {
            if (!Bloqueo(txtNombreProduc.Text))
            {
                txtProdMarc.ReadOnly = true;
                txtProdDpto.ReadOnly = true;
                txtProdDesc.ReadOnly = true;
            }
            else
            {
                txtProdMarc.ReadOnly = false;
                txtProdDpto.ReadOnly = false;
                txtProdDesc.ReadOnly = false;
                Image logo = Image.FromFile("logomiMercadito.png"); // Toma la imágen de la carpeta bin, dentro de la otra carpeta Debug
                pbox_Camara.Image = logo;
            }
        }
    }
}
