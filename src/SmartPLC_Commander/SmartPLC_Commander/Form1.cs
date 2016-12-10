using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net;
using System.IO;

namespace SmartPLC_Commander
{




    public partial class Form1 : Form
    {
        Graphics graphics;
        Image render_image;

        int schematic_coount = 0;
        int schematic_count_min = 0;
        int schematic_coount_max = 9;
        List<node> loaded_nodes = new List<node>();
        List<node> schematic_nodes = new List<node>();
        TreeNode selected_treeview_node = new TreeNode();
        string loaded_node_list = "";
        int nid_counter = 1;
        Bitmap drawing_bitmap = new Bitmap(500, 500);
        TreeNode root_node;
        public Form1()
        {
            InitializeComponent();

            selected_history_tree_node = null;
            selected_treeview_node = null;
            //ENABLE RENDERTIMER
            timer1.Enabled = false;
            //CREATE SCHEMATIC BOX
            comboBox1.Items.Clear();
            for (int i = schematic_count_min; i < schematic_coount_max; i++)
            {
                comboBox1.Items.Add("Schematic " + i.ToString());  
            }
            comboBox1.SelectedIndex = 0;
            //CREATE GRFICS
            drawing_bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(drawing_bitmap);
            //CLEAR PICTURE
            clear_picture();
            pictureBox1.Focus();
        }
        //CLEAR PICTUREBOX WITH GREY
        public void clear_picture()
        {
            graphics.FillRectangle(Brushes.DarkGray, 0, 0, drawing_bitmap.Width, drawing_bitmap.Height);
            pictureBox1.Image = drawing_bitmap;
          
        }
        //LOADED NODE TREEE VIEW
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selected_treeview_node = e.Node;
        }
        //LOAD NODE CONFIG
        private void nodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Implemented Node List (.csv)|*.csv";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                root_node = null;
                loaded_node_list = openFileDialog1.FileName;
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                    treeView1.BeginUpdate();
                    treeView1.Nodes.Clear();
                    treeView1.EndUpdate();

                    loaded_nodes.Clear();
                    //begin loading nodes to tree view
                    //the 1 to skip the headline
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string sel_line = lines[i];
                        string[] splitted_content = sel_line.Split(';');

                        TreeNode tn = new TreeNode();
                        tn.Name = splitted_content[1];
                        tn.Text = splitted_content[2];

                        if (treeView1.Nodes["__cat__" + splitted_content[3]] == null)
                        {
                            TreeNode tmp_tn = new TreeNode();
                            tmp_tn.Name = "__cat__" + splitted_content[3];
                            tmp_tn.Text = splitted_content[3];
                            treeView1.Nodes.Add(tmp_tn);
                            treeView1.Nodes["__cat__" + splitted_content[3]].Nodes.Add(tn);
                            if(root_node != null) { root_node = tn; }
                        }
                        else
                        {
                            treeView1.Nodes["__cat__" + splitted_content[3]].Nodes.Add(tn);
                        }

                        //CREATE PRPERTY CLASS
                        node tnode = new node();
                        tnode.category = splitted_content[3];
                        tnode.xml_name = splitted_content[1];
                        tnode.title = splitted_content[2];
                        tnode.description = splitted_content[13];
                        tnode.param_properties = splitted_content[12];
                        tnode.output_con_string = splitted_content[11];
                        tnode.input_con_string = splitted_content[10];
                        tnode.extention_name = splitted_content[9];


                        tnode.class_name = splitted_content[0];

                        if (splitted_content[4].ToLower().Contains("true"))
                        {
                            tnode.use_serial = true;
                        }
                        else
                        {
                            tnode.use_serial = false;
                        }


                        if (splitted_content[5].ToLower().Contains("true"))
                        {
                            tnode.is_static = true;
                        }
                        else
                        {
                            tnode.is_static = false;
                        }

                        if (splitted_content[14].ToLower().Contains("true"))
                        {
                            tnode.use_timer = true;
                        }
                        else
                        {
                            tnode.use_timer = false;
                        }

                        if (splitted_content[15].ToLower().Contains("true"))
                        {
                            tnode.is_lua_node = true;
                        }
                        else
                        {
                            tnode.is_lua_node = false;
                        }


