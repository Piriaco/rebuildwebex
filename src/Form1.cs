using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
    
namespace rebuild
{  
    public partial class Form1 : Form
    {
        private DataTable dt = new DataTable();
        DataRow dr;
        List<FileItems> containerList = new List<FileItems>();

        public Form1()
        {
            InitializeComponent();
        }

        string GetLastPart(string name)
        {
            int x = 0;
            int p = 0;
            for (p = name.IndexOf("_", x); p != -1; x++)
                p = name.IndexOf("_", x);
            
            return name.Substring(x);
        }

        void ClearLog()
        {
            textLog.Clear();
            textLog.Text = "Webex Rebuild Tool v0.2\r\n======================\r\n\r\n1) Select the directory containing the WebEx session. If you know the path write it directly.\r\n\r2) Click the 'Go' button\r\n3) The tool will generate the file rebuild.arf\n";
        }

        void ToLog(string text)
        {
            textLog.Text = textLog.Text + "\r\n" + text;
            textLog.SelectionStart = textLog.TextLength;
            textLog.SelectionLength = 1;
            textLog.ScrollToCaret();
        }

        void Rebuild(List<FileItems> inputFiles, string outputFile)
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Number of sections of the .arf file
                    int nsections = inputFiles.Count;
                    String nsections_hex = nsections.ToString("X");

                    BinaryReader reader = new BinaryReader(stream);
                    ARF_HEADER arf_head = new ARF_HEADER
                    {
                        e_magic = 0x00020001,
                        e_unknown = 0x00000000,
                        e_filesize = 0xadafea,
                        e_reserved0 = 0,
                        e_nsections = UInt32.Parse(nsections_hex, System.Globalization.NumberStyles.AllowHexSpecifier),
                        e_reserved1 = 0
                    };

                    int video = 0;
                    ARF_ITEMS[] arf_item = new ARF_ITEMS[arf_head.e_nsections];
                    long Offset = Marshal.SizeOf(typeof(ARF_ITEMS)) * arf_head.e_nsections + Marshal.SizeOf(typeof(ARF_HEADER));
                    for (int x = 0; x < inputFiles.Count; x++)
                    {
                        FileInfo f = new FileInfo(inputFiles[x].fileName);
                        FileSystemInfo f1 = new FileInfo(inputFiles[x].fileName);

                        FileItems.SegmentType id = inputFiles[x].id;
                        arf_item[x].e_id = (uint)id;
                        arf_item[x].e_sectionoffset = (uint)Offset;
                        arf_item[x].e_sectionlen = (uint)f.Length;

                        arf_item[x].e_index = 0;

                        if (id == FileItems.SegmentType.video || id == FileItems.SegmentType.video_idx)
                            arf_item[x].e_index = (uint)video;
                        if (id == FileItems.SegmentType.video_idx)
                            video++;

                        arf_item[x].e_reserved1 = 0;
                        arf_item[x].e_reserved2 = 0;
                        arf_item[x].e_reserved3 = 0;
                        arf_item[x].e_reserved4 = 0;
                        Offset += f.Length + 1;
                    }

                    arf_head.e_filesize = (uint)(Offset - 1);
                    writer.Write(GetBytes(arf_head));
                    for (int x = 0; x < arf_head.e_nsections; x++)
                        writer.Write(GetBytes(arf_item[x]));
                    Offset = 0;

