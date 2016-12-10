using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace SmartPLC_Commander
{

    public class connection
    {
        public connection()
        {

        }
        ~connection()
        {

        }
        public int x, y;
        public int parent_node_id;
        public int connection_id;
        public string description;
        public type con_type;
        public datatype con_dtype;
        public Color con_color;
        public Rectangle drawable_rect;
        public Rectangle text_rect;
        public  node parent_node;


    }

    public struct transform
    {
        public int x, y;
    }

    public enum type
    {
        input, output, none
    }
    public enum datatype
    {
        _none, _string, _bool, _int, _float, _special, _generic, _color
    }

    public class node
    {

        public string class_name;
        public bool use_timer;
        public bool is_static;
        public bool is_lua_node;
        public bool use_serial;
        public bool requires_extention;
        public bool pass_though;
        public List<connection> connections = new List<connection>();
        public int idnr;
        public int nid;
        public transform pos;
        public string xml_name;
        public string title;
        public string category;

        public string description;
        public string param_string;
        public string input_con_string;
        public string output_con_string;

        public string param_properties;

        public string connection_string;
        public string extention_name;

        public int  schematic_id = -1;
        public Rectangle base_rect = new Rectangle();
        public Rectangle clipping_recht;
        Rectangle headline_rect = new Rectangle();
        Font text_font = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point);
        public node() {
            pos = new transform();
            xml_name = "";
            title = "";
            nid = -1;
            category = "";
            description = "";
            param_string = "";
            input_con_string = "";
            output_con_string = "";
            param_properties = "";
            connection_string = "";
            extention_name = "";

            use_timer = true;
            is_static = false;
            is_lua_node = false;
            use_serial = false;
            requires_extention = false;
            pass_though = false;
            class_name = "";

        }


        public void create_property_plane(ref Panel parameter_panel, ref Label ltitle, ref Label lnid, ref Label lnsi)
        {

            ltitle.Text = title;
            lnid.Text = nid.ToString();
            lnsi.Text = xml_name;
            parameter_panel.Controls.Clear();
            //  List<System.Windows.Forms> forms = new List<System.Windows.Forms>();
            //param_properties auslesen nach [0,type,desc,()]
            //fdazu passend die elementer erstelln und nach 0 anordnen

            if (param_properties == "") { return; } //dan gibts nichts zu laden
            string[] split_comma = param_properties.Split('%');

            for (int i = 0; i < split_comma.Length; i++)
            {
                if (split_comma[i] == "" || split_comma[i] == " ") { continue; } //reach end ?
                string param_construct = split_comma[i];
                param_construct = param_construct.Substring(1, param_construct.Length - 2);
                string[] split_construct = param_construct.Split(',');

                int param_id = Int16.Parse(split_construct[0]);
                //CREATE LABEL
                Label lable_tmp = new Label();
                lable_tmp.Enabled = true;
                lable_tmp.Location = new Point(6, 25 + (40 * param_id));
                lable_tmp.Text = split_construct[2];
                lable_tmp.Name = "desc_" + param_id.ToString();
                lable_tmp.Size = new Size(94, 13);
                parameter_panel.Controls.Add(lable_tmp);





                //string ohne bereich = normale textbox
                if ((split_construct[1] == "string" || split_construct[1] == "special" || split_construct[1] == "generic") && split_construct.Length == 3)
                {
                    TextBox textbox_tmp = new TextBox();
                    textbox_tmp.Enabled = true;
                    textbox_tmp.Location = new Point(100, 20 + (40 * param_id));
                    textbox_tmp.Size = new Size(150, 20);
                    textbox_tmp.Name = "param_" + param_id.ToString();

                    if (split_construct[2].ToLower().Replace(" ", "") == "msi")
                    {
                        if (extention_name != "") {
                            textbox_tmp.Text = extention_name;
                   
                        }
                    }
                    else
                    {
                        textbox_tmp.Text = "";
                    }
                    parameter_panel.Controls.Add(textbox_tmp);
                }
                //string mit breich = combobox
                else if (split_construct[1] == "string" && split_construct.Length == 4 && split_construct[3].Contains(" "))
                {
                    string[] values = split_construct[3].Substring(1, split_construct[3].Length - 2).Split(' ');
                    if (values.Length == 0) { break; }
                    ComboBox tcombp = new ComboBox();
                    tcombp.Enabled = true;
                    tcombp.Name = "param_" + param_id.ToString();
                    tcombp.Size = new Size(150, 20);
                    tcombp.Location = new Point(100, 20 + (40 * param_id));
                    tcombp.Items.AddRange(values);
                    tcombp.SelectedIndex = 0;
                    tcombp.Text = values[0];
                    parameter_panel.Controls.Add(tcombp);
                }

                else if (split_construct[1] == "int" && split_construct.Length == 3)
                {
                    NumericUpDown tnud = new NumericUpDown();
                    tnud.Enabled = true;
                    tnud.Name = "param_" + param_id.ToString();
                    tnud.Size = new Size(150, 20);
                    tnud.Location = new Point(100, 20 + (40 * param_id));
                    if (split_construct[2].ToLower().Replace(" ", "") == "mid")
                    {
                        tnud.Maximum = 65536;
                        tnud.Minimum = 0;
                        tnud.Value = 0;
                    }
                    else {
                        tnud.Maximum = int.MaxValue;
                        tnud.Minimum = int.MinValue;
                        tnud.Value = 0;
                    }




                    tnud.DecimalPlaces = 0;
                    tnud.Increment = 1;
                    parameter_panel.Controls.Add(tnud);
                }
                else if (split_construct[1] == "float" && split_construct.Length == 3)
                {
                    NumericUpDown tnud = new NumericUpDown();
                    tnud.Enabled = true;
                    tnud.Name = "param_" + param_id.ToString();
                    tnud.Size = new Size(150, 20);
                    tnud.Location = new Point(100, 20 + (40 * param_id));
                    tnud.Minimum = decimal.MinValue;
                    tnud.Maximum = decimal.MaxValue;
                    tnud.DecimalPlaces = 1;
                    tnud.Value = new decimal(0.0f);
                    tnud.Increment = new decimal(0.1f);
                    parameter_panel.Controls.Add(tnud);


                }
                else if (split_construct[1] == "int" && split_construct.Length == 4 && split_construct[3].Contains("-"))
                {
                    string[] values = split_construct[3].Substring(1, split_construct[3].Length - 2).Split('-');
                    if (values.Length == 0) { break; }

                    NumericUpDown tnud = new NumericUpDown();
                    tnud.Enabled = true;
                    tnud.Name = "param_" + param_id.ToString();
                    tnud.Size = new Size(150, 20);
                    tnud.Location = new Point(100, 20 + (40 * param_id));
                    tnud.Minimum = Int32.Parse(values[0]);
                    tnud.Maximum = Int32.Parse(values[1]);
                    tnud.Value = Int32.Parse(values[0]);
                    tnud.DecimalPlaces = 0;
                    tnud.Increment = 1;
                    parameter_panel.Controls.Add(tnud);
                }
                else if (split_construct[1] == "float" && split_construct.Length == 4 && split_construct[3].Contains("-"))
                {
                    string[] values = split_construct[3].Substring(1, split_construct[3].Length - 2).Split('-');
                    if (values.Length == 0) { break; }

                    NumericUpDown tnud = new NumericUpDown();
                    tnud.Enabled = true;
                    tnud.Name = "param_" + param_id.ToString();
                    tnud.Size = new Size(150, 20);
                    tnud.Location = new Point(100, 20 + (40 * param_id));
                    tnud.Minimum = new decimal(float.Parse(values[0]));
                    tnud.Maximum = new decimal(float.Parse(values[1]));
                    tnud.Value = new decimal(float.Parse(values[0]));
                    tnud.DecimalPlaces = 1;
                    tnud.Increment = new decimal(0.1f);
                    parameter_panel.Controls.Add(tnud);
                } else if (split_construct[1] == "bool" && split_construct.Length == 3)
                {


                    CheckBox tchbx = new CheckBox();
                    tchbx.Name = "param_" + param_id.ToString();
                    tchbx.Location = new Point(70, 20 + (40 * param_id));
                    tchbx.Enabled = true;
                    parameter_panel.Controls.Add(tchbx);

               
                }






                //bool checkbox



            }


            load_parameters(ref parameter_panel);

        }




        public void load_parameters(ref Panel param_panel)
        {

            if (param_string == "" || !param_string.Contains("%")) { return; }
             string[] split_param = param_string.Split('%');
            for (int i = 0; i < split_param.Length; i++)
            {
                if (param_panel.Controls["param_" + i.ToString()] != null)
                {

                    //TYPE DIFFERENT
                    param_panel.Controls["param_" + i.ToString()].Text = split_param[i];



                }

            }



            //wenn string != ""
            //durchsteppen und in die boxen schreiben

        }

        public void save_parameters(ref Panel param_panel)
        {
            param_string = "";

            for (int i = 0; i < param_panel.Controls.Count; i++)
            {
                ///LABELS ENTFEREN
                if (param_panel.Controls[i].Name.Contains("param_")) {
                    param_string += param_panel.Controls[i].Text + "%";
                }

            }
            if (param_string == "") {
                param_string = "%";
            }

        }




        public void create_connection_list()
        {
            connections.Clear();



            {
                if (input_con_string != "")
                {
                    string[] splitted_input_string = input_con_string.Split('%');
                    if (splitted_input_string.Length > 0)
                    {
                        for (int i = 0; i < splitted_input_string.Length; i++)
                        {
                            if (splitted_input_string[i] == "") { break; }
                            string[] one_input_string = splitted_input_string[i].Substring(1, splitted_input_string[i].Length - 2).Split(',');
                            connection tmp_con = new connection();
                            tmp_con.parent_node_id = nid;
                            tmp_con.con_type = type.input;
                            tmp_con.connection_id = Int32.Parse(one_input_string[0]);
                            tmp_con.description = one_input_string[2];
                           
                            if (one_input_string[1] == "" || one_input_string[1].ToLower() == "none") { tmp_con.con_dtype = datatype._none; tmp_con.con_color = Color.Black; }
                            if (one_input_string[1].ToLower() == "string") { tmp_con.con_dtype = datatype._string; tmp_con.con_color = Color.OrangeRed; }
                            if (one_input_string[1].ToLower() == "float") { tmp_con.con_dtype = datatype._float; tmp_con.con_color = Color.Cyan; }
                            if (one_input_string[1].ToLower() == "special") { tmp_con.con_dtype = datatype._special; tmp_con.con_color = Color.Yellow; }
                            if (one_input_string[1].ToLower() == "generic") { tmp_con.con_dtype = datatype._generic; tmp_con.con_color = Color.Violet; }
                            if (one_input_string[1].ToLower() == "color") { tmp_con.con_dtype = datatype._color; tmp_con.con_color = Color.Plum; }
                            if (one_input_string[1].ToLower() == "bool") { tmp_con.con_dtype = datatype._bool; tmp_con.con_color = Color.DarkSeaGreen; }
                            if (one_input_string[1].ToLower() == "int") { tmp_con.con_dtype = datatype._int; tmp_con.con_color = Color.Azure; }
                            connections.Add(tmp_con);
                        }
                    }
                }
            }







            {
                if (output_con_string != "")
                {
                    string[] splitted_input_string = output_con_string.Split('%');
                    if (splitted_input_string.Length > 0)
                    {
                        for (int i = 0; i < splitted_input_string.Length; i++)
                        {
                            if (splitted_input_string[i] == "") { break; }
                            string[] one_input_string = splitted_input_string[i].Substring(1, splitted_input_string[i].Length - 2).Split(',');
                            connection tmp_con = new connection();
                            tmp_con.parent_node_id = nid;


                            tmp_con.con_type = type.output;
                            tmp_con.connection_id = Int32.Parse(one_input_string[0]);
                            tmp_con.description = one_input_string[2];
                            if (one_input_string[1].ToLower() == "string") { tmp_con.con_dtype = datatype._string; tmp_con.con_color = Color.OrangeRed; }
                            if (one_input_string[1].ToLower() == "float") { tmp_con.con_dtype = datatype._float; tmp_con.con_color = Color.Cyan; }
                            if (one_input_string[1].ToLower() == "special") { tmp_con.con_dtype = datatype._special; tmp_con.con_color = Color.Yellow; }
                            if (one_input_string[1].ToLower() == "generic") { tmp_con.con_dtype = datatype._generic; tmp_con.con_color = Color.Violet; }
                            if (one_input_string[1].ToLower() == "color") { tmp_con.con_dtype = datatype._color; tmp_con.con_color = Color.Plum; }
                            if (one_input_string[1].ToLower() == "bool") { tmp_con.con_dtype = datatype._bool; tmp_con.con_color = Color.DarkSeaGreen; }
                            if (one_input_string[1].ToLower() == "int") { tmp_con.con_dtype = datatype._int; tmp_con.con_color = Color.Azure; }
                            connections.Add(tmp_con);
                        }
                    }
                }
            }
            //den connectionstring druchgehen und passend splitten
        }

        public void draw_update(ref Graphics g, Rectangle view_rect, ref Point offset)
        {
            if (clipping_recht.IntersectsWith(view_rect))
            {

                //BASIC CLIPPING
                if (base_rect.IntersectsWith(view_rect))
                {
                    //DRAW BACKGROUND RECT
                    g.DrawRectangle(new Pen(Color.Green), base_rect);
                    //DRAW HEADLINE
                    if (title != "")
                    {
                        g.DrawString(title, text_font, Brushes.Black, headline_rect);
                    }

                    //DRAWW ALL I/O CONNECTIONS
                    for (int i = 0; i < connections.Count; i++)
                    {
                        if (view_rect.IntersectsWith(connections[i].drawable_rect))
                        {
                            g.DrawRectangle(new Pen(connections[i].con_color), connections[i].drawable_rect);
                        }
                         if (connections[i].description != "" || view_rect.IntersectsWith(connections[i].text_rect))
                            {
                                g.DrawString(connections[i].description, text_font, Brushes.Black, connections[i].text_rect);
                            }
                        
                    }

                }

            }
        }
        
    


        //FOR RECT WIDTH
        public int distance_betewenn_con_text = 20; //defult: *3
        private int distance_between_border_and_inputtext_end = 20;
        private int distance_between_border_and_outputtext_end = 20;
        private int char_lenght_multiplier = 9;
        //FOR RECT HEIGHT
        public  int headline_text_distance = 20; //default: *3
        public int distance_between_connections = 20;

        private int connection_rect_widht = 15;
        private int connection_rect_height = 15;
        public void create_drawable()
        {
            int rect_width = 3* distance_betewenn_con_text;
            int recht_height = 3* headline_text_distance;

            int intput_con_amount = 0;
            int output_con_amount = 0;
            for (int i = 0; i < connections.Count; i++)
            {

                connection tmp_con = connections[i];

                if(tmp_con.con_type == type.input)
                {
                    intput_con_amount++;
                    if ((tmp_con.description.Length*char_lenght_multiplier) > distance_between_border_and_inputtext_end)
                    {
                        distance_between_border_and_inputtext_end = (tmp_con.description.Length * char_lenght_multiplier);
                    }
                }

                if (tmp_con.con_type == type.output)
                {
                    output_con_amount++;
                    if ((tmp_con.description.Length * char_lenght_multiplier) > distance_between_border_and_outputtext_end)
                    {
                        distance_between_border_and_outputtext_end = (tmp_con.description.Length * char_lenght_multiplier);
                    }
                }
            }
            //CALC BASE RECT
            if (output_con_amount > intput_con_amount)
            {
                recht_height = (distance_between_connections * output_con_amount)+ headline_text_distance;
            }
            else {
                recht_height = (distance_between_connections * intput_con_amount)+ headline_text_distance;
            }
            
            rect_width = distance_betewenn_con_text + distance_between_border_and_outputtext_end + distance_between_border_and_inputtext_end;
            base_rect.Size = new Size(rect_width, recht_height);
            base_rect.Location = new Point(pos.x, pos.y);
            //CALC CLIPPING RECT
            clipping_recht.Location = new Point(pos.x- connection_rect_widht, pos.y);
            clipping_recht.Size = new Size(rect_width+ (2*connection_rect_widht), recht_height);

            //CALC HEADLINE TEXT RECT
            headline_rect.Location = base_rect.Location; //TODO: center it
            headline_rect.Size = new Size(title.Length * char_lenght_multiplier, headline_text_distance);

            Point input_cons_start_point = new Point(base_rect.X, base_rect.Y + headline_text_distance);
            Point output_cons_start_point = new Point(base_rect.X + base_rect.Width, base_rect.Y + headline_text_distance);

            intput_con_amount = 0;
            output_con_amount = 0;
            
            for (int i = 0; i < connections.Count; i++)
            {
                connection tmp_con = connections[i];
                if(tmp_con.con_type == type.input)
                {
                    
                    tmp_con.drawable_rect.Location = new Point(input_cons_start_point.X- connection_rect_widht, input_cons_start_point.Y + (intput_con_amount* distance_between_connections));
                    tmp_con.drawable_rect.Size = new Size(connection_rect_widht, connection_rect_height);

                    tmp_con.text_rect.Location = new Point(input_cons_start_point.X + connection_rect_widht, tmp_con.drawable_rect.Location.Y);
                    tmp_con.text_rect.Size = new Size(tmp_con.description.Length * char_lenght_multiplier, connection_rect_height);


                    connections[i] = tmp_con;
                    intput_con_amount++;
                }


                if (tmp_con.con_type == type.output)
                {
                 
                    tmp_con.drawable_rect.Location = new Point(output_cons_start_point.X, output_cons_start_point.Y + (output_con_amount * distance_between_connections));
                    tmp_con.drawable_rect.Size = new Size(connection_rect_widht, connection_rect_height);

                    tmp_con.text_rect.Location = new Point(tmp_con.drawable_rect.Location.X-(tmp_con.description.Length * char_lenght_multiplier), tmp_con.drawable_rect.Location.Y);
                    tmp_con.text_rect.Size = new Size(tmp_con.description.Length * char_lenght_multiplier, connection_rect_height);


                    connections[i] = tmp_con;
                    output_con_amount++;
                }
            }

            //calc the headline recht

            //calc the con desc rects


        }
        public void calc_element_positions()
        {
            //REcht grösse berechnen ->nach anzahl der zeichen in der headline und

           
          

          
          
        }
    }
}
