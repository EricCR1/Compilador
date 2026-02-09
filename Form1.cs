using System.IO;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Compilador
{
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


        private List<string> libreria = new List<string>
{
    // Librerías estándar de C más comunes
    "math.h",       // Funciones matemáticas
    "time.h",       // Funciones de tiempo
    "ctype.h",      // Clasificación de caracteres

};

        private string ExtraerNombreLibreria(string linea)
        {
            try
            {
                // Eliminar #include y espacios
                string resto = linea.Substring(8).Trim(); // 8 = longitud de "#include"

                // Buscar librería entre < >
                int inicioAngular = resto.IndexOf('<');
                int finAngular = resto.IndexOf('>');

                if (inicioAngular != -1 && finAngular != -1 && finAngular > inicioAngular)
                {
                    return resto.Substring(inicioAngular + 1, finAngular - inicioAngular - 1).Trim();
                }

                // Buscar librería entre " "
                int inicioComilla = resto.IndexOf('"');
                int finComilla = resto.LastIndexOf('"');

                if (inicioComilla != -1 && finComilla != -1 && finComilla > inicioComilla)
                {
                    return resto.Substring(inicioComilla + 1, finComilla - inicioComilla - 1).Trim();
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        private void aNALIZARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guardar();
            archivoback = archivo.Remove(archivo.Length - 1) + "back";
            archivotrad = archivo.Remove(archivo.Length - 1) + "trad";
            N_error = 0;
            N_linea = 1;
            Rtbx_salida.Clear();

            // ====== VERIFICACIÓN DE CABECERA ======

            // Buscar la primera línea que no esté vacía
            string primeraLineaReal = "";
            foreach (var linea in File.ReadLines(archivo))
            {
                string lineaTrim = linea.Trim();
                if (!string.IsNullOrWhiteSpace(lineaTrim))
                {
                    primeraLineaReal = lineaTrim;
                    break;
                }
            }

            // PASO 1: Verificar que empiece con #include
            if (!primeraLineaReal.StartsWith("#include"))
            {
                Rtbx_salida.AppendText("Error: La primera línea no vacía debe ser una directiva #include\n");
                Rtbx_salida.AppendText("Formato esperado: #include <libreria.h>\n");
                Error(-1);
                return;
            }

            // PASO 2: Extraer el nombre de la librería
            string nombreLibreria = ExtraerNombreLibreria(primeraLineaReal);

            if (string.IsNullOrEmpty(nombreLibreria))
            {
                Rtbx_salida.AppendText("Error: No se pudo extraer el nombre de la librería\n");
                Rtbx_salida.AppendText("Formato esperado: #include <libreria.h>\n");
                Error(-1);
                return;
            }

            // PASO 3: Verificar que la librería esté en la lista (OPCIONAL - puedes quitar esto si quieres aceptar cualquiera)
            if (!libreria.Contains(nombreLibreria))
            {
                Rtbx_salida.AppendText($"Advertencia: La librería '{nombreLibreria}' no está en la lista de librerías conocidas.\n");
                Rtbx_salida.AppendText("Se procesará de todos modos.\n");
            }

            // Si llegamos aquí, la cabecera es válida
            Rtbx_salida.AppendText($"Cabecera detectada correctamente: {nombreLibreria}\n");

            Escribir = new StreamWriter(archivoback);
            StreamWriter EscribirTrad = new StreamWriter(archivotrad);
            Leer = new StreamReader(archivo);

            // Escribimos los tokens separados manualmente
            Escribir.WriteLine("#");
            Escribir.WriteLine("include");
            Escribir.WriteLine("<");
            Escribir.WriteLine("libreria");
            Escribir.WriteLine(">");
            EscribirTrad.Write($"#include <{nombreLibreria}>\n");


            // Saltamos la primera línea para que no se analice de nuevo
            Leer.ReadLine();

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
                        Escribir.WriteLine(palabra);
                        EscribirTrad.Write(tipos_traduccion[idxTipo] + " ");
                    }
                    else
                    {
                        int idx = reservadas.IndexOf(palabra);
                        if (idx != -1) // es palabra reservada
                        {
                            Escribir.WriteLine(palabra);
                            EscribirTrad.Write(traducciones[idx] + " ");
                        }
                        else // identificador
                        {
                            Escribir.WriteLine("identificador");
                            EscribirTrad.Write(palabra + " ");
                        }
                    }
                }
                else if (tipo == 'd') // dígito
                {
                    string numero = "";
                    // El carácter actual está en i_caracter
                    while (i_caracter != -1 && i_caracter >= 48 && i_caracter <= 57) // 0-9
                    {
                        numero += (char)i_caracter;
                        i_caracter = Leer.Read();
                    }

                    // Dejamos el último char leido (no-dígito) en lookahead para que el bucle principal lo procese
                    lookahead = i_caracter;

                    // Escribimos un SOLO token "Numero" en el .back y el valor en el .trad
                    Escribir.WriteLine("Numero");
                    EscribirTrad.Write(numero + " ");
                }
                else if (tipo == 's') // símbolo
                {
                    Escribir.WriteLine((char)i_caracter);
                    EscribirTrad.Write((char)i_caracter);
                }
                else if (tipo == 'n') // salto de línea
                {
                    Escribir.WriteLine("LF");
                    EscribirTrad.Write("\n");
                    N_linea++;
                }
                else if (tipo == 'x') //Cadena
                {
                    bool cerrada;
                    string contenidoCadena = Cadena(out cerrada);
                    Escribir.WriteLine("cadena");

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

                else if (tipo == 'b')
                {
                    // Espacios y tabs - simplemente los escribimos en el .trad pero no en .back
                    EscribirTrad.Write((char)i_caracter);
                    // No escribimos nada en .back para espacios
                }

                else if (i_caracter != -1) // cualquier otro carácter
                {
                    Escribir.WriteLine((char)i_caracter);
                    EscribirTrad.Write((char)i_caracter);
                }
                else if (tipo == 'z')
                {
                    // no hacer nada, fin de archivo
                }


            } while (i_caracter != -1);

            Rtbx_salida.AppendText("Errores: " + N_error);
            Escribir.Close();
            EscribirTrad.Close();
            Leer.Close();

            //FIN DE ANALISIS LEXICO

            Rtbx_salida.AppendText("\n--- Análisis léxico finalizado. Iniciando análisis sintáctico ---\n");

            LeerBack = new StreamReader(archivoback);
            finArchivo = false;
            SiguienteToken(); // leer primer token

            try
            {
                Declaracion();
                if (!finArchivo)
                    Rtbx_salida.AppendText("Análisis sintáctico completado correctamente.\n");
            }
            catch (Exception ex)
            {
                Rtbx_salida.AppendText("Error sintáctico: " + ex.Message + "\n");
            }

            LeerBack.Close();
        }
        private void SiguienteToken()
        {
            do
            {
                if (LeerBack == null || LeerBack.EndOfStream)
                {
                    finArchivo = true;
                    token = "EOF";
                    return;
                }
                else
                {
                    token = LeerBack.ReadLine()?.Trim() ?? "EOF";
                }
            } while (string.IsNullOrEmpty(token)); // Ignorar líneas vacías
        }
        private void Declaracion()
        {
            while (!finArchivo && token != "EOF")
            {
                // Cambiar de "tipo de dato" a verificar los tipos específicos
                if (token == "int" || token == "float" || token == "double" ||
                    token == "char" || token == "void" || token == "bool")
                {

                    SiguienteToken();
                    VariableGlobal();
                }

                else if (token == "If" || token == "Switch" || token == "For" || token == "do" || token == "While")
                {

                    SiguienteToken();
                    EstructuraControl();
                }

                else
                {
                    // Si no es tipo de dato, pasamos al siguiente token
                    SiguienteToken();
                }
            }
        }

        private void VariableGlobal()
        {
            // Ahora puede ser "identificador" O "main" (porque main está en palabras reservadas)
            if (token != "identificador" && token != "main")
                throw new Exception("Se esperaba un identificador.");

            // Guardar el nombre del identificador
            string nombreIdentificador = token;
            SiguienteToken();

            // Verificar si es la función main
            if (nombreIdentificador == "main" && token == "(")
            {
                Rtbx_salida.AppendText("Se detectó main\n");
                FuncionMain();
                return;
            }

            // Si no es main, continuar con variable global normal
            if (token == "=")
            {
                SiguienteToken();
                Constante();
            }
            else if (token == "[")
            {
                SiguienteToken();
                Arreglo();
                // Después de Arreglo(), el token actual debería ser ';'
            }
            if (token == "(")
            {
                Rtbx_salida.AppendText("Funcion encontrada");
                SiguienteToken();
                EstructuraFuncion();
            }

            // Esta verificación se hace para todos los casos
            if (token == ";")
            {
                Rtbx_salida.AppendText("Declaración de variable global correcta.\n");
                SiguienteToken();
            }
            //else
            //{
            //    throw new Exception("Falta ';' al final de la declaración.");
            //}
        }

        private void FuncionMain()
        {
            if (token != "(")
                throw new Exception("Se esperaba '(' después de main.");

            SiguienteToken();

            if (token != ")")
                throw new Exception("Se esperaba ')' en main.");

            SiguienteToken();

            CuerpoFuncion();

            Rtbx_salida.AppendText("Función main analizada correctamente.\n");
        }

        private void EstructuraFuncion()
        {
            // 1. Procesar parámetros en un bucle (no recursivo)
            while (token != ")" && !finArchivo)
            {
                if (token == "int" || token == "float" || token == "double" ||
                    token == "char" || token == "void" || token == "bool")
                {
                    SiguienteToken(); // Consume el tipo
                    if (token != "identificador")
                        throw new Exception("Se esperaba un identificador como parámetro.");

                    SiguienteToken(); // Consume el identificador

                    if (token == ",")
                    {
                        SiguienteToken(); // Consume la coma y sigue el bucle
                        continue;
                    }
                }
                else if (token == "LF")
                {
                    SiguienteToken();
                }
                else if (token != ")")
                {
                    throw new Exception("Se esperaba tipo de dato o cierre de paréntesis.");
                }
            }

            // 2. Validar cierre de paréntesis
            if (token != ")")
                throw new Exception("Se esperaba ')' al final de los parámetros.");

            Rtbx_salida.AppendText("Cierre correcto de parámetros\n");
            SiguienteToken(); // Consume el ')'

            // 3. Procesar el cuerpo de la función (una sola vez)
            CuerpoFuncion();

            // NOTA: Se eliminó la llamada a Declaracion() aquí.
            // El flujo regresará al loop de Declaracion() original.
        }
        private void CuerpoFuncion()
        {
            while (token == "LF" && !finArchivo)
            {
                SiguienteToken();
            }
            if (token != "{")
                throw new Exception("Se esperaba '{'. es token es " + token);

            SiguienteToken();

            while (token != "}" && !finArchivo && token != "EOF")
            
            {
                if (token == "if")
                {
                    CondicionalSimple();
                }
                else if (token == "switch")
                {
                    SeleccionMultiple();
                }
                else if (token == "for")
                {
                    CicloPara();
                }
                else if (token == "while")
                {
                    CicloMientras();
                }
                else if (token == "do")
                {
                    CicloRepetirMientras();
                }
                else if (token == "return")
                {
                    SentenciaReturn();
                }
                else if (token == "printf" || token == "identificador")
                {
                    Sentencia();
                }
                else if (token == "{")
                {
                    CuerpoFuncion();
                }
                else if (token == "break" || token == "continue")
                {
                    SiguienteToken();
                    if (token != ";")
                        throw new Exception("Se esperaba ';'.");
                    SiguienteToken();
                }
                else
                {
                    SiguienteToken();
                    
                }
            }

            if (token != "}")
                throw new Exception("Se esperaba '}'.");

            SiguienteToken();
        }

        private void CondicionalSimple()
        {
            Rtbx_salida.AppendText("Analizando if...\n");

            SiguienteToken(); // consume 'if'

            if (token != "(")
                throw new Exception("Se esperaba '(' después de if.");
            SiguienteToken();

            // Aquí buscamos "condicion" como identificador
            if (token != "identificador")
                throw new Exception("Se esperaba condición en if.");

            SiguienteToken();

            if (token != ")")
                throw new Exception("Se esperaba ')' después de la condición.");
            SiguienteToken();

            // Cuerpo del if
            if (token == "{")
            {
                CuerpoFuncion();
            }
            else
            {
                // Sentencia simple
                Sentencia();
            }

            // Verificar else
            if (token == "else")
            {
                Rtbx_salida.AppendText("Detectado else (condicional compuesta)...\n");
                SiguienteToken();

                if (token == "{")
                {
                    CuerpoFuncion();
                }
                else
                {
                    Sentencia();
                }
            }

            Rtbx_salida.AppendText("Condicional analizada correctamente.\n");
        }


        // Sentencia genérica
        private void Sentencia()
        {
            string id = token;
            SiguienteToken();

            if (token == "(")
            {
                // Llamada a función
                LlamadaFuncion();
                if (token != ";")
                    throw new Exception("Se esperaba ';'.");
                SiguienteToken();
            }
            else if (token == "=")
            {
                // Asignación
                SiguienteToken();
                if (token == "Numero" || token == "cadena" || token == "identificador")
                {
                    SiguienteToken();
                }
                if (token != ";")
                    throw new Exception("Se esperaba ';'.");
                SiguienteToken();
            }
            else if (token == "[")
            {
                // Asignación a arreglo
                SiguienteToken();
                if (token == "Numero" || token == "identificador")
                {
                    SiguienteToken();
                }
                if (token != "]")
                    throw new Exception("Se esperaba ']'.");
                SiguienteToken();
                if (token == "=")
                {
                    SiguienteToken();
                    if (token == "Numero" || token == "cadena" || token == "identificador")
                    {
                        SiguienteToken();
                    }
                }
                if (token != ";")
                    throw new Exception("Se esperaba ';'.");
                SiguienteToken();
            }
        }

        private void LlamadaFuncion()
        {
            SiguienteToken(); // consume '('

            // Argumentos
            if (token != ")")
            {
                if (token == "Numero" || token == "cadena" || token == "identificador")
                {
                    SiguienteToken();
                }

                while (token == ",")
                {
                    SiguienteToken();
                    if (token == "Numero" || token == "cadena" || token == "identificador")
                    {
                        SiguienteToken();
                    }
                }
            }

            if (token != ")")
                throw new Exception("Se esperaba ')' al final de la llamada.");
            SiguienteToken();
        }
        private void Constante()
        {
            if (token == "Numero" || token == "cadena" || token == "identificador")
            {
                SiguienteToken();
            }
            else
            {
                throw new Exception("Se esperaba una constante o identificador en la asignación.");
            }
        }

        private void Arreglo()
        {
            if (token != "Numero")
                throw new Exception("Se esperaba un número dentro de los corchetes del arreglo.");

            SiguienteToken();

            if (token != "]")
                throw new Exception("Falta ']' en la declaración del arreglo.");

            SiguienteToken();  // Consume el ']'

            // Verificar si hay inicialización después del arreglo
            if (token == "=")
            {
                SiguienteToken();
                InicializacionArreglo();
            }
            else if (token == "[")
            {
                // Arreglo multidimensional
                SiguienteToken();
                Arreglo(); // Llamada recursiva para manejar más dimensiones
            }

            // NO llamar SiguienteToken() aquí - dejar que VariableGlobal() maneje el ';'
        }

        private void InicializacionArreglo()
        {
            // Debe empezar con {
            if (token != "{")
                throw new Exception("Se esperaba '{' para inicialización del arreglo.");

            SiguienteToken();

            bool primerElemento = true;
            bool esperaElemento = true;

            while (token != "}" && !finArchivo && token != "EOF")
            {
                if (!esperaElemento)
                {
                    // Debe haber una coma entre elementos
                    if (token != ",")
                        throw new Exception("Falta ',' entre elementos del arreglo.");

                    SiguienteToken();
                    esperaElemento = true;
                }

                if (esperaElemento)
                {
                    // Puede ser un número, cadena, o otro arreglo
                    if (token == "Numero" || token == "cadena" || token == "identificador")
                    {
                        SiguienteToken();
                        esperaElemento = false;
                        primerElemento = false;
                    }
                    else if (token == "{")
                    {
                        // Arreglo multidimensional - procesar sub-arreglo
                        InicializacionArreglo();
                        esperaElemento = false;
                        primerElemento = false;
                    }
                    else
                    {
                        throw new Exception("Se esperaba un elemento válido para el arreglo.");
                    }
                }
            }

            // Verificar que termine con }
            if (token != "}")
                throw new Exception("Falta '}' al final de la inicialización del arreglo.");

            SiguienteToken(); // Consumir el }
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
                if (i_caracter == 10) N_linea++; //ASCII 10 = SALTO DE LINEA
                if (i_caracter != 34 && i_caracter != -1) // ASCII fin de cadena 34 y fin de archivo -1
                {
                    contenido += (char)i_caracter; // acumula el carácter
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

            // IMPORTANTE: Resetear lookahead después de procesar cadena
            lookahead = -1;

            return contenido;
        }
        private void Error(int i_caracter)
        {
            Rtbx_salida.AppendText("Error léxico " + i_caracter + ", línea " + N_linea + "\n");
            N_error++;
        }


        private void EstructuraControl()
        {
            //if (token == "If" || token == "Switch" || token == "For" || token == "do" || token == "While")
            //{
            Rtbx_salida.AppendText("Se detecto estructura de control\n");
            //    //SiguienteToken();
            //}
            //else
            //{
            //    throw new Exception("Se esperaba una constante o identificador en la asignación.");
            //}
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

        private void SeleccionMultiple()
        {
            Rtbx_salida.AppendText("Analizando switch...\n");

            SiguienteToken(); // consume 'switch'

            if (token != "(")
                throw new Exception("Se esperaba '(' después de switch.");
            SiguienteToken();

            if (token != "identificador" && token != "Numero")
                throw new Exception("Se esperaba variable en switch.");
            SiguienteToken();

            if (token != ")")
                throw new Exception("Se esperaba ')' en switch.");
            SiguienteToken();

            if (token != "{")
                throw new Exception("Se esperaba '{' en switch.");
            SiguienteToken();

            // Procesar cases
            while ((token == "case" || token == "default") && !finArchivo)
            {
                if (token == "case")
                {
                    SiguienteToken();

                    if (token != "Numero" && token != "identificador" && token != "cadena")
                        throw new Exception("Se esperaba valor en case.");
                    SiguienteToken();

                    if (token != ":")
                        throw new Exception("Se esperaba ':' después del case.");
                    SiguienteToken();

                    // Sentencias del case
                    while (token != "case" && token != "default" && token != "}" && token != "break" && !finArchivo)
                    {
                        if (token == "printf" || token == "identificador")
                        {
                            Sentencia();
                        }
                        else if (token == "if" || token == "for" || token == "while")
                        {
                            // Permitir estructuras dentro del case
                            if (token == "if") CondicionalSimple();
                            else if (token == "for") CicloPara();
                            else if (token == "while") CicloMientras();
                        }
                        else
                        {
                            SiguienteToken();
                        }
                    }

                    if (token == "break")
                    {
                        SiguienteToken();
                        if (token != ";")
                            throw new Exception("Se esperaba ';' después de break.");
                        SiguienteToken();
                    }
                }
                else if (token == "default")
                {
                    SiguienteToken();

                    if (token != ":")
                        throw new Exception("Se esperaba ':' después de default.");
                    SiguienteToken();

                    // Sentencias del default
                    while (token != "}" && token != "break" && !finArchivo)
                    {
                        if (token == "printf" || token == "identificador")
                        {
                            Sentencia();
                        }
                        else
                        {
                            SiguienteToken();
                        }
                    }

                    if (token == "break")
                    {
                        SiguienteToken();
                        if (token != ";")
                            throw new Exception("Se esperaba ';' después de break.");
                        SiguienteToken();
                    }
                }
            }

            if (token != "}")
                throw new Exception("Se esperaba '}' al final del switch.");
            SiguienteToken();

            Rtbx_salida.AppendText("Switch analizado correctamente.\n");
        }

        // 3. CICLO FOR
        private void CicloPara()
        {
            Rtbx_salida.AppendText("Analizando for...\n");

            SiguienteToken(); // consume 'for'

            if (token != "(")
                throw new Exception("Se esperaba '(' después de for.");
            SiguienteToken();

            // Inicialización (puede incluir tipo de dato)
            if (token == "int" || token == "float" || token == "double")
            {
                SiguienteToken();
            }

            if (token == "identificador")
            {
                SiguienteToken();
                if (token == "=")
                {
                    SiguienteToken();
                    if (token == "Numero" || token == "identificador")
                    {
                        SiguienteToken();
                    }
                }
            }

            if (token != ";")
                throw new Exception("Se esperaba ';' después de la inicialización.");
            SiguienteToken();

            // Condición
            if (token != ";")
            {
                // Simplificado: esperar identificador
                if (token == "identificador" || token == "Numero")
                {
                    SiguienteToken();
                }
                // Operador
                if (token == "<" || token == ">" || token == "=" || token == "!")
                {
                    SiguienteToken();
                    if (token == "=") SiguienteToken();
                }
                if (token == "identificador" || token == "Numero")
                {
                    SiguienteToken();
                }
            }

            if (token != ";")
                throw new Exception("Se esperaba ';' después de la condición.");
            SiguienteToken();

            // Incremento
            if (token != ")")
            {
                if (token == "identificador")
                {
                    SiguienteToken();
                    if (token == "+" || token == "-")
                    {
                        SiguienteToken();
                        if (token == "+" || token == "-") SiguienteToken();
                    }
                }
            }

            if (token != ")")
                throw new Exception("Se esperaba ')' en for.");
            SiguienteToken();

            // Cuerpo
            if (token == "{")
            {
                CuerpoFuncion();
            }
            else
            {
                Sentencia();
            }

            Rtbx_salida.AppendText("For analizado correctamente.\n");
        }
        private void CicloMientras()
        {
            Rtbx_salida.AppendText("Analizando while...\n");

            SiguienteToken(); // consume 'while'

            if (token != "(")
                throw new Exception("Se esperaba '(' después de while.");
            SiguienteToken();

            // Condición simplificada
            if (token != "identificador")
                throw new Exception("Se esperaba condición en while.");
            SiguienteToken();

            if (token != ")")
                throw new Exception("Se esperaba ')' después de la condición.");
            SiguienteToken();

            // Cuerpo
            if (token == "{")
            {
                CuerpoFuncion();
            }
            else
            {
                Sentencia();
            }

            Rtbx_salida.AppendText("While analizado correctamente.\n");
        }

        private void CicloRepetirMientras()
        {
            Rtbx_salida.AppendText("Analizando do-while...\n");

            SiguienteToken(); // consume 'do'

            // Cuerpo
            if (token == "{")
            {
                CuerpoFuncion();
            }
            else
            {
                Sentencia();
            }

            if (token != "while")
                throw new Exception("Se esperaba 'while' después del cuerpo del do.");
            SiguienteToken();

            if (token != "(")
                throw new Exception("Se esperaba '(' después de while.");
            SiguienteToken();

            // Condición
            if (token != "identificador")
                throw new Exception("Se esperaba condición en do-while.");
            SiguienteToken();

            if (token != ")")
                throw new Exception("Se esperaba ')' después de la condición.");
            SiguienteToken();

            if (token != ";")
                throw new Exception("Se esperaba ';' al final del do-while.");
            SiguienteToken();

            Rtbx_salida.AppendText("Do-while analizado correctamente.\n");
        }

        // Return
        private void SentenciaReturn()
        {
            SiguienteToken(); // consume 'return'

            if (token != ";")
            {
                if (token == "Numero" || token == "identificador" || token == "cadena")
                {
                    SiguienteToken();
                }
            }

            if (token != ";")
                throw new Exception("Se esperaba ';' después de return.");
            SiguienteToken();
        }
    }


}