                    foreach (FileItems file in inputFiles)
                    {
                        FileStream streamOrigin = new FileStream(file.fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                        BinaryReader readerOrigin = new BinaryReader(streamOrigin);

                        try
                        {
                            var bytes = readerOrigin.ReadBytes((int)streamOrigin.Length);
                            writer.Write(bytes);
                        }
                        catch (OutOfMemoryException)
                        {
                            MessageBox.Show("Error", "The program just ran out of memory!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            MessageBox.Show("Info", "This may be fixed someday...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Application.Exit();
                        }

                        Offset += streamOrigin.Length;
                            //long  pad = (Offset % 4);
                            //if (pad!=0)
                        writer.Write((byte)0);//x);

                    }
                    writer.Flush();
                    ToLog("[=] Re-constructed file: " + outputFile);
                    MessageBox.Show("Reconstruction completed", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    writer.Dispose();
                    writer.Close();                   
                }
            }
            catch (IOException)
            {
                MessageBox.Show("The directory is not valid", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            finally
            {
                if (stream != null)
                    stream.Dispose();
            }        
        }

        private DataRow AddFileToGrid(string archive)
        {
           
            FileInfo fileInfo = new FileInfo(archive);
            FileSystemInfo fileSysInfo = new FileInfo(archive);
            dr = dt.NewRow();
            //Get File name of each file name
            dr["Real_Filename"] = fileSysInfo.Name;
            dr["File_Name"] = GetLastPart(fileSysInfo.Name);
            //Get File Type/Extension of each file 
            dr["File_Type"] = fileSysInfo.Extension;
            //Get File Size of each file in KB format
            dr["File_Size"] = (fileInfo.Length).ToString();
            dr["Create_Date"] = (fileSysInfo.CreationTime).ToString();
            return dr;
        }

        bool FillDataGrid(string path)
        {
            containerList.Clear();
            dt.Clear();
            dt.Columns.Clear();

            //Add Data Grid Columns with name
            dt.Columns.Add("File_Name");       
            dt.Columns.Add("Real_Filename");
            dt.Columns.Add("File_Type");
            dt.Columns.Add("File_Size");
            dt.Columns.Add("Create_Date");

            if (Directory.Exists(path))
            {
                SearchOption topDir = SearchOption.TopDirectoryOnly;

                string[] Files_CFG = Directory.GetFiles(path, "*.conf", topDir);

                string[] Files_STD = Directory.GetFiles(path, "*.std", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();

                string[] Files_WAV = Directory.GetFiles(path, "*.wav", topDir);

                string[] Files_VID = Directory.GetFiles(path, "*_4_*.dat", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();
                string[] Files_VID_IDX = Directory.GetFiles(path, "*_4_*.idx", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();

                string[] Files_FINMM = Directory.GetFiles(path, "*_6_*.dat", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();
                string[] Files_FINMM_IDX = Directory.GetFiles(path, "*_6_*.idx", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();
                string[] Files_FINMM_CAD = Directory.GetFiles(path, "*_6_*.cad", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();
                string[] Files_FINMM_CAI = Directory.GetFiles(path, "*_6_*.cai", topDir).OrderBy(o => new FileInfo(o).Name).ToArray();

                string[] Files_SND = Directory.GetFiles(path, "*_20_*.dat", topDir);
                string[] Files_SND_IDX = Directory.GetFiles(path, "*_20_*.idx", topDir);

                string[] Files_BACKUP = Directory.GetFiles(path, "*_21_*.dat", topDir);
                string[] Files_BACKUP_IDX = Directory.GetFiles(path, "*_21_*.idx", topDir);

                // Check having at least one wav file and one conf file
                if (Files_CFG.Length != 1 /* || Files_WAV.Length != 1*/)
                {
                    MessageBox.Show("Something failed, WebEx format may have changed");
                    return false;
                }
                else
                {
                    // If the file exists we add chat
                    if (Files_STD.Length > 0)
                    {
                        ToLog("[+] Chat file: " + Path.GetFileName(Files_STD[0]));
                        containerList.Add(new FileItems(Files_STD[0], FileItems.SegmentType.chat));
                        dt.Rows.Add(AddFileToGrid(Files_STD[0]));

                        if (Files_STD.Length > 1)
                        {
                            ToLog("[+] file file: " + Path.GetFileName(Files_STD[1]));
                            containerList.Add(new FileItems(Files_STD[1], FileItems.SegmentType.file));
                            dt.Rows.Add(AddFileToGrid(Files_STD[1]));
                        }
                    }
                    else
                    {
                        ToLog("[--] Chat file: Not found.");
                    }

                    // Add the .CFG file
                    if (Files_CFG.Length > 0)
                    {
                        ToLog("[+] Config file: " + Path.GetFileName(Files_CFG[0]));
                        containerList.Add(new FileItems(Files_CFG[0], FileItems.SegmentType.cfg ));
                        dt.Rows.Add(AddFileToGrid(Files_CFG[0]));
                    }
                    else
                    {
                        ToLog("[--] Config file: Not found.");
                    }

                    // We add segment index Video and Video
                    if (Files_VID.Length > 0 && Files_VID_IDX.Length > 0 && Files_VID.Length == Files_VID_IDX.Length)
                    {
                        for (int x = 0; x < Files_VID.Length; x++)
                        {
                            // Add Video
                            ToLog(string.Format(@"[+] Video file {0}/{1} : {2}", x + 1, Files_VID.Length, Path.GetFileName(Files_VID[x].ToString())));

                            containerList.Add(new FileItems(Files_VID[x], FileItems.SegmentType.video ));
                            dt.Rows.Add(AddFileToGrid(Files_VID[x]));

                            // Add Video index
                            ToLog(string.Format(@"[+] Index file {0}/{1} : {2}", x + 1, Files_VID_IDX.Length, Path.GetFileName(Files_VID[x].ToString())));
                            containerList.Add(new FileItems(Files_VID_IDX[x], FileItems.SegmentType.video_idx ));
                            dt.Rows.Add(AddFileToGrid(Files_VID_IDX[x]));
                        }
                    }
                    else
                    {
                        if (Files_VID.Length == 0)
                            ToLog("[--] Video file: Not found.");
                        if (Files_VID_IDX.Length == 0)
                            ToLog("[--] Index file: Not found.");
                        if (Files_VID.Length != Files_VID_IDX.Length)
                            ToLog("[--] video or index file not pairs.");
                    }

                    // We add segment index Sound and Sound
                    if (Files_SND.Length > 0 && Files_SND_IDX.Length > 0 && Files_SND.Length == Files_SND_IDX.Length)
                    {
                        for (int x = 0; x < Files_SND.Length; x++)
                        {
                            // Add Sound
                            ToLog(string.Format(@"[+] Sound file {0}/{1} : {2}", x + 1, Files_SND.Length, Path.GetFileName(Files_SND[x].ToString())));

                            containerList.Add(new FileItems(Files_SND[x], FileItems.SegmentType.snd));
                            dt.Rows.Add(AddFileToGrid(Files_SND[x]));

                            // Add Sound index
                            ToLog(string.Format(@"[+] Index file {0}/{1} : {2}", x + 1, Files_SND_IDX.Length, Path.GetFileName(Files_SND[x].ToString())));
                            containerList.Add(new FileItems(Files_SND_IDX[x], FileItems.SegmentType.snd_idx));
                            dt.Rows.Add(AddFileToGrid(Files_SND_IDX[x]));
                        }
                    }
                    else
                    {
                        if (Files_SND.Length == 0)
                            ToLog("[--] Sound file: Not found.");
                        if (Files_SND_IDX.Length == 0)
                            ToLog("[--] Index file: Not found.");
                        if (Files_SND.Length != Files_SND_IDX.Length)
                            ToLog("[--] sound or index file not pairs.");
                    }

                    // Add WAV
                    if (Files_WAV.Length == 1)
                    {
                        ToLog("[+] Wav file: " + Path.GetFileName(Files_WAV[0]));
                        dt.Rows.Add(AddFileToGrid(Files_WAV[0]));
                        containerList.Add(new FileItems(Files_WAV[0], FileItems.SegmentType.wav ));
                    }
                    else
                    {
                        if (Files_WAV.Length == 0)
                            ToLog("[--] Wav file: Not found.");
                        else
                            ToLog("[--] Wav file: Too many wav files.");
                    }

                    // Add FIN_MM
                    if (Files_FINMM.Length == 1)
                    {
                        ToLog("[+] MM_END file: " + Path.GetFileName(Files_FINMM[0]));
                        dt.Rows.Add(AddFileToGrid(Files_FINMM[0]));
                        containerList.Add(new FileItems(Files_FINMM[0], FileItems.SegmentType.mmfin) );
                    }
                    else
                    {
                        if (Files_FINMM.Length == 0)
                            ToLog("[--] MM_END file: Not found.");
                        else
                            ToLog("[--] MM_END file: Too many wav files.");
                    }
                    // Add FIN_MM_IDX
                    if (Files_FINMM_IDX.Length == 1)
                    {
                        ToLog("[+] MM_IDX file: " + Path.GetFileName(Files_FINMM_IDX[0]));
                        dt.Rows.Add(AddFileToGrid(Files_FINMM_IDX[0]));
                        containerList.Add(new FileItems(Files_FINMM_IDX[0], FileItems.SegmentType.mmfin_idx) );
                    }
                    else
                    {
                        if (Files_FINMM_IDX.Length == 0)
                            ToLog("[--] MM_IDX end file: Not found.");
                        else
                            ToLog("[--] MM_IDX end file: Too many MM_IDX files.");
                    }
                    if (Files_FINMM_CAD.Length == 1)
                    {
                        ToLog("[+] MM_CAD file: " + Path.GetFileName(Files_FINMM_CAD[0]));
                        dt.Rows.Add(AddFileToGrid(Files_FINMM_CAD[0]));
                        containerList.Add(new FileItems(Files_FINMM_CAD[0], FileItems.SegmentType.mmfin_cad));
                    }
                    if (Files_FINMM_CAI.Length == 1)
                    {
                        ToLog("[+] MM_CAI file: " + Path.GetFileName(Files_FINMM_CAI[0]));
                        dt.Rows.Add(AddFileToGrid(Files_FINMM_CAI[0]));
                        containerList.Add(new FileItems(Files_FINMM_CAI[0], FileItems.SegmentType.mmfin_cai));
                    }

                    // Añadimos BACKUP
                    if (Files_BACKUP.Length == 1)
                    {
                        ToLog("[+] BACKUP file: " + Path.GetFileName(Files_BACKUP[0]));
                        dt.Rows.Add(AddFileToGrid(Files_BACKUP[0]));
                        containerList.Add(new FileItems(Files_BACKUP[0],  FileItems.SegmentType.backup) );
                    }
                    else
                    {
                        if (Files_BACKUP.Length == 0)
                            ToLog("[--] BACKUP file: Not found.");
                        else
                            ToLog("[--] BACKUP file: Too many wav files.");
                    }

                    // Añadimos BACKUP IDX
                    if (Files_BACKUP_IDX.Length == 1)
                    {
                        ToLog("[+] BACKUP_IDX file: " + Path.GetFileName(Files_BACKUP_IDX[0]));
                        dt.Rows.Add(AddFileToGrid(Files_BACKUP_IDX[0]));
                        containerList.Add(new FileItems(Files_BACKUP_IDX[0], FileItems.SegmentType.backup_idx));
                    }
                    else
                    {
                        if (Files_BACKUP_IDX.Length == 0)
                            ToLog("[--] BACKUP_IDX file: Not found.");
                        else
                            ToLog("[--] BACKUP_IDX file: Too many wav files.");
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    //Finally Add DataTable into DataGridView
                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["File_Size"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridView1.Columns["File_Size"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridView1.Columns["Real_Filename"].Visible = false;
                }
            }
            else
                ToLog("[Error] Folder not found.");
            try
            {
                dataGridView1.Columns[0].Width = 150;
                dataGridView1.Columns[1].Width = 80;
                dataGridView1.Columns[2].Width = 74;
                dataGridView1.Columns[3].Width = 70;
                dataGridView1.Columns[4].Width = 150;
            }
            catch (Exception)
            {
            }
            return true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK)
                pathText.Text = fbd.SelectedPath;
            else
                pathText.Text = "";
        }

        void Go()
        {
            ClearLog();
            ToLog("[+] Getting files.");
            string path = pathText.Text;

            if (path.Length > 0)
            {
                if (FillDataGrid(path))
                    Rebuild(containerList, path + "/rebuild.arf");
                else
                    ToLog("[--] Required files not found.");
            }
            else
                ToLog("[--] Select a path.");
        }

        public byte[] GetBytes<T>(T str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClearLog();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Go();
        }
    }

    public class FileItems
    {
        public enum SegmentType
        {
            chat =      0x70100,
            file =      0x70103,
            cfg =       0x70112,
            video =     0x7010c,
            video_idx = 0x7010d,
            wav =       0x70105,
            mmfin =     0x70114,
            mmfin_idx = 0x70115,
            mmfin_cad = 0x7010A,
            mmfin_cai = 0x7010B,
            //          new code _20_ dat idx: probably sound
            snd =       0x7010E,
            snd_idx =   0x7010F,
            backup =    0x70110,
            backup_idx =0x70111

        };

        public string fileName;
        public SegmentType id;
        public FileItems(string filename, SegmentType ts)
        {
            fileName = filename;
            id = ts;
        }
    }

    public struct ARF_HEADER
    {    
        public UInt32 e_magic;                // Magic number 
        public UInt32 e_unknown;              // Posible ID
        public UInt32 e_filesize;             // File size
        public UInt32 e_reserved0;            // 
        public UInt32 e_nsections;            // Number of sections
        public UInt32 e_reserved1;            // 
        
    }

    public struct ARF_ITEMS
    {
        public UInt32 e_id;                
        public UInt32 e_index;               
        public UInt32 e_sectionlen;              
        public UInt32 e_reserved1;               
        public UInt32 e_sectionoffset;           
        public UInt32 e_reserved2;               
        public UInt32 e_reserved3;               
        public UInt32 e_reserved4;
    }
}