                        if (splitted_content[8].ToLower().Contains("true"))
                        {
                            tnode.requires_extention = true;
                        }
                        else
                        {
                            tnode.requires_extention = false;
                        }


                        if (splitted_content[7].ToLower().Contains("true"))
                        {
                            tnode.pass_though = true;
                        }
                        else
                        {
                            tnode.pass_though = false;
                        }
                        tnode.idnr = i;
                        tnode.extention_name = splitted_content[9];
                        loaded_nodes.Add(tnode);



                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error while loading ");
                }
            }
        }
        //SAVE NODE CONFIG
        private void saveNodeSchematicToolStripMenuItem_Click(object sender, EventArgs e)
        {
           //create header string
            string final_string = "<?xml version=\"1.0\"?><info schematic-list=\"" + loaded_node_list + "\" date=\"" + System.DateTime.UtcNow.ToString() + "\" schematiccount=\"" + (schematic_coount_max - schematic_count_min).ToString() + "\" />";
            for (int j = schematic_count_min; j < schematic_coount_max; j++)
            {

                final_string += "<schematic>";
            
                foreach (node n in schematic_nodes)
                {
                    if(n.schematic_id != j)
                    {
                        continue;
                    }
                    //GENERATE CONNECTION STRING
                    string constring_tmp = "";
                    for (int i = 0; i < connection_list.Count; i++)
                    {
                        if (connection_list[i].source.parent_node_id == n.nid)
                        {
                            constring_tmp += connection_list[i].source.parent_node_id.ToString() + ":" + connection_list[i].source.connection_id.ToString() + ":" + connection_list[i].target.parent_node_id.ToString() + ":" + connection_list[i].target.connection_id.ToString() + "%";
                        }
                    }
                    n.connection_string = constring_tmp;
                    //<node nid="6" nsi="ctimest" ncon="6:0:7:0%" nparam="%" />
                    final_string += "<node nid=\"" + n.nid + "\" nsi=\"" + n.xml_name + "\" ncon=\"" + constring_tmp + "\" nparam=\"" + n.param_string + "\" pos=\"" + n.pos.x.ToString() + ";" + n.pos.y.ToString() + "\" use_timer=\"" + n.use_timer.ToString().ToLower() + "\" is_static=\"" + n.is_static.ToString().ToLower() + "\" is_lua_node=\"" + n.is_lua_node.ToString().ToLower() + "\" use_serial=\"" + n.use_serial.ToString().ToLower() + "\" requires_extention=\"" + n.requires_extention.ToString().ToLower() + "\" extention_name=\"" + n.extention_name + "\" pass_through=\"" + n.pass_though.ToString().ToLower() + "\" class_name=\"" + n.class_name + "\" />";
              
                }
                final_string += "</schematic>";
           


            }
          

            final_string += "</xml>";
            //UPLOAD
            if (!checkBox1.Checked)
            {
                WebClient webClient = new WebClient();

                NameValueCollection formData = new NameValueCollection();
                formData["data"] = final_string ;
            

                byte[] responseBytes = webClient.UploadValues("http://192.168.178.58/smartsps/text.php", "POST", formData);
                string responsefromserver = Encoding.UTF8.GetString(responseBytes);
                Console.WriteLine(responsefromserver);
                webClient.Dispose();
            }
            else
            {
                saveFileDialog1.Filter = "Node Schematic (.xml)|*.xml";
                saveFileDialog1.FilterIndex = 1;


                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog1.FileName);
                    sw.Write(final_string);
                    sw.Close();
                }
            }
            //  MessageBox.Show(final_string);

    

        }
        //LOAD ADDItION NODE CONFIG
        private void loadAdditionalNodeConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Implemented Node List (.csv)|*.csv";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                loaded_node_list += " | " + openFileDialog1.FileName;
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                    // treeView1.BeginUpdate();
                    // treeView1.Nodes.Clear();
                    //treeView1.EndUpdate();

                    //loaded_nodes.Clear();
                    //begin loading nodes to tree view
                    //the 1 to skip the headline
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string sel_line = lines[i];
                        string[] splitted_content = sel_line.Split(';');

                        TreeNode tn = new TreeNode();
                        tn.Name = splitted_content[1];
                        tn.Text = splitted_content[2];

                        if (treeView1.Nodes["__cat__" + splitted_content[3]] == null)
                        {
                            TreeNode tmp_tn = new TreeNode();
                            tmp_tn.Name = "__cat__" + splitted_content[3];
                            tmp_tn.Text = splitted_content[3];
                            treeView1.Nodes.Add(tmp_tn);
                            treeView1.Nodes["__cat__" + splitted_content[3]].Nodes.Add(tn);
                        }
                        else
                        {
                            treeView1.Nodes["__cat__" + splitted_content[3]].Nodes.Add(tn);
                        }

                        //CREATE PRPERTY CLASS
                        node tnode = new node();
                        tnode.category = splitted_content[3];
                        tnode.xml_name = splitted_content[1];
                        tnode.title = splitted_content[2];
                        tnode.description = splitted_content[13];
                        tnode.param_properties = splitted_content[12];
                        tnode.output_con_string = splitted_content[11];
                        tnode.input_con_string = splitted_content[10];
                        tnode.extention_name = splitted_content[9];


                        tnode.class_name = splitted_content[0];

                        if (splitted_content[4].ToLower().Contains("true"))
                        {
                            tnode.use_serial = true;
                        }
                        else
                        {
                            tnode.use_serial = false;
                        }


                        if (splitted_content[5].ToLower().Contains("true"))
                        {
                            tnode.is_static = true;
                        }
                        else
                        {
                            tnode.is_static = false;
                        }

                        if (splitted_content[14].ToLower().Contains("true"))
                        {
                            tnode.use_timer = true;
                        }
                        else
                        {
                            tnode.use_timer = false;
                        }

                        if (splitted_content[15].ToLower().Contains("true"))
                        {
                            tnode.is_lua_node = true;
                        }
                        else
                        {
                            tnode.is_lua_node = false;
                        }


                        if (splitted_content[8].ToLower().Contains("true"))
                        {
                            tnode.requires_extention = true;
                        }
                        else
                        {
                            tnode.requires_extention = false;
                        }


                        if (splitted_content[7].ToLower().Contains("true"))
                        {
                            tnode.pass_though = true;
                        }
                        else
                        {
                            tnode.pass_though = false;
                        }

                        tnode.idnr = i;
                        loaded_nodes.Add(tnode);



                    }



                }
                catch (Exception)
                {
                    MessageBox.Show("Error while loading ");

                }

            }
        }
        //HISTORY TREE VIEW SELECT
        TreeNode selected_history_tree_node = new TreeNode();
        node selected_history_node = null;
        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selected_history_tree_node = e.Node;
            string nid_string = e.Node.Name.Replace("inst_", "");
            int sel_nid = Int32.Parse(nid_string);
            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                if (schematic_nodes[i].nid == sel_nid && schematic_nodes[i].schematic_id == comboBox1.SelectedIndex)
                {
                    selected_history_node = schematic_nodes[i];
                    schematic_nodes[i].create_property_plane(ref parameter_panel_form, ref node_title_text, ref node_nid_text, ref node_nsi_text);
                }

            }
        }
        //ADD NODE BTN
        private void button1_Click(object sender, EventArgs e)
        {
            if (selected_treeview_node == null) { return; }
            if (selected_treeview_node.Name.Contains("__cat__")) { return; }
          
            for (int i = 0; i < loaded_nodes.Count; i++)
            {
                if (loaded_nodes[i].xml_name == selected_treeview_node.Name)
                {
                    nid_counter++;
                    node tmnode = new node();
                    tmnode.is_static = loaded_nodes[i].is_static;
                    tmnode.use_timer = loaded_nodes[i].use_timer;
                    tmnode.use_serial = loaded_nodes[i].use_serial;
                    tmnode.is_lua_node = loaded_nodes[i].is_lua_node;

                    tmnode.requires_extention = loaded_nodes[i].requires_extention;
                    tmnode.pass_though = loaded_nodes[i].pass_though;
                    tmnode.class_name = loaded_nodes[i].class_name;



                    tmnode.idnr = loaded_nodes[i].idnr;
                    tmnode.xml_name = loaded_nodes[i].xml_name;
                    tmnode.title = loaded_nodes[i].title;
                    tmnode.category = loaded_nodes[i].category;
                    tmnode.description = loaded_nodes[i].description;
                    tmnode.param_properties = loaded_nodes[i].param_properties;
                    tmnode.param_string = loaded_nodes[i].param_string;
                    tmnode.input_con_string = loaded_nodes[i].input_con_string;
                    tmnode.output_con_string = loaded_nodes[i].output_con_string;
                    tmnode.connection_string = loaded_nodes[i].connection_string;
                    tmnode.extention_name = loaded_nodes[i].extention_name;
                    tmnode.nid = nid_counter;
                    tmnode.pos.x = 10;
                    tmnode.pos.y = 10;
                    tmnode.schematic_id = comboBox1.SelectedIndex;
                    tmnode.create_connection_list();
                    tmnode.create_drawable(); //actung zuerst create_con_list_aufrufen
                    schematic_nodes.Add(tmnode);
                    //set for every node in node schem for all connections the parent_node tho the schem_node
                    make_connection_reference();
                     update_insta_view();
                    if (!timer1.Enabled)
                    {
                        timer1.Enabled = true;
                    }
                    treeView1.SelectedNode = root_node;
                    break;
                }
            }
            
            selected_treeview_node = null;
        }
        //REMOVE NODE BTN
        private void button2_Click(object sender, EventArgs e)
        {
            if (selected_history_tree_node == null) { return; }
            string nid_string = selected_history_tree_node.Name.Replace("inst_", "");
            int sel_nid = Int32.Parse(nid_string);
            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                if (schematic_nodes[i].nid == sel_nid)
                {
                    treeView2.Nodes.Remove(selected_history_tree_node);
                    schematic_nodes.Remove(schematic_nodes[i]);
                }
            }


            selected_history_tree_node = null;
        }
        //DRAWING TIMER
        private void timer1_Tick(object sender, EventArgs e)
        {
            //CLEAR WITH WHITE
            graphics.FillRectangle(Brushes.DarkGray, 0, 0, drawing_bitmap.Width, drawing_bitmap.Height);
         
            //DRAW ALL NODES
            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                if (schematic_nodes[i].schematic_id == comboBox1.SelectedIndex)
                {
                    schematic_nodes[i].draw_update(ref graphics, new Rectangle(0, 0, drawing_bitmap.Width, drawing_bitmap.Height), ref pos_scoll_offset);
                }
              }

            //DRAW CONNECTION LINES

            for (int j = 0; j < connection_list.Count; j++)
            {

              if(connection_list[j].source.parent_node.schematic_id == comboBox1.SelectedIndex && connection_list[j].target.parent_node.schematic_id == comboBox1.SelectedIndex)
                {
                    Point point_source = new Point(connection_list[j].source.drawable_rect.Location.X + connection_list[j].source.drawable_rect.Size.Width, connection_list[j].source.drawable_rect.Location.Y + (connection_list[j].source.drawable_rect.Size.Width / 2));
                    Point point_target = new Point(connection_list[j].target.drawable_rect.Location.X, connection_list[j].target.drawable_rect.Location.Y + (connection_list[j].target.drawable_rect.Size.Width / 2));
                    Point point_middle = new Point(0, 0);
                    if (point_source.X < point_target.X)
                    {
                        point_middle.X = point_target.X - point_source.X;
                    }
                    else
                    {
                        point_middle.X = point_source.X + point_target.X;
                    }

                    if (point_source.Y < point_target.Y)
                    {
                        point_middle.Y = point_target.Y - point_source.Y;
                    }
                    else
                    {
                        point_middle.Y = point_source.Y + point_target.Y;
                    }

                    //NACH TYP
                    graphics.DrawBezier(new Pen(connection_list[j].source.con_color, 5.0f), point_source, new Point(point_middle.X, point_source.Y), new Point(point_middle.X, point_target.Y), point_target); 
                }



            }


            //DRAW IMAGE
            pictureBox1.Image = drawing_bitmap;

        }
        //CLAR ALLL BTN
        private void button3_Click(object sender, EventArgs e)
        {
            nid_counter = 1;
            schematic_nodes.Clear();
            connection_list.Clear();

            //clear 
            treeView2.Nodes.Clear();
            //cleat_view
            graphics.FillRectangle(Brushes.DarkGray, 0, 0, drawing_bitmap.Width, drawing_bitmap.Height);
            pictureBox1.Image = drawing_bitmap;
        }
        //MOUSE CLICK STUFF
        Rectangle mouse_pos_rect;
        node drag_node = null;
        Point drag_node_offset = new Point(0, 0);
        connection selected_connection = null;
        struct connection_pair
        {
            public connection source;
            public connection target;
        }
        List<connection_pair> connection_list = new List<connection_pair>();
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            mouse_pos_rect.Location = e.Location;
            mouse_pos_rect.Size = new Size(1, 1);
            if (mouse_pos_rect.IntersectsWith(new Rectangle(0, 0, drawing_bitmap.Width, drawing_bitmap.Height)))
            {

                for (int i = 0; i < schematic_nodes.Count; i++)
                {
                    if (schematic_nodes[i].clipping_recht.IntersectsWith(mouse_pos_rect))
                    {
                        //check if is in base rect
                        if (schematic_nodes[i].base_rect.IntersectsWith(mouse_pos_rect))
                        {
                            selected_connection = null;
                            drag_node = schematic_nodes[i];
                            drag_node_offset.X = (schematic_nodes[i].pos.x - mouse_pos_rect.Location.X);
                            drag_node_offset.Y = (schematic_nodes[i].pos.y - mouse_pos_rect.Location.Y);
                            //switch to property plane
                            drag_node.create_property_plane(ref parameter_panel_form, ref node_title_text, ref node_nid_text, ref node_nsi_text);
                            if(selected_history_node != null)
                            {
                              //  selected_history_node.save_parameters(ref parameter_panel_form);
                            }
                            selected_history_node = drag_node;
                            tabControl1.SelectedIndex = 2;
                        }
                        else
                        {
                            //check if mouse on a connector
                            for (int j = 0; j < schematic_nodes[i].connections.Count; j++)
                            {
                                if (schematic_nodes[i].connections[j].drawable_rect.IntersectsWith(mouse_pos_rect))
                                {

                                    //WENN NICHT LERR DANN VERBINFUNG MACHEN
                                    if (selected_connection == null)
                                    {
                                        selected_connection = schematic_nodes[i].connections[j];
                                    }
                                    else
                                    {

                                        connection second_clicked_connection = schematic_nodes[i].connections[j];
                                        if (second_clicked_connection.con_type == type.input && selected_connection.con_type == type.output)
                                        {
                                            connection_pair tmp_cpair = new connection_pair();
                                            tmp_cpair.source = selected_connection;
                                            tmp_cpair.target = second_clicked_connection;

                                            bool exits = false;
                                            for (int k = 0; k < connection_list.Count; k++)
                                            {
                                                if (connection_list[k].source.connection_id == tmp_cpair.source.connection_id && connection_list[k].source.parent_node_id == tmp_cpair.source.parent_node_id)
                                                {
                                                    if (connection_list[k].target.connection_id == tmp_cpair.target.connection_id && connection_list[k].target.parent_node_id == tmp_cpair.target.parent_node_id)
                                                    {
                                                        exits = true;
                                                    }
                                                }
                                            }

                                            if (!exits)
                                            {
                                                connection_list.Add(tmp_cpair);
                                            }
                                        }
                                        else if (second_clicked_connection.con_type == type.output && selected_connection.con_type == type.input)
                                        {

                                            connection_pair tmp_cpair = new connection_pair();
                                            tmp_cpair.source = second_clicked_connection;
                                            tmp_cpair.target = selected_connection;
                                            bool exits = false;
                                            for (int k = 0; k < connection_list.Count; k++)
                                            {
                                                if (connection_list[k].source.connection_id == tmp_cpair.source.connection_id && connection_list[k].source.parent_node_id == tmp_cpair.source.parent_node_id)
                                                {
                                                    if (connection_list[k].target.connection_id == tmp_cpair.target.connection_id && connection_list[k].target.parent_node_id == tmp_cpair.target.parent_node_id)
                                                    {
                                                        exits = true;
                                                    }
                                                }
                                            }

                                            if (!exits)
                                            {
                                                connection_list.Add(tmp_cpair);
                                            }
                                        }

                                        selected_connection = null;
                                    }


                                }
                            }
                        }
                    }
                }

            }
            //create 1x1 mouse click rect
            //fist is mouse on image
            //deltapos von der geklickten zur xy des nodes dmit es nicht springt


        }
        Point min_pos_values = new Point(-100,-100);
        Point max_pos_vlaues = new Point(100,100);
        Point pos_scoll_offset = new Point(0,0);
        //MOUSE MOVE PICTUREBOX
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag_node != null)
            {
                mouse_pos_rect.Location = e.Location;

                drag_node.pos.x = mouse_pos_rect.X + drag_node_offset.X;
                drag_node.pos.y = mouse_pos_rect.Y + drag_node_offset.Y;
                drag_node.create_drawable();
                // drag_node_offset

                for (int i = 0; i < schematic_nodes.Count; i++)
                {
                   if( schematic_nodes[i].base_rect.X < min_pos_values.X) {
                        min_pos_values.X = schematic_nodes[i].base_rect.X;
                    }
                    if (schematic_nodes[i].base_rect.Y < min_pos_values.Y)
                    {
                        min_pos_values.Y = schematic_nodes[i].base_rect.Y;
                    }



                    if (schematic_nodes[i].base_rect.X > max_pos_vlaues.X)
                    {
                        max_pos_vlaues.X = schematic_nodes[i].base_rect.X;
                    }
                    if (schematic_nodes[i].base_rect.Y > max_pos_vlaues.Y)
                    {
                        max_pos_vlaues.Y = schematic_nodes[i].base_rect.Y;
                    }

                }



                hScrollBar1.Maximum = max_pos_vlaues.X;
                hScrollBar1.Minimum = min_pos_values.X;

                vScrollBar1.Maximum = max_pos_vlaues.Y;
                vScrollBar1.Minimum = min_pos_values.Y;

            }
        }
        //MOUSE BUTTON UP PICTUREBOX
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            drag_node = null;
        }
        //MOUSE BUTTONS CLICK PICTUREBOX
        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            mouse_pos_rect.Location = e.Location;
            mouse_pos_rect.Size = new Size(1, 1);
            if (mouse_pos_rect.IntersectsWith(new Rectangle(0, 0, drawing_bitmap.Width, drawing_bitmap.Height)))
            {

                for (int i = 0; i < schematic_nodes.Count; i++)
                {
                    if (schematic_nodes[i].clipping_recht.IntersectsWith(mouse_pos_rect))
                    {

                        //check if mouse on a connector
                        for (int j = 0; j < schematic_nodes[i].connections.Count; j++)
                        {
                            if (schematic_nodes[i].connections[j].drawable_rect.IntersectsWith(mouse_pos_rect))
                            {
                                connection tmp_con = schematic_nodes[i].connections[j];

                                for (int k = 0; k < connection_list.Count; k++)
                                {
                                    if(tmp_con.con_type == type.output) {
                                        if(tmp_con.connection_id == connection_list[k].source.connection_id && tmp_con.parent_node_id == connection_list[k].source.parent_node_id)
                                        {
                                            connection_list.Remove(connection_list[k]);
                                        }
                                    }else if (tmp_con.con_type == type.input)
                                    {
                                        if (tmp_con.connection_id == connection_list[k].target.connection_id && tmp_con.parent_node_id == connection_list[k].target.parent_node_id)
                                        {
                                            connection_list.Remove(connection_list[k]);
                                        }
                                    }

                                }


                            }
                        }

                            }
                        }
            }

        }
        //LOAD SCHEMATIC BTN
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

            //download current schematic
            //parse
            //instante all nodes with right values for variables
            //call create drawabel
            //load parameters
            //create_connection_list


            string schem_content = "";

            openFileDialog1.Filter = "Node-Schematic (.xml)|*.xml";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if(openFileDialog1.FileName == "") { MessageBox.Show("enter valid file"); return; }

              
                try
                {
                    schem_content = System.IO.File.ReadAllText(openFileDialog1.FileName);
                }
                catch (Exception)
                {

                    MessageBox.Show("ERROR WHILE LOADING");
                }
         
            }


            if(schem_content == "")
            {
                return;
            }

            schem_content = schem_content.Replace("\r\n", "");
            if (!schem_content.StartsWith("<?xml version=\"1.0\"?>")) { MessageBox.Show("StartWith - FAIL"); return; }
            bool schem_starts = false;
            int schem_count = 0;
            schematic_nodes.Clear();
            connection_list.Clear();
            clear_picture();
            timer1.Enabled = false;
            string[] split_xml = schem_content.Split('<');

            for (int i = 0; i < split_xml.Length; i++)
            {
                if (split_xml[i].StartsWith("?xml")) { /* XML HEAD */}

                if (split_xml[i].StartsWith("info")) { /* XML INFO HEAD */}

                if (split_xml[i].StartsWith("schematic")) { /*Start schem */schem_starts = true; }

                if (split_xml[i].StartsWith("/schematic")) { /*end schem */
                    schem_starts = false; schem_count++; }

                if (split_xml[i].StartsWith("node") && schem_starts) {
                    /* NODE CONTENT */
                    string node_info_string = split_xml[i];
                    node tmp = new node();
                    tmp.schematic_id = schem_count;


                    int start_exp_pos = -1;
                    int end_exp_pos = -1;
                    string extr_value = "";
                    //READ NID
                     start_exp_pos = node_info_string.IndexOf("nid=\"")+ "nid=\"".Length;
                     end_exp_pos = node_info_string.IndexOf("\" ", start_exp_pos);
                     extr_value = node_info_string.Substring(start_exp_pos, (end_exp_pos-start_exp_pos));
                    tmp.nid = Int32.Parse(extr_value);
                    //READ NSI -> nbdi
                    start_exp_pos = node_info_string.IndexOf("nsi=\"") + "nsi=\"".Length;
                    end_exp_pos = node_info_string.IndexOf("\" ", start_exp_pos);
                    extr_value = node_info_string.Substring(start_exp_pos, (end_exp_pos - start_exp_pos));
                    tmp.xml_name =extr_value;
                    //ncon
                    start_exp_pos = node_info_string.IndexOf("ncon=\"") + "ncon=\"".Length;
                    end_exp_pos = node_info_string.IndexOf("\" ", start_exp_pos);
                    extr_value = node_info_string.Substring(start_exp_pos, (end_exp_pos - start_exp_pos));
                    tmp.connection_string = extr_value;
                    //nparam
                    start_exp_pos = node_info_string.IndexOf("nparam=\"") + "nparam=\"".Length;
                    end_exp_pos = node_info_string.IndexOf("\" ", start_exp_pos);
                    extr_value = node_info_string.Substring(start_exp_pos, (end_exp_pos - start_exp_pos));
                    tmp.param_string = extr_value;
                    //pos
                    start_exp_pos = node_info_string.IndexOf("pos=\"") + "pos=\"".Length;
                    end_exp_pos = node_info_string.IndexOf("\" ", start_exp_pos);
                    extr_value = node_info_string.Substring(start_exp_pos, (end_exp_pos - start_exp_pos));
                    string[] spos = extr_value.Split(';');
                    tmp.pos.x = Int32.Parse(spos[0]);
                    tmp.pos.y = Int32.Parse(spos[1]);


                    //NOW READ MISSING INFO FROM THE CSV
                    /*
                    input_con_string
                    output_con_string
                    param_properies string
                    extern_name
                    idnr
                    title
                    category
                    description
    */
                    for (int l = 0; l < loaded_nodes.Count; l++)
                    {
                        if(loaded_nodes[l].xml_name == tmp.xml_name)
                        {
                            tmp.idnr = l;
                            tmp.title = loaded_nodes[l].title;
                            tmp.description = loaded_nodes[l].description;
                            tmp.category = loaded_nodes[l].category;
                            tmp.extention_name = loaded_nodes[l].extention_name;
                            tmp.param_properties = loaded_nodes[l].param_properties;
                            tmp.output_con_string = loaded_nodes[l].output_con_string;
                            tmp.input_con_string = loaded_nodes[l].input_con_string;
                            break;
                        }
                    }

                    //create some lists
                    tmp.create_connection_list();
                    tmp.create_drawable();
                    //add to list
                    schematic_nodes.Add(tmp);
                    //some other linking stuff
                    make_connection_reference();
                    update_insta_view();
                    
                 }
              
                timer1.Enabled = true;

            }



            //    saveFileDialog1.Filter = "Node Schematic (.xml)|*.xml";
            // saveFileDialog1.FilterIndex = 1;

            //download schematic y
            //instnace nodes
            //wen pos nicht bekannt dann 10;10
            //create_connectreions // create drawable // aufrufen
            reconstruct_connections();
        }


        public void reconstruct_connections()
        {
            //durch alle nodes -> alle con strings und in die connection list einfügen
            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                node sel_tmp_node = schematic_nodes[i];
                string con_string = sel_tmp_node.connection_string;
                if(con_string == "" || con_string == " " || con_string == "%") { continue; }

                string[] splitted_con_string = con_string.Split('%');

                for (int j = 0; j < splitted_con_string.Length; j++)
                {
                    if(splitted_con_string[j] == "" || splitted_con_string[j] == ":::" || splitted_con_string[j] == " ") { continue; }
                    string[] single_con_string = splitted_con_string[j].Split(':');

                    int source_node = Int32.Parse(single_con_string[0]);
                    int source_con = Int32.Parse(single_con_string[1]);
                    int target_node = Int32.Parse(single_con_string[2]);
                    int target_con= Int32.Parse(single_con_string[3]);


                    connection_pair tmp_pair = new connection_pair();
                    tmp_pair.source = null;
                    tmp_pair.target = null;

                    for (int k = 0; k < schematic_nodes.Count; k++)
                    {
                       
                        for (int l = 0; l < schematic_nodes[k].connections.Count; l++)
                        {
                            if(schematic_nodes[k].connections[l].parent_node_id == source_node && schematic_nodes[k].connections[l].connection_id == source_con)
                            {
                                tmp_pair.source = schematic_nodes[k].connections[l];
                            }

                            if (schematic_nodes[k].connections[l].parent_node_id == target_node && schematic_nodes[k].connections[l].connection_id == target_con)
                            {
                                tmp_pair.target = schematic_nodes[k].connections[l];
                            }
                        }
                      
                    }
                    if (tmp_pair.source != null && tmp_pair.target != null)
                    {
                        connection_list.Add(tmp_pair);
                    }

                }

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



        //SCROLLING STUFF
        public int scroll_multiplier = 5;
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            pos_scoll_offset.Y = e.NewValue;


            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                schematic_nodes[i].pos.y += (e.NewValue - e.OldValue)* scroll_multiplier;
              
                schematic_nodes[i].create_drawable();
            }
        }
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            pos_scoll_offset.X = e.NewValue;
            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                schematic_nodes[i].pos.x += (e.NewValue - e.OldValue)* scroll_multiplier;

                schematic_nodes[i].create_drawable();
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        //CHANGE SCHEMATIC SLOT
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_insta_view();
            pictureBox1.Focus();

        }
        //UPDATE NODE INTA TREE VIEW
        public void update_insta_view()
        {
            treeView2.Nodes.Clear();

            for (int i = 0; i < schematic_nodes.Count; i++)
            {
                if (schematic_nodes[i].schematic_id == comboBox1.SelectedIndex)
                {
                    if(schematic_nodes[i] == null) { break; }
                    TreeNode inst_tree_node_tmp = new TreeNode();
                    inst_tree_node_tmp.Name = "inst_" + schematic_nodes[i].nid.ToString();
                    inst_tree_node_tmp.Text = schematic_nodes[i].title + ":" + schematic_nodes[i].nid.ToString();
                    treeView2.Nodes.Add(inst_tree_node_tmp);
                    inst_tree_node_tmp = null;
                }
            }
        }

        public void make_connection_reference()
        {
            for (int k = 0; k < schematic_nodes.Count; k++)
            {
                for (int l = 0; l < schematic_nodes[k].connections.Count; l++)
                {
                    schematic_nodes[k].connections[l].parent_node = schematic_nodes[k];
                }
            }
        }
        //SAVE PARAM BUTTON
        private void button4_Click(object sender, EventArgs e)
        {
            if(selected_history_node == null)
            {
                return;
            }

            selected_history_node.save_parameters(ref parameter_panel_form);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
