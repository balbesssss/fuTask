using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TaskSerrvice.Desctop
{
    public partial class Action : Form
    {
        private Guid Id { get; set; }
        public Action(Guid id)
        {
            Id = id;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var editForm = new EditForm(Id);
            editForm.Show();
            
        }
    }
}
