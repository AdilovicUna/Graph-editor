using System;
using System.Windows.Forms;

class Program{
    [STAThread]
    static void Main(){
        Form Form = new MyForm();
        Form.Text = "Untitled";
        Application.Run(Form);
    }
}
delegate void Notify();