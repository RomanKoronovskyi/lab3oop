using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace lab3
{
    public partial class MainForm : Form
    {
        private DataGridView gridView;
        private BindingSource bindingSource;
        private ILibraryDataService dataService;
        private List<JObject> libraryData;

        public MainForm()
        {
            this.dataService = new LibraryDataService();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "JSON Library Manager";
            this.Size = new System.Drawing.Size(800, 600);

            gridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var addMenu = new ToolStripMenuItem("Add");
            var editMenu = new ToolStripMenuItem("Edit");
            var deleteMenu = new ToolStripMenuItem("Delete");
            var saveMenu = new ToolStripMenuItem("Save");
            var aboutMenu = new ToolStripMenuItem("About");

            fileMenu.DropDownItems.Add(saveMenu);
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(addMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(deleteMenu);
            menuStrip.Items.Add(aboutMenu);

            saveMenu.Click += SaveMenu_Click;
            addMenu.Click += AddMenu_Click;
            editMenu.Click += EditMenu_Click;
            deleteMenu.Click += DeleteMenu_Click;
            aboutMenu.Click += AboutMenu_Click;

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            this.Controls.Add(gridView);

            bindingSource = new BindingSource();
            gridView.DataSource = bindingSource;
        }

        private void LoadData()
        {
            try
            {
                libraryData = dataService.LoadData();
                bindingSource.DataSource = libraryData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMenu_Click(object sender, EventArgs e)
        {
            try
            {
                dataService.SaveData(libraryData);
                MessageBox.Show("Data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddMenu_Click(object sender, EventArgs e)
        {
            var editForm = new EditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                dataService.AddData(libraryData, editForm.EditedData);
                bindingSource.ResetBindings(false);
            }
        }

        private void EditMenu_Click(object sender, EventArgs e)
        {
            if (gridView.CurrentRow != null)
            {
                var selectedData = (JObject)gridView.CurrentRow.DataBoundItem;
                var editForm = new EditForm(selectedData);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    dataService.EditData(libraryData, editForm.EditedData);
                    bindingSource.ResetBindings(false);
                }
            }
        }

        private void DeleteMenu_Click(object sender, EventArgs e)
        {
            if (gridView.CurrentRow != null)
            {
                var selectedData = (JObject)gridView.CurrentRow.DataBoundItem;
                dataService.DeleteData(libraryData, selectedData);
                bindingSource.ResetBindings(false);
            }
        }

        private void AboutMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "JSON Library Manager\n\nDeveloped by: Roman Koronovskyi\nCourse: 2\nGroup: K-23",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    public interface ILibraryDataService
    {
        List<JObject> LoadData();
        void SaveData(List<JObject> data);
        void AddData(List<JObject> data, JObject newData);
        void EditData(List<JObject> data, JObject editedData);
        void DeleteData(List<JObject> data, JObject dataToDelete);
    }

    public class LibraryDataService : ILibraryDataService
    {
        private readonly string filePath = "data.json";

        public List<JObject> LoadData()
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var jsonObject = JObject.Parse(json);
                var libraryData = (JArray)jsonObject["libraryDatabase"];
                return libraryData.ToObject<List<JObject>>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data", ex);
            }
        }

        public void SaveData(List<JObject> data)
        {
            try
            {
                var jsonObject = new JObject { ["libraryDatabase"] = JArray.FromObject(data) };
                File.WriteAllText(filePath, jsonObject.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving data", ex);
            }
        }

        public void AddData(List<JObject> data, JObject newData)
        {
            data.Add(newData);
        }

        public void EditData(List<JObject> data, JObject editedData)
        {
            var index = data.FindIndex(d => d["BK_ID"]?.ToString() == editedData["BK_ID"]?.ToString());
            if (index >= 0)
            {
                data[index] = editedData;
            }
        }

        public void DeleteData(List<JObject> data, JObject dataToDelete)
        {
            data.Remove(dataToDelete);
        }
    }

    public class EditForm : Form
    {
        public JObject EditedData { get; private set; }

        private TextBox nameTextBox;
        private TextBox infoTextBox;
        private TextBox idTextBox;
        private TextBox authorTextBox;
        private TextBox dcnameTextBox;

        public EditForm(JObject data = null)
        {
            EditedData = data ?? new JObject();
            InitializeComponent();

            if (data != null)
            {
                nameTextBox.Text = data["BK_NAME"]?.ToString();
                infoTextBox.Text = data["BK_INFO"]?.ToString();
                idTextBox.Text = data["BK_ID"]?.ToString();
                authorTextBox.Text = data["AU_NAME"]?.ToString();
                dcnameTextBox.Text = data["DC_NAME"]?.ToString();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Edit Entry";
            this.Size = new System.Drawing.Size(400, 300);

            var nameLabel = new Label { Text = "Name", Top = 20, Left = 10 };
            nameTextBox = new TextBox { Top = 20, Left = 100, Width = 250 };

            var infoLabel = new Label { Text = "Info", Top = 60, Left = 10 };
            infoTextBox = new TextBox { Top = 60, Left = 100, Width = 250 };

            var idLabel = new Label { Text = "ID", Top = 100, Left = 10 };
            idTextBox = new TextBox { Top = 100, Left = 100, Width = 250 };

            var authorLabel = new Label { Text = "Author", Top = 140, Left = 10 };
            authorTextBox = new TextBox { Top = 140, Left = 100, Width = 250 };

            var dcnameLabel = new Label { Text = "Name of discipline", Top = 180, Left = 10 };
            dcnameTextBox = new TextBox { Top = 180, Left = 100, Width = 250 };

            var saveButton = new Button { Text = "Save", Top = 220, Left = 100 };
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(nameTextBox);
            this.Controls.Add(nameLabel);
            this.Controls.Add(infoTextBox);
            this.Controls.Add(infoLabel);
            this.Controls.Add(idTextBox);
            this.Controls.Add(idLabel);
            this.Controls.Add(authorTextBox);
            this.Controls.Add(authorLabel);
            this.Controls.Add(dcnameTextBox);
            this.Controls.Add(dcnameLabel);
            this.Controls.Add(saveButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            EditedData["BK_NAME"] = nameTextBox.Text;
            EditedData["BK_INFO"] = infoTextBox.Text;
            EditedData["BK_ID"] = idTextBox.Text;
            EditedData["AU_NAME"] = authorTextBox.Text;
            EditedData["DC_NAME"] = dcnameTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}






