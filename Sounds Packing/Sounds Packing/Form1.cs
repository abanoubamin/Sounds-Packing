using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;

using System.Diagnostics;

namespace sound_packing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /*Symbols: (N) -> Number of Audio Files 
                   (M) -> Number of Folders or (Nodes).*/

        //Global Variables
        List<string> names = new List<string>();
        List<double> durations = new List<double>();

        static string sourceDir;

        static string destinationDir;
        static string output;

        double duration_each_folder;

        static string folder_name;

        /*Time function for time partitioning
         Usage: used to transfer total time in seconds to the formal format (00:00:00).
         Complexity: 𝞱(1) , because it consists of only if conditions.*/
        static string time(double total_seconds)
        {
            int hours = (int)total_seconds / 3600;
            int minutes = (int)total_seconds / 60;
            int seconds = (int)total_seconds % 60;

            string Time = hours.ToString();
            if (hours > 9)//𝞱(1)
                Time = hours.ToString();
            else if (hours < 10)//𝞱(1)
                Time = "0" + hours;
            if (minutes > 9)//𝞱(1)
                Time += ":" + minutes;
            else if (minutes < 10)//𝞱(1)
                Time += ":0" + minutes;
            if (seconds > 9)//𝞱(1)
                Time += ":" + seconds;
            else if (seconds < 10)//𝞱(1)
                Time += ":0" + seconds;

            return Time;//𝞱(1)
        }

        //Select the text file button
        private void button1_Click(object sender, EventArgs e)//𝞱(N)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "TXT files|*.txt";
            OFD.Title = "Select the txt file";
            OFD.Multiselect = false;

            if (OFD.ShowDialog() == DialogResult.OK)//𝞱(N)
            {
                names.Clear();
                durations.Clear();

                string source = OFD.FileName;
                string[] lines = File.ReadAllLines(source);
                string count = lines[0];
                for (int i = 0; i < lines.Length - 1; i++)//overall Complexity:𝞱(N),Number of iteration N
                {
                    string line = lines[i + 1];
                    names.Add(line.Split(' ')[0]);
                    string dur = line.Split(' ')[1];
                    TimeSpan TS = TimeSpan.Parse(dur);
                    durations.Add(TS.TotalSeconds);
                }
                MessageBox.Show(count + " Files");
            }
        }

        //Select the folder button
        private void button8_Click(object sender, EventArgs e)//𝞱(1)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();

            if (FBD.ShowDialog() == DialogResult.OK)//𝞱(1)
            {
                sourceDir = Path.GetFullPath(FBD.SelectedPath);
                MessageBox.Show("You selected folder: " + Path.GetFileName(sourceDir));
            }
        }

        //Select the folder to copy button
        private void button7_Click(object sender, EventArgs e)//𝞱(1)
        {
            FolderBrowserDialog FBD2 = new FolderBrowserDialog();

            if (FBD2.ShowDialog() == DialogResult.OK)//𝞱(1)
            {
                destinationDir = Path.GetDirectoryName(FBD2.SelectedPath);
                destinationDir = Path.Combine(destinationDir, Path.GetFileName(FBD2.SelectedPath));
                destinationDir = Path.Combine(destinationDir, "OUTPUT");
                output = destinationDir;
            }

            if (!Directory.Exists(destinationDir))//𝞱(1)
            {
                Directory.CreateDirectory(destinationDir);
            }

            MessageBox.Show("Done !");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Enter the duration of each folder button
        private void button3_Click(object sender, EventArgs e)//𝞱(1)
        {
            duration_each_folder = double.Parse(textBox1.Text);//𝞱(1)
            MessageBox.Show("Duration of each folder is : " + duration_each_folder.ToString());
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        //->Create Folder Function --------------------------------------------------------------------------------------------------------------
        /*Usage: Creates the path of the destination folder.
         Complexity: 𝞱(1) , because it has only if conditions to check if this path exists or not.*/
        void Create(string FN)
        {
            destinationDir = output;
            destinationDir = Path.Combine(destinationDir, FN);
            if (Directory.Exists(destinationDir))//𝞱(1)
            {
                MessageBox.Show("You have used WorstFit Algorithm before !");
            }
            else//𝞱(1)
            {
                Directory.CreateDirectory(destinationDir);
            }
        }
        //-->Generating MetaFiles Function ------------------------------------------------------------------------------------------------------
        /*Usage: Creates the text files(MetaData) that has the name and the duration of the copied audio files.
         Complexity: 𝞱(N+M) , because it has a nested for loop,
         the number of iterations of the outer loop is a number of folders (M),
         and the number of iterations of the inner for loop is a number of audio files (N)
         and has a calling to time() function that has a complexity 𝞱(1) .*/
        void METADATA(List<folder> folders)
        {
            //Stopwatch StpW = Stopwatch.StartNew();

            //The number of iterations of the outer loop is a number of folders "M" -> 𝞱(M) complexity
            for (int i = 0; i < folders.Count; i++)
            {
                string METADATA = Path.Combine(destinationDir, folders[i].folder_name + "_METADATA.txt");
                FileStream FS = new FileStream(METADATA, FileMode.Create);
                StreamWriter SW = new StreamWriter(FS);
                SW.WriteLine(folders[i].folder_name);

                string timeString;
                string metadataString;
                double total_time_each_folder;

                //The number of iterations of the inner for loop is a number of audio files "N" -> 𝞱(N) complexity
                for (int j = 0; j < folders[i].files_Durations.Count; j++)
                {
                    timeString = time(folders[i].files_Durations[j]);
                    metadataString = folders[i].files_names[j] + " " + timeString;
                    SW.WriteLine(metadataString);
                }

                total_time_each_folder = (duration_each_folder - folders[i].totalremainduration);
                timeString = time(total_time_each_folder);
                SW.WriteLine(timeString);
                SW.Close();
                FS.Close();
            }

            //StpW.Stop();
            //MessageBox.Show(StpW.Elapsed.ToString());

        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //--->Copying the Files into the Generated Directories Function -------------------------------------------------------------------------
        /*Usage: Copies the audio files into the destination folder.
         Complexity: 𝞱(N+M) , because it has a nested for loop,
         the number of iterations of the outer loop is a number of folders (M),
         and the number of iterations of the inner for loop is a number of audio files (N).*/
        void Copy(List<folder> folders)
        {
            //The number of iterations of the outer loop is a number of folders "M" -> 𝞱(M) complexity
            for (int i = 0; i < folders.Count; i++)
            {
                string tmp_out = Path.Combine(destinationDir, folders[i].folder_name);
                if (!Directory.Exists(tmp_out))//𝞱(1)
                {
                    Directory.CreateDirectory(tmp_out);
                }

                //The number of iterations of the inner for loop is a number of audio files "N" -> 𝞱(N) complexity
                for (int j = 0; j < folders[i].files_Durations.Count; j++)
                {
                    string Imp3 = Path.Combine(sourceDir, folders[i].files_names[j]);
                    string Omp3 = Path.Combine(tmp_out, folders[i].files_names[j]);
                    File.Copy(Imp3, Omp3, true);
                }
            }
        }
        //1.Worst-fit algorithm using Linear Search. --------------------------------------------------------------------------------------------
        /*Usage: Used to get the index of the folder that has a max remaining time.
         Complexity: 𝞱(M) , because it has a for loop that has the number of iterations is the number of folders (M).*/
        int maxfolder(List<folder> tmpfolders)
        {
            double tmp = tmpfolders[0].totalremainduration;
            int index = 0;
            //The number of iterations is the number of folders "M" -> 𝞱(M)
            for (int i = 1; i < tmpfolders.Count; i++)
            {
                if (tmpfolders[i].totalremainduration > tmp)//𝞱(1)
                {
                    tmp = tmpfolders[i].totalremainduration;
                    index = i;
                }              
            }
            return index;
        }
        //Button: Worst-fit algorithm using Linear Search.
        //Overall complexity: for loop(𝞱(N*M))+Create(𝞱(1))+MetaData(𝞱(N+M))+Copy(𝞱(N+M)) = O(N*M).
        private void button2_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            List<folder> folders = new List<folder>();
            folder tempfolder = new folder();
            tempfolder.folder_name = "F1";
            tempfolder.totalremainduration = duration_each_folder - durations[0];
            tempfolder.number_files = 1;
            tempfolder.files_names.Add(names[0]);
            tempfolder.files_Durations.Add(durations[0]);
            folders.Add(tempfolder);
            for (int i = 1; i < durations.Count; i++)//Overall Complexity:𝞱(N*M),Number of the iteration=N
            {
                int max_folder_index = maxfolder(folders);//𝞱(M)
                if (durations[i] <= folders[max_folder_index].totalremainduration)//𝞱(1)
                {
                    folders[max_folder_index].totalremainduration -= durations[i];
                    folders[max_folder_index].files_names.Add(names[i]);
                    folders[max_folder_index].files_Durations.Add(durations[i]);
                    folders[max_folder_index].number_files++;
                }
                else//𝞱(1)
                {
                    folder tempfolder2 = new folder();
                    tempfolder2.folder_name = "F" + (folders.Count + 1).ToString();
                    tempfolder2.totalremainduration = duration_each_folder - durations[i];
                    tempfolder2.files_names.Add(names[i]);
                    tempfolder2.files_Durations.Add(durations[i]);
                    tempfolder2.number_files = 1;
                    folders.Add(tempfolder2);
                }
            }

            string FolderName = "[1] WorstFit";
            Create(FolderName);//𝞱(1)

            METADATA(folders);//𝞱(N+M)

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

            Copy(folders);//𝞱(N+M)

            MessageBox.Show("Done !");

        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //2.Worst-fit algorithm using Priority queue.  ------------------------------------------------------------------------------------------

        public class max_binary_heap
        {
            public Node root;
            public Node insert_pos;
            public Queue<Node> ins;

            public max_binary_heap(Node N)//Constructor of the class used to create root,Complexity:𝞱(1)
            {
                ins = new Queue<Node>();
                root = N;
                insert_pos = N;
                ins.Enqueue(N);
            }
            /*Usage: This function is called to insert node in its correct  position in the priority queue
              based on the total remaining time of the created node.*/
            //Overall Complexity: Max(First Condition:O(log m),Second Condition:O(log m)) = O(log m)
            public void insert(Node node)
            {
                insert_pos = ins.First();
                if (insert_pos.left == null)//Overall Complexity:O(log m)
                {
                    insert_pos.left = node;
                    node.parent = insert_pos;
                    ins.Enqueue(node);
                    max_heapify(node);//O(log m)
                    return;
                }
                else//Overall Complexity:O(log m)
                {
                    insert_pos.right = node;
                    node.parent = insert_pos;
                    ins.Enqueue(node);
                    ins.Dequeue();

                    max_heapify(node);//O(log m)
                    return;
                }
            }
            /*Usage: This function is used to modify the root position in the priority queue
              when the remaining time of the root is changed.*/
            /*Overall Complexity: O(log m),because the function calls itself from the top to bottom to move along  the height of the priority queue
              after changing the remaining time of the root to put it in the new correct position based on its new remaining time.*/
            public void max_heapify1(Node start)
            {
                if (start.left != null || start.right != null)
                {
                    Node max = start;
                    if (start.left != null && start.total_remaining_time < start.left.total_remaining_time)//𝞱(1)
                    {
                        max = start.left;
                    }
                    if (start.right != null && max.total_remaining_time < start.right.total_remaining_time)//𝞱(1)
                    {
                        max = start.right;
                    }
                    if (max != start)//O(log M)
                    {
                        double temp = start.total_remaining_time;
                        start.total_remaining_time = max.total_remaining_time;
                        max.total_remaining_time = temp;

                        string temp2 = start.name;
                        start.name = max.name;
                        max.name = temp2;

                        List<string> temp3 = new List<string>(start.list_names);
                        start.list_names = max.list_names;
                        max.list_names = temp3;

                        List<double> temp4 = new List<double>(start.list_durations);
                        start.list_durations = max.list_durations;
                        max.list_durations = temp4;

                        max_heapify1(max);//O(1og m)
                    }
                }
            }
            /*Usage: This function is used to put an inserted node (folder) at its correct position 
              in the priority queue based on the remaining time of inserted node.*/
            /*Overall Complexity: O(log m),because it moves from bottom to top to move along the height of the priority queue 
              after inserting a new node to put it in the correct position.*/
            private void max_heapify(Node node)
            {
                while (node.parent != null)//O(log m)
                {
                    if (node == null)//𝞱(1)
                    {
                        break;
                    }

                    if (node.parent.total_remaining_time < node.total_remaining_time)//𝞱(1)
                    {
                        double temp1 = node.parent.total_remaining_time;
                        node.parent.total_remaining_time = node.total_remaining_time;
                        node.total_remaining_time = temp1;
                        string temp2 = node.parent.name;
                        node.parent.name = node.name;
                        node.name = temp2;
                        List<string> temp3 = new List<string>(node.parent.list_names);
                        node.parent.list_names = node.list_names;
                        node.list_names = temp3;
                        List<double> temp4 = new List<double>(node.parent.list_durations);
                        node.parent.list_durations = node.list_durations;
                        node.list_durations = temp4;
                        node = node.parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            /*Usage: This function is used to have access to all inserted audio files in all nodes to create the MetaData,
              we visit each node of the queue exactly once.*/
            /*Overall Complexity: 𝞱(N+M),because we move to each node exactly once and the audio files are distributed over nodes 
              which make us to make sure that the complexity will be exactly(N+M) and it has a calling to function time that has a complexity 𝞱(1).*/
            public void traverse(double duration)
            {
                destinationDir = output;
                destinationDir = Path.Combine(destinationDir, folder_name);
                if (Directory.Exists(destinationDir))//𝞱(1)
                {
                    MessageBox.Show("You have used WorstFit Algorithm before !");
                }
                else//𝞱(1)
                {
                    Directory.CreateDirectory(destinationDir);
                }

                //Stopwatch StpW = Stopwatch.StartNew();

                Queue<Node> q = new Queue<Node>();
                q.Enqueue(root);
                while (q.Count > 0)//𝞱(N+M)
                {
                    Node node = q.Dequeue();

                    string METADATA = Path.Combine(destinationDir, node.name + "_METADATA.txt");
                    FileStream FS = new FileStream(METADATA, FileMode.Create);
                    StreamWriter SW = new StreamWriter(FS);
                    SW.WriteLine(node.name);

                    string timeString;
                    string metadataString;
                    double total_time_each_folder;

                    for (int i = 0; i < node.list_names.Count(); i++)//𝞱(N),N is a number of audio files of a specific folder.
                    {
                        timeString = time(node.list_durations[i]);
                        metadataString = node.list_names[i] + "  " + timeString;
                        SW.WriteLine(metadataString);
                    }

                    total_time_each_folder = (duration - node.total_remaining_time);
                    timeString = time(total_time_each_folder);
                    SW.WriteLine(timeString);
                    SW.Close();
                    FS.Close();

                    if (node.left != null)//𝞱(1)
                        q.Enqueue(node.left);
                    if (node.right != null)//𝞱(1)
                        q.Enqueue(node.right);
                }

                //StpW.Stop();
                //MessageBox.Show(StpW.Elapsed.ToString());

            }
            /*Usage: This function is used to have access to all inserted audio files in all nodes to create the folders,
              we visit each node of the queue exactly once.*/
            /*Overall Complexity: 𝞱(N+M),because we move to each node exactly once and the audio files are distributed over nodes 
            which make us to make sure that the complexity will be exactly(N+M).*/
            public void traverse_Copy(double duration)
            {
                Queue<Node> q = new Queue<Node>();
                q.Enqueue(root);
                while (q.Count > 0)//𝞱(N+M)
                {
                    Node node = q.Dequeue();

                    string temp_out = Path.Combine(destinationDir, node.name);
                    if (!Directory.Exists(temp_out))//𝞱(1)
                    {
                        Directory.CreateDirectory(temp_out);
                    }

                    for (int i = 0; i < node.list_names.Count(); i++)//𝞱(N),N is a number of audio files of a specific folder.
                    {
                        string imp3 = Path.Combine(sourceDir, node.list_names[i]);
                        string omp3 = Path.Combine(temp_out, node.list_names[i]);
                        File.Copy(imp3, omp3, true);
                    }

                    if (node.left != null)//𝞱(1)
                        q.Enqueue(node.left);
                    if (node.right != null)//𝞱(1)
                        q.Enqueue(node.right);
                }
            }
        }

        //Button: Worst-fit algorithm using Priority queue.
        //Overall Complexity: for loop(O(N log M))+traverse(𝞱(N+M))+traverse_copy(𝞱(N+M)) = O(N log M).
        private void button5_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            folder_name = "[1] WorstFit";

            int count = 2;
            double duration_of_each_file = double.Parse(textBox1.Text);
            Node node = new Node(duration_of_each_file - durations[0], "F1");
            node.list_names.Add(names[0]);
            node.list_durations.Add(durations[0]);
            max_binary_heap h = new max_binary_heap(node);
            for (int i = 1; i < durations.Count(); i++)//Overall Complexity:O(N log M),Number of iteration=N
            {
                if (h.root.total_remaining_time >= durations[i])//Overall Complexity:O(log M)
                {
                    h.root.list_names.Add(names[i]);
                    h.root.list_durations.Add(durations[i]);
                    h.root.total_remaining_time -= durations[i];
                    h.max_heapify1(h.root);//O(log M)
                }
                else//Overall Complexity:O(log M)
                {
                    node = new Node(duration_of_each_file - durations[i], "F" + count.ToString());
                    node.list_names.Add(names[i]);
                    node.list_durations.Add(durations[i]);
                    count++;
                    h.insert(node);//O(log M)
                }
            }

            h.traverse(double.Parse(textBox1.Text));//𝞱(N+M)

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

            h.traverse_Copy(double.Parse(textBox1.Text));//𝞱(N+M)
            MessageBox.Show("Done !");
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //3.Worst-fit decreasing algorithm using Linear Search. ---------------------------------------------------------------------------------
        //Merge Sort Function: Used to sort the class Names_Durations which has list of names and list of durations of the audio files decreasing.
        //Overall Complexity: O(N log N)
        private static Names_Durations MergeSort(Names_Durations unsorted)
        {
            if (unsorted.duration.Count <= 1)
                return unsorted;

            Names_Durations left = new Names_Durations();
            Names_Durations right = new Names_Durations();

            int middle = unsorted.duration.Count / 2;
            for (int i = 0; i < middle; i++)  //Dividing the unsorted list
            {
                left.name.Add(unsorted.name[i]);
                left.duration.Add(unsorted.duration[i]);
            }
            for (int i = middle; i < unsorted.duration.Count; i++)
            {
                right.name.Add(unsorted.name[i]);
                right.duration.Add(unsorted.duration[i]);
            }

            left = MergeSort(left);
            right = MergeSort(right);
            return Merge(left, right);
        }

        private static Names_Durations Merge(Names_Durations left, Names_Durations right)
        {
            Names_Durations result = new Names_Durations();

            while (left.duration.Count > 0 || right.duration.Count > 0)
            {
                if (left.duration.Count > 0 && right.duration.Count > 0)
                {
                    if (left.duration.First() >= right.duration.First())  //Comparing First two elements to see which is bigger
                    {
                        result.duration.Add(left.duration.First());
                        result.name.Add(left.name.First());
                        left.name.Remove(left.name.First());//Rest of the list minus the first element
                        left.duration.Remove(left.duration.First());
                    }
                    else
                    {
                        result.duration.Add(right.duration.First());
                        result.name.Add(right.name.First());
                        right.name.Remove(right.name.First());//Rest of the list minus the first element
                        right.duration.Remove(right.duration.First());
                    }
                }
                else if (left.duration.Count > 0)
                {
                    result.duration.Add(left.duration.First());
                    result.name.Add(left.name.First());
                    left.duration.Remove(left.duration.First());
                    left.name.Remove(left.name.First());
                }
                else if (right.duration.Count > 0)
                {
                    result.duration.Add(right.duration.First());
                    result.name.Add(right.name.First());
                    right.duration.Remove(right.duration.First());
                    right.name.Remove(right.name.First());
                }
            }
            return result;
        }

        //Button: Worst-fit decreasing algorithm using Linear Search.
        //Overall Complexity: First for loop(𝞱(N))+Max(Merge Sort(O(N log N)),Second for loop(𝞱(N*M))+Create(𝞱(1))+MetaData(𝞱(N+M))+Copy(𝞱(N+M))) =.O(Max(N log N,N*M)).      
        private void button4_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            Names_Durations list = new Names_Durations();
            for (int i = 0; i < durations.Count; i++)//First for loop:𝞱(N)
            {
                list.name.Add(names[i]);
                list.duration.Add(durations[i]);
            }
            Names_Durations NewList = MergeSort(list);//O(N log N)

            List<folder> folders = new List<folder>();
            folder tempfolder = new folder();
            tempfolder.folder_name = "F1";
            tempfolder.totalremainduration = duration_each_folder - NewList.duration[0];
            tempfolder.number_files = 1;
            tempfolder.files_names.Add(NewList.name[0]);
            tempfolder.files_Durations.Add(NewList.duration[0]);
            folders.Add(tempfolder);
            for (int i = 1; i < NewList.duration.Count; i++)//Overall Complexity of the Second for loop:𝞱(N*M),Number of the iteration=N
            {
                int max_folder_index = maxfolder(folders);//𝞱(M)
                if (NewList.duration[i] <= folders[max_folder_index].totalremainduration)//𝞱(1)
                {
                    folders[max_folder_index].totalremainduration -= NewList.duration[i];
                    folders[max_folder_index].files_names.Add(NewList.name[i]);
                    folders[max_folder_index].files_Durations.Add(NewList.duration[i]);
                    folders[max_folder_index].number_files++;
                }
                else//𝞱(1)
                {
                    folder tempfolder2 = new folder();
                    tempfolder2.folder_name = "F" + (folders.Count + 1).ToString();
                    tempfolder2.totalremainduration = duration_each_folder - NewList.duration[i];
                    tempfolder2.files_names.Add(NewList.name[i]);
                    tempfolder2.files_Durations.Add(NewList.duration[i]);
                    tempfolder2.number_files = 1;
                    folders.Add(tempfolder2);
                }
            }

            string FolderName = "[2] WorstFit Decreasing";
            Create(FolderName);//𝞱(1) 

            METADATA(folders);//𝞱(N+M) 

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

            Copy(folders);//𝞱(N+M) 

            MessageBox.Show("Done !");

        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //4.Worst-fit decreasing algorithm using Priority queue. --------------------------------------------------------------------------------
        //Button: Worst-fit decreasing algorithm using Priority queue.
        /*Overall Complexity:First for loop(𝞱(N))+Max(Merge Sort(O(N log N)),Second for loop(𝞱(N log M))+traverse(𝞱(N+M))+traverse_copy(𝞱(N+M)))
                             =𝞱(N)+Max(N log N,N log M)=O(N log N) */
        private void button6_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            folder_name = "[2] WorstFit Decreasing";

            Names_Durations list = new Names_Durations();
            for (int i = 0; i < durations.Count; i++)//First for loop:𝞱(N)
            {
                list.name.Add(names[i]);
                list.duration.Add(durations[i]);
            }
            Names_Durations NewList = MergeSort(list);//O(N log N)

            int count = 2;
            double duration_of_each_file = double.Parse(textBox1.Text);
            Node node = new Node(duration_of_each_file - NewList.duration[0], "F1");
            node.list_names.Add(NewList.name[0]);
            node.list_durations.Add(NewList.duration[0]);
            max_binary_heap h = new max_binary_heap(node);
            for (int i = 1; i < NewList.duration.Count; i++)//Overall Complexity of the Second for loop:𝞱(N log M),Number of iteration = N
            {
                if (h.root.total_remaining_time >= NewList.duration[i])//Overall Complexity:𝞱(log M)
                {
                    h.root.list_names.Add(NewList.name[i]);
                    h.root.list_durations.Add(NewList.duration[i]);
                    h.root.total_remaining_time -= NewList.duration[i];
                    h.max_heapify1(h.root);//𝞱(log M)
                }
                else//Overall Complexity:𝞱(log M)
                {
                    node = new Node(duration_of_each_file - NewList.duration[i], "F" + count.ToString());
                    node.list_names.Add(NewList.name[i]);
                    node.list_durations.Add(NewList.duration[i]);
                    count++;
                    h.insert(node);//𝞱(log M)
                }
            }
           // h.traverse(double.Parse(textBox1.Text));//𝞱(N+M)

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

           // h.traverse_Copy(double.Parse(textBox1.Text));//𝞱(N+M)
            MessageBox.Show("Done !");
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //5.First-fit decreasing algorithm using Linear Search. ---------------------------------------------------------------------------------
        //Button: First-fit decreasing algorithm using Linear Search.
        /*Overall Complexity: First for loop(𝞱 (N))+ 𝞱 (Max(Merge(O(N log N)),Second for loop(𝞱(N*M))))+Create(𝞱(1))+METADATA(𝞱(N+M))+Copy(𝞱(N+M)) =
                             =𝞱 (Max(Merge(𝞱(N log N)),Second for loop(𝞱(N*M)))) = O(Max(N log N,N*M)).*/
        private void button9_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            Names_Durations list = new Names_Durations();
            for (int i = 0; i < durations.Count; i++)//First for loop:𝞱 (N)
            {
                list.name.Add(names[i]);
                list.duration.Add(durations[i]);
            }
            Names_Durations NewList = MergeSort(list);//O(N log N)

            List<folder> folders = new List<folder>();
            folder folder1 = new folder();
            folder1.folder_name = "F1";
            folder1.totalremainduration = duration_each_folder - NewList.duration[0];
            folder1.number_files = 1;
            folder1.files_names.Add(NewList.name[0]);
            folder1.files_Durations.Add(NewList.duration[0]);
            folders.Add(folder1);
            for (int i = 1; i < NewList.duration.Count; i++)//Secong for loop:𝞱(N*M),Number of iteration= N
            {
                bool flag = false;

                for (int j = 0; j < folders.Count; j++)//𝞱(M),Number of iteration M
                {
                    if (NewList.duration[i] <= folders[j].totalremainduration)//𝞱(1)
                    {
                        folders[j].totalremainduration -= NewList.duration[i];
                        folders[j].files_names.Add(NewList.name[i]);
                        folders[j].files_Durations.Add(NewList.duration[i]);
                        folders[j].number_files++;
                        flag = true;
                        break;
                    }
                }

                if (flag == false)//𝞱(1)
                {
                    folder New_folder = new folder();
                    New_folder.folder_name = "F" + (folders.Count + 1).ToString();
                    New_folder.totalremainduration = duration_each_folder - NewList.duration[i];
                    New_folder.files_names.Add(NewList.name[i]);
                    New_folder.files_Durations.Add(NewList.duration[i]);
                    New_folder.number_files = 1;
                    folders.Add(New_folder);
                    flag = true;
                }
                
            }

            string FolderName = "[3] FirstFit Decreasing";
            Create(FolderName);//𝞱(1)

            METADATA(folders);//𝞱(N+M)

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

            Copy(folders);//𝞱(N+M)

            MessageBox.Show("Done !");

        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //6.Folder filling algorithm. -----------------------------------------------------------------------------------------------------------
        public static Names_Durations exactlySum(ref Names_Durations items, int N, int D)
        {
            Names_Durations answer = new Names_Durations();
            Names_Durations NewItems = new Names_Durations();
            N = items.duration.Count;
            bool[,] dp = new bool[N + 1, D + 1];
            dp[0, 0] = true;
            if (items.duration[0] <= D) dp[0, (int)items.duration[0]] = true;//𝞱(1)
            for (int i = 1; i < N; i++) //O(N*D) where D is duration of  each folder 
                                         // N is number of audio files
             {
                for (int j = 0; j <= D; j++) //𝞱(D) 
                {
                    if (dp[i - 1, j] == true) dp[i, j] = true;
                    if (dp[i - 1, j] == true && items.duration[i] + j <= D) dp[i, (int)(j + items.duration[i])] = true;
                }
            }
            int K = 0;
            for (int j = D; j >= 0; j--)//O(D)
            {
                if (dp[N - 1, j] == true)
                {
                    K = j;
                    break;
                }
            }
            for (int i = N - 1; i >= 1; i--)//𝞱(N)
            {
                if (K - items.duration[i] >= 0)
                {
                    if (dp[i - 1, (int)(K - items.duration[i])] == true)
                    {
                        K -= (int)items.duration[i];
                        answer.duration.Add(items.duration[i]);
                        answer.name.Add(items.name[i]);
                    }
                    else
                    {
                        NewItems.duration.Add(items.duration[i]);
                        NewItems.name.Add(items.name[i]);
                    }
                }
                else
                {
                    NewItems.duration.Add(items.duration[i]);
                    NewItems.name.Add(items.name[i]);
                }
            }
            if (K != 0)//𝞱(1)
            {
                answer.duration.Add(items.duration[0]);
                answer.name.Add(items.name[0]);
            }
            else   //𝞱(1)
            {
                NewItems.duration.Add(items.duration[0]);
                NewItems.name.Add(items.name[0]);
            }
            items = NewItems; 
            return answer;  
        }
        //folder filling algorithm
        /*
          -complexity of exactlySum 𝞱(N*D)
          -exactlySum is called inside while loop when there is more audios
          -overall of folder filling of while loop inside folder filling button
          -complexity 𝞱(D*N^2)
          -Where D is duration of each folder and N is number of audio files
        */
        private void button10_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            Names_Durations obj = new Names_Durations();
            for (int i = 0; i < durations.Count; i++)//𝞱(N)
            {
                obj.name.Add(names[i]);
                obj.duration.Add(durations[i]);
            }

            List<folder> folders = new List<folder>();
            while (true) //O(N^2*D)
            {
                if (obj.duration.Count == 0) break;//𝞱(1)
                Names_Durations answer = exactlySum(ref obj, obj.duration.Count, (int)duration_each_folder);//𝞱(N*D)
                if (answer.duration.Count == 0) break;//𝞱(1)

                folder tempfolder2 = new folder();
                tempfolder2.folder_name = "F" + (folders.Count + 1).ToString();
                tempfolder2.totalremainduration = 0;
                tempfolder2.totalremainduration = duration_each_folder;
                for (int i = 0; i < answer.duration.Count; i++)//O(N)
                {
                    tempfolder2.files_names.Add(answer.name[i]);
                    tempfolder2.files_Durations.Add(answer.duration[i]);
                    tempfolder2.totalremainduration -= answer.duration[i];
                }
                folders.Add(tempfolder2);
            }

            string FolderName = "[4] FolderFilling";
            Create(FolderName);

            METADATA(folders);//𝞱(N+M)

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

            Copy(folders); //𝞱(N+M)

            MessageBox.Show("Done !");

        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //7.Best fit strategy. ------------------------------------------------------------------------------------------------------------------
        //Usage: Used to get the index of the folder that has a minimum remaining time where the audio file fits in it.
        //Overall Complexity: Max(First condition:M,Second condition:M)) = 𝞱(M).
        int minfolder(List<folder> tmpfolders, double duration)
        {
            double tmp = tmpfolders[0].totalremainduration;
            int index = 0;
            if (tmp < duration)//O(M)
            {
                for (int i = 1; i < tmpfolders.Count; i++)//Overall Complexity:O(M),Number of iteration = M
                {
                    if (tmpfolders[i].totalremainduration > tmp && duration <= tmpfolders[i].totalremainduration)//𝞱(1)
                    {
                        tmp = tmpfolders[i].totalremainduration;
                        index = i;
                        break;
                    }
                }
            }
            if (tmp >= duration)//𝞱(M)
            {
                for (int i = 0; i < tmpfolders.Count; i++)//Overall Complexity:𝞱(M),Number of iteration = M
                {
                    if (duration <= tmpfolders[i].totalremainduration && tmpfolders[i].totalremainduration < tmp)//𝞱(1)
                    {
                        index = i;
                    }
                }
            }
            return index;
        }
        //Button: Best fit algorithm.
        /*Overall Complexity:first for loop(𝞱(N))+Max(Merge Sort(O(N log N)),second for loop(𝞱(N*M))+Create(𝞱(1))+METADATA(𝞱(N+M))+Copy(𝞱(N+M)))=
                             = O(Max(N log N,N*M)).*/
        private void button11_Click(object sender, EventArgs e)
        {
            Stopwatch StpW = Stopwatch.StartNew();

            Names_Durations list = new Names_Durations();
            for (int i = 0; i < durations.Count; i++)//𝞱(N)
            {
                list.name.Add(names[i]);
                list.duration.Add(durations[i]);
            }
            Names_Durations NewList = MergeSort(list);//O(N log N)

            List<folder> folders = new List<folder>();
            folder tempfolder = new folder();
            tempfolder.folder_name = "F1";
            tempfolder.totalremainduration = duration_each_folder - NewList.duration[0];
            tempfolder.number_files = 1;
            tempfolder.files_names.Add(NewList.name[0]);
            tempfolder.files_Durations.Add(NewList.duration[0]);
            folders.Add(tempfolder);
            for (int i = 1; i < NewList.duration.Count; i++)//Second for loop:𝞱(N*M),Number of iteration=N
            {
                int min_folder_index = minfolder(folders, NewList.duration[i]);//𝞱(M)
                if (NewList.duration[i] <= folders[min_folder_index].totalremainduration)//𝞱(1)
                {
                    folders[min_folder_index].totalremainduration -= NewList.duration[i];
                    folders[min_folder_index].files_names.Add(NewList.name[i]);
                    folders[min_folder_index].files_Durations.Add(NewList.duration[i]);
                    folders[min_folder_index].number_files++;
                }
                else//𝞱(1)
                {
                    folder tempfolder2 = new folder();
                    tempfolder2.folder_name = "F" + (folders.Count + 1).ToString();
                    tempfolder2.totalremainduration = duration_each_folder - NewList.duration[i];
                    tempfolder2.files_names.Add(NewList.name[i]);
                    tempfolder2.files_Durations.Add(NewList.duration[i]);
                    tempfolder2.number_files = 1;
                    folders.Add(tempfolder2);
                }
            }

            string FolderName = "[5] BestFit";
            Create(FolderName);//𝞱(1)

            METADATA(folders);//𝞱(N+M)

            StpW.Stop();
            MessageBox.Show(StpW.Elapsed.ToString());

            Copy(folders);//𝞱(N+M)

            MessageBox.Show("Done !");

        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
    }
}
