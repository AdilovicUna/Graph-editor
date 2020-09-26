using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

class MyForm : Form {
    Dictionary<int, Point> pos = new Dictionary<int, Point>();  // position of each vertex
    Graph graph = new Graph();
    int selected;
    bool isMouseDown;
    bool temp1;
    string path;

#region Menu
    public MyForm(){
        graph.changed += Invalidate;
        ClientSize = new Size(500, 500);
        ToolStripMenuItem[] fileItems = {
            new ToolStripMenuItem("New", null, New),
            new ToolStripMenuItem("Open", null, Open),
            new ToolStripMenuItem("Save", null, Save),
            new ToolStripMenuItem("Quit", null, Quit)
        };
        
        ToolStripMenuItem[] topItems = {
            new ToolStripMenuItem("File", null, fileItems)
        };

        MenuStrip strip = new MenuStrip();
        foreach (var item in topItems)
            strip.Items.Add(item);

        Controls.Add(strip);   
    }
    void New(object sender, EventArgs args){
        graph.deleteAll();
        pos.Clear();
        selected = 0;
        Invalidate();
    }
    void Open(object sender, EventArgs args){
        pos.Clear();
        graph.deleteAll();
        string inFile = string.Empty;
        OpenFileDialog openFile = new OpenFileDialog();
        using (openFile){
            if(openFile.ShowDialog() == DialogResult.OK){
                path = openFile.FileName;
                StreamReader reader = new StreamReader(openFile.OpenFile());
                using(reader){
                    bool open = false;
                    Read(reader, open);
                }
                Text = Path.GetFileNameWithoutExtension(path);
                Invalidate();
                
            }
        }
    }
    void Save(object sender, EventArgs args){
        if (File.Exists(path)){
            using(StreamWriter writer = new StreamWriter(File.Create(path))){
                Write(writer);
            }
        }else{
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "SaveFile";
            using(save){
                if(save.ShowDialog() == DialogResult.OK){
                    string path = save.FileName;
                    using(StreamWriter writer = new StreamWriter(File.Create(path))){
                        Write(writer);
                    }
                    Text = Path.GetFileNameWithoutExtension(path);
                    Invalidate();
                }
            }
        }
    }
    void Quit(object sender, EventArgs args){
        Application.Exit();
    }
#endregion

#region Protected override
    protected override void OnMouseDown(MouseEventArgs args) {
        int addVertex;
        if (Control.ModifierKeys == Keys.Shift){
            addVertex = graph.addVertex(0);
            selected = addVertex;
            pos.Add(addVertex, new Point(args.X,args.Y)); 
        }else{
            foreach (KeyValuePair<int,Point> entry in pos){ 
                if (InVertex(entry.Value.X, entry.Value.Y, 15, args.X, args.Y)){ 
                    temp1 = true;
                    isMouseDown = true;
                    if (Control.ModifierKeys == Keys.Control && selected != 0){
                        if (graph.areConnected(selected, entry.Key)){
                            graph.disconnect(selected, entry.Key);
                        }else{
                            addToDict(selected, entry.Key);
                        }
                    }else{
                    selected= entry.Key;
                    }
                }
            }
            if (!temp1){
                selected = 0; // nothing is selected
            }
        }
        Invalidate();
    }
    protected override void OnMouseMove(MouseEventArgs args){
        if (isMouseDown == true && Control.ModifierKeys != Keys.Control){
            pos[selected] = new Point(args.X,args.Y);
            Invalidate();
        }
    }
    protected override void OnMouseUp(MouseEventArgs args)
        {
            isMouseDown = false;
        }
    protected override void OnKeyDown(KeyEventArgs args){
        if (Keys.Delete == args.KeyCode && selected != 0){
            List<int> temp = new List<int>();
            foreach (int v in graph.vertices()) {
                if (graph.areConnected(v, selected)) {
                temp.Add(v);
            }
        }

        foreach (int i in temp){
            graph.disconnect(i,selected);
        }
        pos.Remove(selected);
        graph.delete(selected);
        selected = 0;
        Invalidate();
        }
    }
    protected override void OnPaint(PaintEventArgs args) {
        Graphics g = args.Graphics;
        Edge(args, g);
        Vertex(args, g);
    }
#endregion

#region Edge
    void Edge(PaintEventArgs args, Graphics g){
        Point center1, center2;
        Pen p2 = new Pen (Brushes.Black, 2);
        foreach (int i in graph.vertices()){
            foreach (int j in graph.neighbors(i))
            {
                center1 = new Point (pos[i].X , pos[i].Y);
                center2 = new Point(pos[j].X, pos[j].Y);
                g.DrawLine (p2, center1.X, center1.Y, center2.X, center2.Y);
                drawArrow(g, center1, center2, p2);
            }
        }
        p2.Dispose();
    }
    void drawArrow(Graphics g, Point center1, Point center2, Pen p2){
        Vector AB = new Vector (center1.X - center2.X, center1.Y - center2.Y); // vector from A to B
        double v = Math.Sqrt( Math.Pow(AB.X,2) + Math.Pow(AB.Y,2) ); // ||AB||
        Vector V = new Vector ( AB.X / v , AB.Y / v ); // unit vector of AB
        Vector W = new Vector ((-1) * V.Y, V.X); //normal on V

        Vector tip = new Vector (center2.X + 15 * V.X, center2.Y + 15 * V.Y); // intersection btw vertex and edge 
        
        double cx1 = center2.X + 30 * V.X + 15/2 * W.X; // A + CV + DW  // C = 2*R, D = R/2
        double cy1 = center2.Y + 30 * V.Y + 15/2 * W.Y;
        Vector corner1 = new Vector (cx1, cy1);
        
        double cx2 = center2.X + 30 * V.X - 15/2 * W.X; // A + CV - DW  // C = 2*R, D = R/2
        double cy2 = center2.Y + 30 * V.Y - 15/2 * W.Y;
        Vector corner2 = new Vector (cx2, cy2);
        
        g.DrawLine (p2, (int) corner1.X, (int) corner1.Y,  (int)tip.X, (int)tip.Y);
        g.DrawLine (p2, (int)corner2.X, (int)corner2.Y, (int)tip.X, (int)tip.Y);
    }
#endregion

#region  Vertex
    void Vertex(PaintEventArgs args, Graphics g){
        Font f = new Font ("Arial Black", 12);
        StringFormat format = new StringFormat();
        format.LineAlignment = StringAlignment.Center;
        format.Alignment = StringAlignment.Center;
        foreach (KeyValuePair<int,Point> entry in pos){
            int x = entry.Value.X - 15;
            int y = entry.Value.Y - 15;
            int id = entry.Key;
            if (id == selected){ // selected vertex
                drawVertex(g, x, y, id, f, format,Brushes.Black, Brushes.Black , Brushes.White);
                temp1 = false;
            }else{ // not selected vertices
             drawVertex(g, x, y, id, f, format, Brushes.Black, Brushes.White, Brushes.Black); 
            }
        }
        f.Dispose();
    }
    void drawVertex(Graphics g, int x, int y, int id, Font f, StringFormat format, Brush b0, Brush b1, Brush b2){
        Pen p1 = new Pen (b0, 4); 
        Rectangle rect = new Rectangle (x, y, 30, 30);
        g.DrawEllipse(p1, x, y, 30, 30);
        g.FillEllipse(b1, x, y, 30, 30);
        g.DrawString (id.ToString(), f, b2, rect, format);
        p1.Dispose();
    }
#endregion

#region Helper functions
    public static bool InVertex (int xc, int yc, int r, int x, int y) {
        int dx = xc-x;
        int dy = yc-y;
        return dx*dx+dy*dy <= r*r;
    }
    void addToDict(int id1, int id2){
        if (graph.contains(id1)){
            graph.connect(id1, id2);
        }else{
            graph.addVertex(id1);
           
        } graph.connect(id1, id2);
        
    }
    void Write(StreamWriter writer){
        String s = "";
        foreach(KeyValuePair<int, Point> entry in pos){
                    writer.WriteLine(entry.Key + " " + entry.Value.X  + " " + entry.Value.Y);
                }
                writer.WriteLine("###");
                foreach(int i in graph.vertices()){
                    s += i + " "; 
                    foreach(int j in graph.neighbors(i)){
                        s += j + " ";
                    }
                    writer.WriteLine(s);
                    s = "";
                }
    }
    void Read(StreamReader reader, bool open){
        while(reader.ReadLine() is string s){
            if(s == ""){
                break;
            }else if(s == "###"){
                open = true;
            }else{
                string[] items = s.Trim().Split();
                int number = int.Parse(items[0]);
                if(!open){
                    int x = int.Parse(items[1]);
                    int y = int.Parse(items[2]);
                    pos.Add(number, new Point(x,y));
                    graph.addVertex(number);
                }else{
                    for(int i = 1; i < items.Length; i++){
                        addToDict(number, int.Parse(items[i]));
                    }
                }
            }
        }
    }
#endregion
}