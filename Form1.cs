using System.IO;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Compilador
{

    /*PENDIENTE POR HACER
     * operadores aritmeticos + - * / %
     * operadores relacionales < > <= >= != =
     * digito decimales. 

    /*CONCEPTOS A ENTENDER 

     .Leer.Read() lee uno por uno en ASCII

     */
    public partial class Form1 : Form
    {
        private int lookahead = -1;
        public Form1()
        {
            InitializeComponent();
            cOMPILARToolStripMenuItem.Enabled = false;
            //Se empieza con la opcion de "Compilar" DESACTIVADO

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            cOMPILARToolStripMenuItem.Enabled = true;
            // habilita la opción compilar cuando se realiza un cambio en el texto.
        }


        //***********OPCIONES DEL MENU "ARCHIVO"***********

        //NUEVO
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
        }

        //ABRIR
        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog VentanaAbrir = new OpenFileDialog();
            VentanaAbrir.Filter = "Texto|*.c";
            if (VentanaAbrir.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaAbrir.FileName;
                using (StreamReader Leer = new StreamReader(archivo))
                {
                    richTextBox1.Text = Leer.ReadToEnd();
                }

                Form1.ActiveForm.Text = "Compilador - " + archivo;
                cOMPILARToolStripMenuItem.Enabled = true;
                //deja compilar hasta que haya un archivo C
            }
        }

        //GUARDAR
        private void gUAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guardar();
        }

        //FUNCIONAMIENTO DE GUARDAR
        private void guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            VentanaGuardar.Filter = "Texto|*.c";
            if (archivo != null)
            {
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }

            else
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter Escribir = new StreamWriter(archivo))
                    {
                        Escribir.Write(richTextBox1.Text);
                    }
                }
            }
            Form1.ActiveForm.Text = "Compilador - " + archivo;
        }

        //GUADAR COMO
        private void GuardarComoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            if (VentanaGuardar.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaGuardar.FileName;
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text);
                }
            }
        }

        //SALIR
        private void salirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //**********FIN DE OPCIONES DEL MENU "ARCHIVO"**********


        //**********OPCION DE ANALIZAR **********

        private void aNALIZARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guardar();
            if (Cabecera())
            { 
            N_error = 0;
            N_linea = 1;
            Rtbx_salida.Clear();
            archivoback = archivo.Remove(archivo.Length - 1) + "back"; //extensión .back    
            archivotrad = archivo.Remove(archivo.Length - 1) + "trad"; //extensión .trad

            Escribir = new StreamWriter(archivoback);
            StreamWriter EscribirTrad = new StreamWriter(archivotrad);
            Leer = new StreamReader(archivo);

            string palabra = "";


            do
            {
                if (lookahead != -1)
                {
                    i_caracter = lookahead;
                    lookahead = -1;
                }
                else
                {
                    i_caracter = Leer.Read();
                }

                if (i_caracter == -1)
                {
                    break; // fin de archivo
                }

                char tipo = Tipo_caracter(i_caracter);

                if (tipo == 'l') // palabra (se devolvio una letra) 
                {
                    palabra = "";
                    while (char.IsLetterOrDigit((char)i_caracter) || i_caracter == '_')
                    {
                        palabra += (char)i_caracter;
                        i_caracter = Leer.Read();
                    }
                    lookahead = i_caracter;

                    int idxTipo = tipos.IndexOf(palabra);
                    if (idxTipo != -1) // es tipo de dato
                    {
                        Escribir.WriteLine("tipo: " + palabra);
                        EscribirTrad.Write(tipos_traduccion[idxTipo] + " ");
                    }
                    else
                    {
                        int idx = reservadas.IndexOf(palabra);
                        if (idx != -1) // es palabra reservada
                        {
                            Escribir.WriteLine("palabra reservada: " + palabra);
                            EscribirTrad.Write(traducciones[idx] + " ");
                        }
                        else // identificador
                        {
                            Escribir.WriteLine("identificador: " + palabra);
                            EscribirTrad.Write(palabra + " ");
                        }
                    }
                }
                else if (tipo == 'd') // dígito
                {
                    Escribir.WriteLine("digito: " + (char)i_caracter);
                    EscribirTrad.Write((char)i_caracter);
                }
                else if (tipo == 's') // símbolo
                {
                    Escribir.WriteLine("simbolo: " + (char)i_caracter);
                    EscribirTrad.Write((char)i_caracter);
                }
                else if (tipo == 'n') // salto de línea
                {
                    Escribir.WriteLine("salto de linea");
                    EscribirTrad.Write("\n");
                    N_linea++;
                }
                else if (tipo == 'x') //Cadena
                {
                    bool cerrada;
                    string contenidoCadena = Cadena(out cerrada);
                    Escribir.WriteLine("cadena:\" " + contenidoCadena + "\"");

                    if (cerrada) //verifica que si se haya cerrado la cadena
                        EscribirTrad.Write("\"" + contenidoCadena + "\"");
                    else
                        EscribirTrad.Write("\"" + contenidoCadena); // no la cierra en el .trad

                }


                else if (tipo == 'j')
                {
                    int siguiente = Leer.Read();
                    if (siguiente == '/') // comentario de línea
                    {
                        string comentario = "";
                        while (siguiente != '\n' && siguiente != -1)
                        {
                            comentario += (char)siguiente;
                            siguiente = Leer.Read();
                        }
                        Escribir.WriteLine("comentario: " + comentario);
                        EscribirTrad.Write("/" + comentario + "");
                    }
                    else if (siguiente == '*')
                    {
                        string comentario = "";
                        int anterior = 0; //APUNTADOR PARA GUARDAR EL CARACTER ANTERIOR
                        siguiente = Leer.Read();

                        while (!(anterior == '*' && siguiente == '/') && siguiente != -1)
                        {
                            comentario += (char)siguiente;
                            anterior = siguiente;
                            siguiente = Leer.Read();
                        }
                        Escribir.WriteLine("comentario bloque: " + comentario);
                        EscribirTrad.WriteLine("/*" + comentario + "*/");
                    }

                    else
                    {
                        // no era comentario, solo era '/'
                        Escribir.WriteLine("simbolo: /");
                        EscribirTrad.Write("/");
                        i_caracter = siguiente; // regresamos el último caracter leído al flujo
                    }
                }

                else if (tipo == 'g') // simbolo !
                {
                    Escribir.WriteLine("operador logico: " + (char)i_caracter);
                    EscribirTrad.Write((char)i_caracter);

                }

                else if (tipo == 'h') // &
                {
                    int siguiente = Leer.Read();
                    if (siguiente == '&')
                    {
                        Escribir.WriteLine("operador logico: &&");
                        EscribirTrad.Write("&&");
                    }
                    else
                    {
                        Escribir.WriteLine("simbolo: &");
                        EscribirTrad.Write("&");
                        i_caracter = siguiente; // devolvemos el caracter que se adelantó
                    }
                }

                else if (tipo == 'k') // |
                {
                    int siguiente = Leer.Read();
                    if (siguiente == '|')
                    {
                        Escribir.WriteLine("operador logico: ||");
                        EscribirTrad.Write("||");
                    }
                    else
                    {
                        Escribir.WriteLine("simbolo: |");
                        EscribirTrad.Write("|");
                        i_caracter = siguiente; // devolvemos el caracter que se adelantó
                    }
                }


                else if (i_caracter != -1) // cualquier otro carácter
                {
                    Escribir.WriteLine("otro: " + (char)i_caracter);
                    EscribirTrad.Write((char)i_caracter);
                }
                else if (tipo == 'z')
                {
                    // no hacer nada, fin de archivo
                }

//------------------------------------------CABECERA--------------------

            } while (i_caracter != -1);

            Rtbx_salida.AppendText("Errores: " + N_error);
            Escribir.Close();
            EscribirTrad.Close();
            Leer.Close();
            }
            bool Cabecera()
            {
                Leer = new StreamReader(archivo);
                int i_cabecera = Leer.Read();

                if (i_cabecera == 35)
                {
                    directiva_procesador();
                    return true;
                }
                else
                {
                    Error(i_cabecera);
                    return false;
                }
                    /*switch (i_cabecera)
                    {
                        case 35:
                            directiva_procesador();
                            break;
                        default: Error(i_cabecera);
                            break;
                    }
                    return false;*/
                }

            void directiva_procesador()
            {
                MessageBox.Show("Directiva de preprocesador");
            }
        }

        private void cOMPILARToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private char Tipo_caracter(int caracter)
        {
            //ASCII A-Z y de a-z
            if ((caracter >= 65 && caracter <= 90) || (caracter >= 97 && caracter <= 122))
            {
                return 'l'; // Letra
            }

            else if (caracter >= 48 && caracter <= 57)
            {
                return 'd'; // Dígito
            }

            else if (caracter >= 60 && caracter <= 62)
            {
                return 'i'; // Operador relacional
            }

            if (caracter == -1)
                return 'z'; // marca fin de archivo
            else
            {
                switch (caracter)
                {
                    case 10: return 'n'; // ASCII salto de linea 10
                    case 34: return 'x'; // ASCII inicio de cadena 34
                    case 39: return 'c'; // ASCII de inicio de caracter 39
                    case 47: return 'j'; // ASCII de barra 47
                    case 32: return 'b'; // espacio
                    case 9: return 'b';  // tab
                    //programar para los casos que sean simbolos y regresar 's'
                    case 33: return 'g'; // ASCII de !
                    case 38: return 'h'; // ASCCI DE & posible &&
                    case 124: return 'k'; // ASCII DE |  posible ||
                    default: return 's'; //si no es de los casos anteriores es error

                }
            }



        }
        private string Cadena(out bool cerrada)
        {
            string contenido = "";
            cerrada = false;
            do
            {
                i_caracter = Leer.Read();
                if (i_caracter == 10) N_linea++; //ASCI 10 = SALTO DE LINEA
                if (i_caracter != 34 && i_caracter != -1) // ASCII fin de cadena 34 y fin de archivo -1
                {
                    contenido += (char)i_caracter; // acumula el carácter (se va formando la cadena)
                }
            } while (i_caracter != 34 && i_caracter != -1);

            if (i_caracter == 34)
            {
                cerrada = true; // sí se cerró correctamente
            }
            else
            {
                Error(-1); // cadena sin cerrar
            }

            return contenido;
        }
        private void Error(int i_caracter)
        {
            Rtbx_salida.AppendText("Error léxico " + i_caracter + ", línea " + N_linea + "\n");
            N_error++;
        }


        // Lista de palabras reservadas en C
        List<string> reservadas = new List<string>
        {
            "auto","break","case","const","continue","default","do",
            "else","enum","extern","for","goto","if","inline","int","long",
            "register","restrict","return","short","signed","sizeof","static","struct",
            "switch","typedef","union","unsigned","volatile","while",
            "_Alignas","_Alignof","_Atomic","_Bool","_Complex","_Generic","_Imaginary",
            "_Noreturn","_Static_assert","_Thread_local","asm","catch","class",
            "const_cast","delete","dynamic_cast","explicit","export","false","friend",
            "mutable","namespace","new","operator","private","protected","public",
            "reinterpret_cast","static_cast","template","this","throw","true","try",
            "typeid","typename","using","virtual","wchar_t","main","include","define","printf"
        };

        // Traducciones en español (mismo orden que la lista de arriba)
        List<string> traducciones = new List<string>
        {
            "auto","romper","caso","constante","continuar","defecto","hacer",
            "sino","enumeración","externo","para","ir","si","en línea","entero","largo",
            "registro","restringido","retornar","corto","con signo","tamaño de","estático","estructura",
            "selección","definir tipo","unión","sin signo","volátil","mientras",
            "alinear como","alineación de","atómico","booleano","complejo","genérico","imaginario",
            "sin retorno","afirmación estática","hilo local","ensamblador","capturar","clase",
            "conversión constante","eliminar","conversión dinámica","explícito","exportar","falso","amigo",
            "mutable","espacio de nombres","nuevo","operador","privado","protegido","público",
            "conversión reinterpretada","conversión estática","plantilla","este","lanzar","verdadero","intentar",
            "tipoid","nombre de tipo","usando","virtual","carácter ancho","principal","incluir","definir","imprimir"
        };

        List<string> tipos = new List<string>
        {
            "int","float","double","char","void","bool"
        };


        List<string> tipos_traduccion = new List<string>
        {
            "int","float","double","char","void","bool"
        };





    }
}
