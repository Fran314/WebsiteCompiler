using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebsiteCompiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum SET {
            WINSCP_PATH,
            SOURCE_PATH,
            OUTPUT_PATH,
            SECOND_LANGUAGE
        };

        private Dictionary<SET, string> settings = null;

        private string selected_file_path = "";

        private string selected_var_path = "";
        private string selected_var_en_path = "";

        public MainWindow()
        {
            readSettings();
            InitializeComponent();

            winscpTextBox.Text = settings[SET.WINSCP_PATH];
            sourceTextBox.Text = settings[SET.SOURCE_PATH];
            outputTextBox.Text = settings[SET.OUTPUT_PATH];
            enCheckBox.IsChecked = s2b(settings[SET.SECOND_LANGUAGE]);

            initializeProjectTreeView();
            initializeBlocksListView();
        }

        private void createNewSettings()
        {
            settings = new Dictionary<SET, string>();

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\source");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\output");

            settings.Add(SET.WINSCP_PATH, @"C:\Program Files (x86)\WinSCP\WinSCP.com");
            settings.Add(SET.SOURCE_PATH, Directory.GetCurrentDirectory() + "\\source");
            settings.Add(SET.OUTPUT_PATH, Directory.GetCurrentDirectory() + "\\output");
            settings.Add(SET.SECOND_LANGUAGE, "FALSE");
        }
        private void readSettings()
        {
            FileInfo fi = new System.IO.FileInfo(@"settings.bin");
            if(fi.Exists)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                FileStream binaryFile = fi.OpenRead();

                settings = (Dictionary<SET, string>)binaryFormatter.Deserialize(binaryFile);

                binaryFile.Close();
            }
            else
            {
                createNewSettings();
                saveSettings();
            }
        }
        private void saveSettings()
        {
            if(settings == null)
                createNewSettings();

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo fi = new System.IO.FileInfo(@"settings.bin");
            FileStream binaryFile = fi.Create();

            binaryFormatter.Serialize(binaryFile, settings);
            binaryFile.Flush();

            binaryFile.Close();
        }

        private void winscpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                settings[SET.WINSCP_PATH] = ofd.FileName;
                winscpTextBox.Text = settings[SET.WINSCP_PATH];
                saveSettings();
            }
        }
        private void sourceButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                settings[SET.SOURCE_PATH] = fbd.SelectedPath;
                sourceTextBox.Text = settings[SET.SOURCE_PATH];
                saveSettings();
                initializeProjectTreeView();
            }
        }
        private void outputButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                settings[SET.OUTPUT_PATH] = fbd.SelectedPath;
                outputTextBox.Text = settings[SET.OUTPUT_PATH];
                saveSettings();
            }
        }
        private void enCheckBox_Toggled(object sender, RoutedEventArgs e)
        {
            settings[SET.SECOND_LANGUAGE] = b2s(enCheckBox.IsChecked == true);
            saveSettings();
        }

        private void initializeProjectTreeView()
        {
            if (settings == null) return;

            projectTreeView.Items.Clear();
            projectTreeView.Items.Add(newFolderTreeViewItem(settings[SET.SOURCE_PATH]));
        }
        private void initializeBlocksListView()
        {
            string blocks_folder = Directory.GetCurrentDirectory() + "\\blocks";
            if(Directory.Exists(blocks_folder))
            {
                try
                {
                    foreach (string file in Directory.GetFiles(blocks_folder))
                        blocksListView.Items.Add(newFileListViewItem(file));
                }
                catch (Exception) { }
                
            }
            else
            {
                Directory.CreateDirectory(blocks_folder);
            }
        }

        private void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                try
                {
                    foreach (string file_path in Directory.GetFiles(item.Tag.ToString()))
                        item.Items.Add(newFileTreeViewItem(file_path));

                    foreach (string folder_path in Directory.GetDirectories(item.Tag.ToString()))
                        item.Items.Add(newFolderTreeViewItem(folder_path));
                }
                catch (Exception) { }
            }
        }
        private void file_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem file_item = sender as TreeViewItem;

            if(selected_file_path != "")
            {
                File.WriteAllText(selected_file_path, fileTextBox.Text);
            }

            selected_file_path = file_item.Tag.ToString();
            fileTextBox.Text = File.ReadAllText(selected_file_path);
        }
        private void block_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 2)
            {
                TextBlock file_item = sender as TextBlock;

                if (selected_file_path != "")
                {
                    File.WriteAllText(selected_file_path, fileTextBox.Text);
                }

                selected_file_path = file_item.Tag.ToString();
                fileTextBox.Text = File.ReadAllText(selected_file_path);
            }
        }

        private TreeViewItem newFolderTreeViewItem(string path)
        {
            TreeViewItem item = new TreeViewItem();

            StackPanel stack = new StackPanel();
            stack.Orientation = System.Windows.Controls.Orientation.Horizontal;

            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/folder.png"));
            image.Width = 16;
            image.Height = 16;

            System.Windows.Controls.Label lbl = new System.Windows.Controls.Label();
            lbl.Content = path.Substring(path.LastIndexOf("\\") + 1);

            stack.Children.Add(image);
            stack.Children.Add(lbl);

            item.Header = stack;
            item.Tag = path;
            item.Items.Add(null);
            item.Expanded += new RoutedEventHandler(folder_Expanded);

            return item;
        }
        private TreeViewItem newFileTreeViewItem(string path)
        {
            TreeViewItem item = new TreeViewItem();

            StackPanel stack = new StackPanel();
            stack.Orientation = System.Windows.Controls.Orientation.Horizontal;

            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/file.png"));
            image.Width = 16;
            image.Height = 16;

            System.Windows.Controls.Label lbl = new System.Windows.Controls.Label();
            lbl.Content = path.Substring(path.LastIndexOf("\\") + 1);

            stack.Children.Add(image);
            stack.Children.Add(lbl);

            item.Header = stack;
            item.Tag = path;
            item.MouseDoubleClick += new MouseButtonEventHandler(file_MouseDoubleClick);

            return item;
        }
        private TextBlock newFileListViewItem(string path)
        {
            TextBlock item = new TextBlock();
            string s = path.Substring(path.LastIndexOf("\\") + 1);

            item.Text = path.Substring(path.LastIndexOf("\\") + 1);
            item.Tag = path;
            item.FontWeight = FontWeights.Normal;
            item.MouseLeftButtonDown += new MouseButtonEventHandler(block_MouseLeftButtonDown);

            return item;
        }

        private void fileTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string var_name = fileTextBox.Text.Substring(fileTextBox.SelectionStart, fileTextBox.SelectionLength);
            if(fileTextBox.SelectionStart > 0 && fileTextBox.SelectionStart + fileTextBox.SelectionLength < fileTextBox.Text.Length)
            {
                if (!isAlphanumeric(var_name))
                    return;

                if(fileTextBox.Text[fileTextBox.SelectionStart - 1] == '$')
                {
                    if (fileTextBox.Text[fileTextBox.SelectionStart + fileTextBox.SelectionLength] != '$')
                        return;

                    string var_folder = Directory.GetCurrentDirectory() + "\\variables";
                    if (!Directory.Exists(var_folder))
                        Directory.CreateDirectory(var_folder);

                    if(selected_var_path != "")
                    {
                        File.WriteAllText(selected_var_path, normalVarContent.Text);
                    }
                    if(selected_var_en_path != "")
                    {
                        File.WriteAllText(selected_var_en_path, enVarContent.Text);
                    }

                    selected_var_path = var_folder + "\\" + var_name + ".var";
                    selected_var_en_path = var_folder + "\\" + var_name + "-en.var";

                    varNameTextBlock.Text = var_name;

                    if (File.Exists(selected_var_path))
                    {
                        normalVarContent.Text = File.ReadAllText(selected_var_path);
                    }
                    else
                    {
                        File.Create(selected_var_path);
                        normalVarContent.Text = "";
                    }

                    if (File.Exists(selected_var_en_path))
                    {
                        enVarContent.Text = File.ReadAllText(selected_var_en_path);
                    }
                    else
                    {
                        File.Create(selected_var_en_path);
                        enVarContent.Text = "";
                    }
                }
            }
        }

        private bool isAlphanumeric(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (!isAlphanumeric(s[i]))
                    return false;

            return true;
        }
        private bool isAlphanumeric(char c)
        {
            if (c >= 'a' && c <= 'z')
                return true;
            if (c >= 'A' && c <= 'Z')
                return true;
            if (c >= '0' && c <= '9')
                return true;

            return false;
        }

        private void normalVarContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int new_top_margin = (int)normalVarContent.Margin.Top + (int)normalVarContent.ActualHeight + 13;
            enVarContent.Margin = new Thickness(enVarContent.Margin.Left, new_top_margin, enVarContent.Margin.Right, enVarContent.Margin.Bottom);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void save()
        {
            if (selected_file_path != "")
            {
                File.WriteAllText(selected_file_path, fileTextBox.Text);
            }

            if (selected_var_path != "")
            {
                File.WriteAllText(selected_var_path, normalVarContent.Text);
            }
            if (selected_var_en_path != "")
            {
                File.WriteAllText(selected_var_en_path, enVarContent.Text);
            }
        }

        private void compile()
        {
            List<string> files = new List<string>();
            List<string> folders = new List<string>();
            folders.Add(settings[SET.SOURCE_PATH]);
            int source_path_length = settings[SET.SOURCE_PATH].Length;

            while(folders.Count > 0)
            {
                string current_folder = folders[0];
                folders.RemoveAt(0);

                folders.AddRange(Directory.GetDirectories(current_folder));
                foreach(string file_path in Directory.GetFiles(current_folder))
                {
                    string relative_file_path = file_path.Substring(source_path_length+1);
                    if (relative_file_path.Substring(relative_file_path.LastIndexOf(".") + 1).Equals("html"))
                        files.Add(relative_file_path);
                }
            }

            foreach(string file_path in files)
            {
                StringBuilder outputString1 = new StringBuilder();
                StringBuilder outputString2 = new StringBuilder();

                string folder_parent = "", file_name;

                if (file_path.LastIndexOf('\\') != -1)
                {
                    folder_parent = "\\" + file_path.Substring(0, file_path.LastIndexOf('\\'));
                    file_name = file_path.Substring(file_path.LastIndexOf('\\') + 1);
                }
                else
                    file_name = file_path;

                string inputString = File.ReadAllText(settings[SET.SOURCE_PATH] + folder_parent + "\\" + file_name);

                for(int i = 0; i < inputString.Length; i++)
                {
                    if (inputString[i] == '$')
                    {
                        int j = 1;
                        while (j + i < inputString.Length && inputString[j + i] != '$')
                            j++;

                        if(i + j < inputString.Length)
                        {
                            string var_name = inputString.Substring(i + 1, j - 1);
                            if (isAlphanumeric(var_name))
                            {
                                if(File.Exists(Directory.GetCurrentDirectory() + "\\variables\\" + var_name + ".var"))
                                {
                                    outputString1.Append(File.ReadAllText(Directory.GetCurrentDirectory() + "\\variables\\" + var_name + ".var"));
                                    if(enCheckBox.IsChecked == true)
                                        outputString2.Append(File.ReadAllText(Directory.GetCurrentDirectory() + "\\variables\\" + var_name + "-en.var"));
                                }
                            }
                            else
                            {
                                outputString1.Append('$' + var_name + '$');
                                if (enCheckBox.IsChecked == true)
                                    outputString2.Append('$' + var_name + '$');
                            }
                            i += j;
                        }
                    }
                    else if (inputString[i] == '£')
                    {
                        int j = 1;
                        while (j + i < inputString.Length && inputString[j + i] != '£')
                            j++;

                        if (i + j < inputString.Length)
                        {
                            string block_name = inputString.Substring(i + 1, j - 1);
                            if (isAlphanumeric(block_name))
                            {
                                outputString1.Append(getBlock(block_name, false));
                                if (enCheckBox.IsChecked == true)
                                    outputString2.Append(getBlock(block_name, true));
                            }
                            else
                            {
                                outputString1.Append('£' + block_name + '£');
                                if (enCheckBox.IsChecked == true)
                                    outputString2.Append('£' + block_name + '£');
                            }
                            i += j;
                        }
                    }
                    else
                    {
                        outputString1.Append(inputString[i]);
                        if (enCheckBox.IsChecked == true)
                            outputString2.Append(inputString[i]);
                    }
                }

                Directory.CreateDirectory(settings[SET.OUTPUT_PATH] + folder_parent);
                if (enCheckBox.IsChecked == true)
                    Directory.CreateDirectory(settings[SET.OUTPUT_PATH] + folder_parent + "\\en");

                File.WriteAllText(settings[SET.OUTPUT_PATH] + folder_parent + "\\" + file_name, outputString1.ToString());
                if (enCheckBox.IsChecked == true)
                    File.WriteAllText(settings[SET.OUTPUT_PATH] + folder_parent + "\\en\\" + file_name, outputString2.ToString());
            }
        }
        private string getBlock(string block_name, bool isEN)
        {
            StringBuilder outputString = new StringBuilder();
            string inputString = File.ReadAllText(Directory.GetCurrentDirectory() + "\\blocks\\" + block_name + ".block");

            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] == '$')
                {
                    int j = 1;
                    while (j + i < inputString.Length && inputString[j + i] != '$')
                        j++;

                    if (i + j < inputString.Length)
                    {
                        string var_name = inputString.Substring(i + 1, j - 1);
                        if (isAlphanumeric(var_name))
                        {
                            if (isEN)
                                var_name += "-en";
                            if (File.Exists(Directory.GetCurrentDirectory() + "\\variables\\" + var_name + ".var"))
                            {
                                outputString.Append(File.ReadAllText(Directory.GetCurrentDirectory() + "\\variables\\" + var_name + ".var"));
                            }
                        }
                        else
                        {
                            outputString.Append('$' + var_name + '$');
                        }
                        i += j;
                    }
                }
                else
                    outputString.Append(inputString[i]);
            }

            return outputString.ToString();
        }

        private void compileButton_Click(object sender, RoutedEventArgs e)
        {
            save();
            compile();
        }

        private void compileAndUploadButton_Click(object sender, RoutedEventArgs e)
        {
            save();
            compile();

            string bat_string = "\"" + settings[SET.WINSCP_PATH] + "\" " +
                "/console /ini=nul /script=\"" +
                Directory.GetCurrentDirectory() + "\\script.txt\" " +
                "/parameter \"" + settings[SET.OUTPUT_PATH] + "\"";

            File.WriteAllText(Directory.GetCurrentDirectory() + "\\temp.bat", bat_string);

            Process proc = null;
            try
            {
                string batDir = Directory.GetCurrentDirectory();
                proc = new Process();
                proc.StartInfo.WorkingDirectory = batDir;
                proc.StartInfo.FileName = "temp.bat";
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace.ToString());
            }

            File.Delete(Directory.GetCurrentDirectory() + "\\temp.bat");
        }

        private bool s2b (string arg)
        {
            return arg.Equals("TRUE");
        }
        private string b2s(bool arg)
        {
            if (arg)
                return "TRUE";
            else
                return "FALSE";
        }
    }
}
